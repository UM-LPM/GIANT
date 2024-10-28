using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AITechniques.BehaviorTrees;
using UnityEngine;

public class Util : MonoBehaviour {
    //[SerializeField] bool SeedFromCommunicator = true;
    [SerializeField] int InitialSeed = 316227711;
    [SerializeField] public System.Random Rnd;
    [SerializeField] public System.Random RndBt;
    //[SerializeField] bool RandomSeed = false;

    private void Awake() {
        if (Communicator.Instance != null) {
            if (Communicator.Instance.RandomSeedMode == RandomSeedMode.Fixed) {
                Rnd = new System.Random(Communicator.Instance.InitialSeed);
                RndBt = new System.Random(Communicator.Instance.InitialSeed);
            }
            else if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll) {
                Rnd = new System.Random(Communicator.Instance.InitialSeed);
                RndBt = new System.Random(Communicator.Instance.InitialSeed);
            }
            else { // RandomSeedMode.RandomPerIndividual
                Rnd = new System.Random();
                RndBt = new System.Random();
            }
        }
        else {
            Rnd = new System.Random(InitialSeed);
            RndBt = new System.Random(InitialSeed);
        }
    }

    public double NextDouble()
    {
        return Rnd.NextDouble();
    }

    public double NextDoubleBt()
    {
        return RndBt.NextDouble();
    }

    public float NextFloat(float min, float max) {
        double val = (Rnd.NextDouble() * (max - min) + min);
        return (float)val;
    }

    public float NextFloatdBt(float min, float max)
    {
        double val = (RndBt.NextDouble() * (max - min) + min);
        return (float)val;
    }

    public int NextInt(int min, int max)
    {
        return Rnd.Next(min, max);
    }

    public int NextIntBt(int min, int max)
    {
        return RndBt.Next(min, max);
    }

    public int NextInt(int max)
    {
        return Rnd.Next(max);
    }

    public int NextIntBt(int max)
    {
        return RndBt.Next(max);
    }
}

public class UnityAssetParser {
    public static BehaviourTree[] ParseBehaviourTreesFromFolder(string folderPath, int start, int end) {
        List<BehaviourTree> trees = new List<BehaviourTree>();

        // Read all files in the folder
        string[] files = System.IO.Directory.GetFiles(folderPath, "*.asset");
        files = files.OrderBy(file => int.Parse(Regex.Match(file, @"(\d+)").Groups[0].ToString())).ToArray();

        for (int i = start; i < end; i++) {
            BehaviourTree tree = ParseBehaviourTree(files[i]);
            trees.Add(tree);
        }

        // TODO Remove
        /*foreach (string file in files) {
            // Load the asset
            BehaviourTree tree = ParseBehaviourTree(file);
            trees.Add(tree);
        }*/

        return trees.ToArray();
    }

    public static BehaviourTree ParseBehaviourTree(string path) {
        List<BehaviourTreeNodeDef> behaviourTreeNodeDefs = new List<BehaviourTreeNodeDef>();

        BehaviourTreeNodeDef behaviourTreeNodeDef = null;
        using (StreamReader reader = new StreamReader(path)) {
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] lineArray = line.Split(':', 2);

                // Shared properties
                if (lineArray[0].Contains("--- !u!114")) {
                    // If we have a previous behaviour tree asset, add it to the list
                    if (behaviourTreeNodeDef != null) {
                        behaviourTreeNodeDefs.Add(behaviourTreeNodeDef);
                    }

                    // Initialize new behaviour tree asset
                    string[] nodeHeaderArray = lineArray[0].Split(' ');
                    nodeHeaderArray[2] = nodeHeaderArray[2].Replace('&', ' ').Trim();

                    behaviourTreeNodeDef = new BehaviourTreeNodeDef(nodeHeaderArray[2]);
                }
                else if (lineArray[0].Contains("m_Script")) {
                    behaviourTreeNodeDef.m_Script = JsonConvert.DeserializeObject<MScript>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Contains("m_Name")) {
                    behaviourTreeNodeDef.m_Name = lineArray[1].Trim();
                }
                else if (lineArray[0].Contains("guid")) {
                    behaviourTreeNodeDef.guid = lineArray[1].Trim();
                }
                else if (lineArray[0].Contains("position")) {
                    behaviourTreeNodeDef.position = JsonConvert.DeserializeObject<Position>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Contains("Blackboard")) {
                    lineArray[1] = lineArray[1].Trim();
                    if (lineArray[1].Length > 0) {
                        behaviourTreeNodeDef.blackboard = JsonConvert.DeserializeObject<Blackboard>(convertSringArrayToJson(lineArray[1].Trim()));
                    }
                }
                else if (lineArray[0].Contains("drawGizmos")) {
                    behaviourTreeNodeDef.drawGizmos = int.Parse(lineArray[1].Trim());
                }
                /////// Behaviour tree properties START
                else if (lineArray[0].Contains("Nodes")) {
                    behaviourTreeNodeDef.nodes = GetNodeList(reader);
                }
                else if (lineArray[0].Trim().Equals("Id")) {
                    behaviourTreeNodeDef.bt_properties.Add("Id", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("RootNode")) {
                    behaviourTreeNodeDef.bt_properties.Add("RootNode", (JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(lineArray[1].Trim())).fileID));
                }
                else if (lineArray[0].Contains("TreeState")) {
                    behaviourTreeNodeDef.bt_properties.Add("TreeState", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("callFrequencyCount"))
                {
                    behaviourTreeNodeDef.bt_properties.Add("callFrequencyCount", lineArray[1].Trim());
                }
                /////// Behaviour tree properties END
                /////// Node properties START
                else if (lineArray[0].Trim().Equals("child")) {
                    behaviourTreeNodeDef.child = JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(lineArray[1].Trim()));
                }
                else if (lineArray[0].Trim().Equals("children")) {
                    behaviourTreeNodeDef.children = GetNodeList(reader);
                }
                // Repeat
                else if (lineArray[0].Contains("restartOnSuccess")) {
                    behaviourTreeNodeDef.node_properties.Add("restartOnSuccess", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("restartOnFailure")) {
                    behaviourTreeNodeDef.node_properties.Add("restartOnFailure", lineArray[1].Trim());
                }
                // MoveForward
                else if (lineArray[0].Contains("moveForwardDirection")) {
                    behaviourTreeNodeDef.node_properties.Add("moveForwardDirection", lineArray[1].Trim());
                }
                // MoveSide
                else if (lineArray[0].Contains("moveSideDirection")) {
                    behaviourTreeNodeDef.node_properties.Add("moveSideDirection", lineArray[1].Trim());
                }
                // Rotate & RotateTurret
                else if (lineArray[0].Contains("rotateDirection")) {
                    behaviourTreeNodeDef.node_properties.Add("rotateDirection", lineArray[1].Trim());
                }
                // RayHitObject, GridCellContainsObject
                else if (lineArray[0].Contains("targetGameObject")) {
                    behaviourTreeNodeDef.node_properties.Add("targetGameObject", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("side")) {
                    behaviourTreeNodeDef.node_properties.Add("side", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("rayIndex")) {
                    behaviourTreeNodeDef.node_properties.Add("rayIndex", lineArray[1].Trim());
                }
                // Shoot
                else if (lineArray[0].Contains("shoot")) {
                    behaviourTreeNodeDef.node_properties.Add("shoot", lineArray[1].Trim());
                }
                // PlaceBomb
                else if (lineArray[0].Contains("placeBomb")) {
                    behaviourTreeNodeDef.node_properties.Add("placeBomb", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("gridPositionX")) {
                    behaviourTreeNodeDef.node_properties.Add("gridPositionX", lineArray[1].Trim());
                }
                else if (lineArray[0].Contains("gridPositionY")) {
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

    public static List<Child> GetNodeList(StreamReader reader) {
        List<Child> nodes = new List<Child>();
        string nodeLine = reader.ReadLine();
        nodeLine = nodeLine.Replace(" ", "").Trim();
        while (nodeLine.Contains("{fileID:")) {
            nodeLine = nodeLine.Trim().Substring(1);
            nodes.Add(JsonConvert.DeserializeObject<Child>(convertSringArrayToJson(nodeLine)));
            if(reader.Peek() != ' ')
                break;
            nodeLine = reader.ReadLine();
            nodeLine = nodeLine.Replace(" ", "").Trim();
        }

        return nodes;
    }

    public static string convertSringArrayToJson(string input) {
        input = input.Replace(" ", "");
        input = input.Replace("{", "{\"");
        input = input.Replace("}", "\"}");
        input = input.Replace(",", "\",\"");
        input = input.Replace(":", "\":\"");
        return input;
    }

    public static BehaviourTree ConvertBehaviourTreeNodeDefsToBehaviourTree(List<BehaviourTreeNodeDef> behaviourTreeNodeDefs) {
        BehaviourTree tree = new BehaviourTree();

        // Find the behaviour tree base def
        BehaviourTreeNodeDef behaviourTreeDef = behaviourTreeNodeDefs.Find(x => x.bt_properties.ContainsKey("RootNode"));

        tree.id = int.Parse(behaviourTreeDef.bt_properties["Id"]);
        tree.name = behaviourTreeDef.m_Name;
        tree.treeState = Node.NodeStateStringToNodeState(behaviourTreeDef.bt_properties["TreeState"]);
        
        // Find RootBehaviourTreeNodeDef
        //BehaviourTreeNodeDef rootBehaviourTreeNodeDef = behaviourTreeNodeDefs.Find(x => x.m_Script.guid.Equals("163c147d123e4a945b688eddc64e3ea5"));
        BehaviourTreeNodeDef rootBehaviourTreeNodeDef = behaviourTreeNodeDefs.Find(x => x.m_fileID == behaviourTreeDef.bt_properties["RootNode"]);

        // Set root node and create the tree from behaviourTreeNodeDefs
        tree.rootNode = Node.CreateNodeTreeFromBehaviourTreeNodeDef(rootBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);

        return tree;
    }
}

public class BehaviourTreeNodeDef{
    public string m_fileID { get; set; }
    public MScript m_Script { get; set; }
    public string m_Name { get; set; }
    public string guid { get; set; }
    public Position position { get; set; }
    public Blackboard blackboard { get; set; }
    public int drawGizmos { get; set; }
    public Child child { get; set; }
    public List<Child> children { get; set; }
    public List<Child> nodes { get; set; }

    // TODO add dictionary for node specific properties
    public Dictionary<string, string> bt_properties { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> node_properties { get; set; } = new Dictionary<string, string>();

    public BehaviourTreeNodeDef(string m_id) {
        this.m_fileID = m_id;
    }
}

public class Position {
    public float x { get; set; }
    public float y { get; set; }
}

public class Child {
    public string fileID { get; set; }
}

public class MScript {
    public string fileID { get; set; }
    public string guid { get; set; }
    public int type { get; set; }
}