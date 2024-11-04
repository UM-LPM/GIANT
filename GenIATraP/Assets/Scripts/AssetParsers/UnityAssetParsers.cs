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

    // TODO Remove in the future
    /*public static Individual[] ParseIndividualsFromFolder(string folderPathJSON)
    {
        Individual[] individuals = null;

        // Read all files in the folder
        string[] files = System.IO.Directory.GetFiles(folderPathJSON, "*.asset");
        files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)").Groups[0].ToString())).ToArray();

        individuals = new Individual[files.Length];

        YamlFile[] yamlFiles = YamlParser.ReadYamlFilesFromFolder(files);

        individuals = ParseIndividualsFromYamlFiles(yamlFiles);

        return individuals;
    }*/

    // TODO Remove in the future
    public static Individual[] ParseIndividualsFromYamlFiles(YamlFile[] yamlFiles)
    {
        Individual[] individuals = new Individual[yamlFiles.Length];

        for (int i = 0; i < yamlFiles.Length; i++)
        {
            individuals[i] = ParseIndividualFromYamlFile(yamlFiles[i]);
        }

        return individuals;
    }

    // TODO Remove in the future
    public static Individual ParseIndividualFromYamlFile(YamlFile yamlFile)
    {
        Individual individual = new Individual();

        // 1. Find the individual base def (Document where DocumentProperties contain property "AgentControllers")
        YamlDocument individualDef = yamlFile.Documents.Find(x => x.DocumentProperties.ContainsKey("AgentControllers"));

        // 2. Find the agent controllers (Get the list of agent controllers from the individual base def)
        List<Dictionary<string, object>> agentControllerFileIDs = individualDef.DocumentProperties["AgentControllers"] as List<Dictionary<string, object>>;

        // 3. For each agent controller, load the correct agent controller parser (agent controller can be of type BehaviourTreeAgentController, NeuralNetworkAgentController, etc.)
        List<AgentController> agentControllers = new List<AgentController>();
        foreach (Dictionary<string, object> agentControllerFileID in agentControllerFileIDs)
        {
            // 4. Parse the agent controllers
            agentControllers.Add(ParseAgentController(yamlFile, agentControllerFileID["fileID"].ToString()));
        }

        if(agentControllers.Count == 0)
        {
            throw new Exception("No agent controllers found in the individual");
            // TODO Add error reporting here
        }

        // 5. Set the individual agent controllers   
        individual.AgentControllers = agentControllers.ToArray();

        // 6. Return the individual
        return individual;
    }

    // TODO Remove in the future
    public static AgentController ParseAgentController(YamlFile yamlFile, string agentControllerFileID)
    {
        YamlDocument yamlDocumentACBase = yamlFile.Documents.Find(x => x.DocumentId == agentControllerFileID);
        
        if(yamlDocumentACBase == null)
        {
            throw new Exception("Agent controller not found in the yaml file");
            // TODO Add error reporting here
        }

        if(yamlDocumentACBase.DocumentProperties.ContainsKey("RootNode"))
        {
            return ParseBehaviourTreeAgentController(yamlFile, agentControllerFileID);
        }
        else if (yamlDocumentACBase.DocumentProperties.ContainsKey("ModelAsset"))
        {
            return ParseNeuralNetworkAgentController(yamlFile, agentControllerFileID);
        }
        else // TODO Add support in the future for other agent controller types
        {
            throw new NotImplementedException("Parsers for specified AgentController not implemented");
            // TODO Add error reporting here
        }
    }

    // TODO Remove in the future
    public static AgentController ParseBehaviourTreeAgentController(YamlFile yamlFile, string agentControllerFileID)
    {
        throw new NotImplementedException();
    }

    // TODO Remove in the future
    public static AgentController ParseNeuralNetworkAgentController(YamlFile yamlFile, string agentControllerFileID)
    {
        throw new NotImplementedException();
    }

    // TODO Remove in the future
    public static BehaviorTreeAgentController ParseBehaviourTree(string path)
    {
        List<BehaviourTreeNodeDef> behaviourTreeNodeDefs = new List<BehaviourTreeNodeDef>();

        BehaviourTreeNodeDef behaviourTreeNodeDef = null;
        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] lineArray = line.Split(':', 2);

                // Shared properties
                if (lineArray[0].Contains("--- !u!114"))
                {
                    // If we have a previous behaviour tree asset, add it to the list
                    if (behaviourTreeNodeDef != null)
                    {
                        behaviourTreeNodeDefs.Add(behaviourTreeNodeDef);
                    }

                    // Initialize new behaviour tree asset
                    string[] nodeHeaderArray = lineArray[0].Split(' ');
                    nodeHeaderArray[2] = nodeHeaderArray[2].Replace('&', ' ').Trim();

                    behaviourTreeNodeDef = new BehaviourTreeNodeDef(nodeHeaderArray[2]);
                }
                else if (lineArray[0].Contains("m_Script"))
                {
                    behaviourTreeNodeDef.m_Script = JsonConvert.DeserializeObject<MScript>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Contains("m_Name"))
                {
                    behaviourTreeNodeDef.m_Name = lineArray[1].Trim();
                }
                else if (lineArray[0].Contains("guid"))
                {
                    behaviourTreeNodeDef.guid = lineArray[1].Trim();
                }
                else if (lineArray[0].Contains("position"))
                {
                    behaviourTreeNodeDef.position = JsonConvert.DeserializeObject<Position>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Contains("Blackboard"))
                {
                    lineArray[1] = lineArray[1].Trim();
                    if (lineArray[1].Length > 0)
                    {
                        behaviourTreeNodeDef.blackboard = JsonConvert.DeserializeObject<Blackboard>(convertSringArrayToJson(lineArray[1].Trim()));
                    }
                }
                else if (lineArray[0].Contains("drawGizmos"))
                {
                    behaviourTreeNodeDef.drawGizmos = int.Parse(lineArray[1].Trim());
                }
                /////// Behaviour tree properties START
                else if (lineArray[0].Contains("Nodes"))
                {
                    behaviourTreeNodeDef.nodes = GetNodeList(reader);
                }
                else if (lineArray[0].Trim().Equals("Id"))
                {
                    behaviourTreeNodeDef.bt_properties.Add("Id", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("RootNode"))
                {
                    behaviourTreeNodeDef.bt_properties.Add("RootNode", (JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(lineArray[1].Trim())).fileID));
                }
                else if (lineArray[0].Contains("TreeState"))
                {
                    behaviourTreeNodeDef.bt_properties.Add("TreeState", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("callFrequencyCount"))
                {
                    behaviourTreeNodeDef.bt_properties.Add("callFrequencyCount", lineArray[1].Trim());
                }
                /////// Behaviour tree properties END
                /////// Node properties START
                else if (lineArray[0].Trim().Equals("child"))
                {
                    behaviourTreeNodeDef.child = JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Trim().Equals("children"))
                {
                    behaviourTreeNodeDef.children = GetNodeList(reader);
                }
                // Repeat
                else if (lineArray[0].Contains("restartOnSuccess"))
                {
                    behaviourTreeNodeDef.node_properties.Add("restartOnSuccess", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("restartOnFailure"))
                {
                    behaviourTreeNodeDef.node_properties.Add("restartOnFailure", lineArray[1].Trim());
                }
                // MoveForward
                else if (lineArray[0].Contains("moveForwardDirection"))
                {
                    behaviourTreeNodeDef.node_properties.Add("moveForwardDirection", lineArray[1].Trim());
                }
                // MoveSide
                else if (lineArray[0].Contains("moveSideDirection"))
                {
                    behaviourTreeNodeDef.node_properties.Add("moveSideDirection", lineArray[1].Trim());
                }
                // Rotate & RotateTurret
                else if (lineArray[0].Contains("rotateDirection"))
                {
                    behaviourTreeNodeDef.node_properties.Add("rotateDirection", lineArray[1].Trim());
                }
                // RayHitObject, GridCellContainsObject
                else if (lineArray[0].Contains("targetGameObject"))
                {
                    behaviourTreeNodeDef.node_properties.Add("targetGameObject", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("side"))
                {
                    behaviourTreeNodeDef.node_properties.Add("side", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("rayIndex"))
                {
                    behaviourTreeNodeDef.node_properties.Add("rayIndex", lineArray[1].Trim());
                }
                // Shoot
                else if (lineArray[0].Contains("shoot"))
                {
                    behaviourTreeNodeDef.node_properties.Add("shoot", lineArray[1].Trim());
                }
                // PlaceBomb
                else if (lineArray[0].Contains("placeBomb"))
                {
                    behaviourTreeNodeDef.node_properties.Add("placeBomb", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("gridPositionX"))
                {
                    behaviourTreeNodeDef.node_properties.Add("gridPositionX", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("gridPositionY"))
                {
                    behaviourTreeNodeDef.node_properties.Add("gridPositionY", lineArray[1].Trim());
                }
                // HealthLevel, ShieldLevel, AmmoLevel
                else if (lineArray[0].Contains("healthLevel"))
                {
                    behaviourTreeNodeDef.node_properties.Add("healthLevel", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("shieldLevel"))
                {
                    behaviourTreeNodeDef.node_properties.Add("shieldLevel", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("ammoLevel"))
                {
                    behaviourTreeNodeDef.node_properties.Add("ammoLevel", lineArray[1].Trim());
                }
                // TODO Add other node properties here
                /////// Node properties END


            }
        }
        // Add the last behaviour tree asset to the list
        behaviourTreeNodeDefs.Add(behaviourTreeNodeDef);

        return ConvertBehaviourTreeNodeDefsToBehaviourTree(behaviourTreeNodeDefs);
    }

    // TODO Remove in the future
    public static List<Child> GetNodeList(StreamReader reader)
    {
        List<Child> nodes = new List<Child>();
        string nodeLine = reader.ReadLine();
        nodeLine = nodeLine.Replace(" ", "").Trim();
        while (nodeLine.Contains("{fileID:"))
        {
            nodeLine = nodeLine.Trim().Substring(1);
            nodes.Add(JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(nodeLine)));
            if (reader.Peek() != ' ')
                break;
            nodeLine = reader.ReadLine();
            nodeLine = nodeLine.Replace(" ", "").Trim();
        }

        return nodes;
    }

    // TODO Remove in the future
    public static string convertSringArrayToJson(string input)
    {
        input = input.Replace(" ", "");
        input = input.Replace("{", "{\"");
        input = input.Replace("}", "\"}");
        input = input.Replace(",", "\",\"");
        input = input.Replace(":", "\":\"");
        return input;
    }

    // TODO Remove in the future
    public static BehaviorTreeAgentController ConvertBehaviourTreeNodeDefsToBehaviourTree(List<BehaviourTreeNodeDef> behaviourTreeNodeDefs)
    {
        BehaviorTreeAgentController tree = new BehaviorTreeAgentController();

        // Find the behaviour tree base def
        BehaviourTreeNodeDef behaviourTreeDef = behaviourTreeNodeDefs.Find(x => x.bt_properties.ContainsKey("RootNode"));

        tree.AgentControllerId = int.Parse(behaviourTreeDef.bt_properties["Id"]);
        tree.name = behaviourTreeDef.m_Name;
        tree.TreeState = Node.NodeStateStringToNodeState(behaviourTreeDef.bt_properties["TreeState"]);

        // Find RootBehaviourTreeNodeDef
        //BehaviourTreeNodeDef rootBehaviourTreeNodeDef = behaviourTreeNodeDefs.Find(x => x.m_Script.guid.Equals("163c147d123e4a945b688eddc64e3ea5"));
        BehaviourTreeNodeDef rootBehaviourTreeNodeDef = behaviourTreeNodeDefs.Find(x => x.m_fileID == behaviourTreeDef.bt_properties["RootNode"]);

        // Set root node and create the tree from behaviourTreeNodeDefs
        tree.RootNode = Node.CreateNodeTreeFromBehaviourTreeNodeDef(rootBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);

        return tree;
    }
}