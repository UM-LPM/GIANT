using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using AgentOrganizations;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using VYaml.Serialization;
using Unity.VisualScripting;
using YamlDotNet.Serialization;

public class UnityAssetParser
{
    public static Individual[] ParseIndividualsFromFolder(string folderPath)
    {
        Individual[] individuals = null;

        // Read all files in the folder
        string[] files = System.IO.Directory.GetFiles(folderPath, "*.asset");
        files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)").Groups[0].ToString())).ToArray();

        individuals = new Individual[files.Length];

        YamlFile[] yamlIndividualFiles = ReadYamlDocumentsFromFolder(files);

        /*for (int i = 0; i < files.Length; i++)
        {
            string fileContent = File.ReadAllText(files[i]);
            string[] fileLines = fileContent.Split('\n');
            byte[] bytes = System.Text.Encoding.Default.GetBytes(fileContent);
            var yaml = YamlSerializer.DeserializeMultipleDocuments<dynamic>(bytes);

            // TODO Implement
            throw new NotImplementedException();
            //BehaviorTreeAgentController tree = ParseBehaviourTree(files[i]);
            //trees.Add(tree);


        }*/

        return individuals;
    }

    public static YamlFile[] ReadYamlDocumentsFromFolder(string[] files)
    {
        YamlFile[] yamlFiles = new YamlFile[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            yamlFiles[i] = ReadYamlDocumentsFromFolder(files[i]);
        }

        return yamlFiles;
    }

    public static YamlFile ReadYamlDocumentsFromFolder(string filePath)
    {
        string fileContent = File.ReadAllText(filePath);
        string[] fileLines = fileContent.Split('\n');

        YamlFile yamlFile = new YamlFile();

        YamlDocument yamlDocument = null;
        for (int i = 0; i < fileLines.Length; i++)
        {
            if (fileLines[i].Contains("---")){
                if(yamlDocument != null)
                {
                    yamlFile.Documents.Add(yamlDocument);
                }

                yamlDocument = new YamlDocument();
                string[] nodeHeaderArray = fileLines[i].Split(' ');
                nodeHeaderArray[2] = nodeHeaderArray[2].Replace('&', ' ').Trim();
                yamlDocument.DocumentId = nodeHeaderArray[2];

                yamlDocument.DocumentType = fileLines[++i].Split(":")[0].Trim();
            }
        }

        if(yamlDocument != null)
        {
            yamlFile.Documents.Add(yamlDocument);
        }

        return yamlFile;
    }


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

    public static string convertSringArrayToJson(string input)
    {
        input = input.Replace(" ", "");
        input = input.Replace("{", "{\"");
        input = input.Replace("}", "\"}");
        input = input.Replace(",", "\",\"");
        input = input.Replace(":", "\":\"");
        return input;
    }

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