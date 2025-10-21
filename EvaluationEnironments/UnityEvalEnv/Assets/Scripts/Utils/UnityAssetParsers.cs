using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using AgentOrganizations;
using System.Text.RegularExpressions;
using AgentControllers;
using UnityEditor;
using Configuration;

namespace Utils
{
    public class UnityAssetParser
    {

        public static Individual[] ParseIndividualsFromFolder(string folderPathJSON, int evalRangeStart, int evalRangeEnd)
        {
            Individual[] individuals = null;

            // Read all files in the folder
            string[] files = Directory.GetFiles(folderPathJSON, "*.json");
            files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)(?!.*\d)").Groups[0].ToString())).ToArray();

            if (evalRangeStart < 0)
            {
                evalRangeStart = 0;
            }

            if (evalRangeEnd < 0 || evalRangeEnd > files.Length)
            {
                evalRangeEnd = files.Length;
            }

            individuals = new Individual[evalRangeEnd - evalRangeStart];

            int indx = 0;
            for (int i = evalRangeStart; i < evalRangeEnd; i++)
            {
                individuals[indx++] = JsonConvert.DeserializeObject<Individual>(File.ReadAllText(files[i]), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
            }

            if (individuals.Length == 0)
            {
                throw new Exception("No individuals were loaded from the IndividualsSource");
            }

            return individuals;
        }

        public static void SaveSOIndividualsToSO(Individual[] individuals, string folderPath)
        {
#if UNITY_EDITOR
            // Save individuals to folder (.asset)
            foreach (Individual individual in individuals)
            {
                string individualName = individual.name;
                string individualSOPath = folderPath + "\\" + individualName + ".asset";
                AssetDatabase.CreateAsset(individual, individualSOPath);

                foreach (AgentController agentController in individual.AgentControllers)
                {
                    AssetDatabase.AddObjectToAsset(agentController, individual);
                    agentController.AddAgentControllerToSO(individual);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public static void SaveSOIndividualsToJSON(Individual[] individuals, string folderPath)
        {
#if UNITY_EDITOR
            // Save individuals to folder (.json)
            foreach (Individual individual in individuals)
            {
                string individualName = individual.name;
                string individualSOPath = folderPath + "\\" + individualName + ".json";
                File.WriteAllText(individualSOPath, JsonConvert.SerializeObject(individual, MainConfiguration.JSON_SERIALIZATION_SETTINGS));
            }
#endif
        }
    }
}