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

public class UnityAssetParser
{

    public static Individual[] ParseIndividualsFromFolder(string folderPathJSON, int evalRangeStart, int evalRangeEnd)
    {
        Individual[] individuals = null;

        // Read all files in the folder
        string[] files = System.IO.Directory.GetFiles(folderPathJSON, "*.json");
        files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)").Groups[0].ToString())).ToArray();

        if(evalRangeStart < 0)
        {
            evalRangeStart = 0;
        }

        if(evalRangeEnd < 0 || evalRangeEnd > files.Length)
        {
            evalRangeEnd = files.Length;
        }

        individuals = new Individual[evalRangeEnd - evalRangeStart];

        for (int i = evalRangeStart; i < evalRangeEnd; i++)
        {
            individuals[i] = JsonConvert.DeserializeObject<Individual>(System.IO.File.ReadAllText(files[i]), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
        }

        if (individuals.Length == 0)
        {
            throw new Exception("No individuals were loaded from the IndividualsSource");
            // TODO Add error reporting here
        }

        return individuals;
    }

    public static void SaveIndividualsToFolder(Individual[] individuals, string folderPath)
    {
#if UNITY_EDITOR
        // Save individuals to folder
        foreach (Individual individual in individuals)
        {
            string individualName = individual.name;
            string individualSOPath = folderPath + "\\" + individualName + ".asset";
            AssetDatabase.CreateAsset(individual, individualSOPath);

            foreach(AgentController agentController in individual.AgentControllers)
            {
                AssetDatabase.AddObjectToAsset(agentController, individual);
                agentController.AddAgentControllerToSO(individual);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }
}