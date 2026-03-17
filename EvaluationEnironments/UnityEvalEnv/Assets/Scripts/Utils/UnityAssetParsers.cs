using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using AgentOrganizations;
using System.Text.RegularExpressions;
using AgentControllers;
using UnityEditor;
using Configuration;
using UnityEngine;

namespace Utils
{
    public class UnityAssetParser
    {
        public static Individual[][] ParseIndividualsFromFolder(string folderPathJSON, EvalRange[] evalRanges)
        {
            int folderNum = (evalRanges != null && evalRanges.Length > 0) ? evalRanges.Length : Directory.GetDirectories(folderPathJSON).Length;

            Individual[][] individuals = new Individual[folderNum][];

            for (int i = 0; i < folderNum; i++)
            {
                string[] files = Directory.GetFiles(folderPathJSON + i + "\\", "*.json");
                files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)(?!.*\d)").Groups[0].ToString())).ToArray();

                int evalRangeStart = (evalRanges != null && evalRanges.Length > 0) ? evalRanges[i].Start : 0;
                int evalRangeEnd = (evalRanges != null && evalRanges.Length > 0) ? evalRanges[i].End : files.Length;

                if (evalRangeStart < 0)
                {
                    evalRangeStart = 0;
                }

                if (evalRangeEnd < 0 || evalRangeEnd > files.Length)
                {
                    evalRangeEnd = files.Length;
                }

                individuals[i] = new Individual[evalRangeEnd - evalRangeStart];

                int indx = 0;
                for (int j = evalRangeStart; j < evalRangeEnd; j++)
                {
                    individuals[i][indx++] = JsonConvert.DeserializeObject<Individual>(File.ReadAllText(files[j]), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
                }

                if (individuals[i].Length == 0)
                {
                    throw new Exception("No individuals were loaded from the IndividualsSource for group: " + i + "!");
                }
            }

            return individuals;
        }

        public static Individual[][] ParseIndividualsFromFolder(string folderPathJSON, int[][] evalIndividuals)
        {
            if(evalIndividuals == null || evalIndividuals.Length == 0)
            {
                throw new Exception("evalIndividuals parameter is null or empty!");
            }

            Individual[][] individuals = new Individual[evalIndividuals.Length][];

            for (int i = 0; i < evalIndividuals.Length; i++)
            {
                string[] files = Directory.GetFiles(folderPathJSON + i + "\\", "*.json");
                files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)(?!.*\d)").Groups[0].ToString())).ToArray();

                individuals[i] = new Individual[evalIndividuals[i].Length];

                int indx = 0;
                foreach (int j in evalIndividuals[i])
                {
                    // Find file that ends with the individual index (before .json extension)
                    string file = files.First(f => f.EndsWith(j + ".json"));
                    if(file == null)
                    {
                        throw new Exception("No file found for individual index: " + j + " in group: " + i);
                    }

                    individuals[i][indx++] = JsonConvert.DeserializeObject<Individual>(File.ReadAllText(file), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
                }

                if (individuals[i].Length == 0)
                {
                    throw new Exception("No individuals were loaded from the IndividualsSource for group: " + i);
                }
            }

            return individuals;
        }

        public static void SaveSOIndividualsToSO(Individual[][] individuals, string folderPath)
        {
#if UNITY_EDITOR
            // Save individuals to folder (.asset)
            for (int i = 0; i < individuals.Length; i++)
            {
                for (int j = 0; j < individuals[i].Length; j++)
                {
                    string individualName = individuals[i][j].name;
                    string individualSOPath = folderPath + "\\" + i + "\\" + individualName + ".asset";
                    if(!Directory.Exists(folderPath + "\\" + i))
                    {
                        Directory.CreateDirectory(folderPath + "\\" + i);
                    }

                    AssetDatabase.CreateAsset(individuals[i][j], individualSOPath);

                    foreach (AgentController agentController in individuals[i][j].AgentControllers)
                    {
                        AssetDatabase.AddObjectToAsset(agentController, individuals[i][j]);
                        agentController.AddAgentControllerToSO(individuals[i][j]);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public static void SaveSOIndividualsToJSON(Individual[][] individuals, string folderPath)
        {
#if UNITY_EDITOR
            // Save individuals to folder (.json)
            for (int i = 0; i < individuals.Length; i++)
            {
                string evalFolderPath = folderPath + "\\" + i;
                if (!Directory.Exists(evalFolderPath))
                {
                    Directory.CreateDirectory(evalFolderPath);
                }
                for (int j = 0; j < individuals[i].Length; j++)
                {
                    string individualName = individuals[i][j].name;
                    string individualJSONPath = evalFolderPath + "\\" + individualName + ".json";
                    File.WriteAllText(individualJSONPath, JsonConvert.SerializeObject(individuals[i][j], MainConfiguration.JSON_SERIALIZATION_SETTINGS));
                }
            }
#endif
        }
    }
}