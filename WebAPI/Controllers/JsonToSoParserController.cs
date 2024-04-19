using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class JsonToSoParserController : ControllerBase {

        // POST api/<JsonToSoParserController>
        [HttpPost]
        public async Task<IActionResult> ParseJson(/*[FromBody] TreeModel[] treeModels*/) {
            try {
                string sourceFilePath = @"C:\Users\marko\UnityProjects\GeneralTrainingEnvironmentForMAS\WebAPI\RequestData\jsonBody.json";

                //string destinationFilePath = @"C:\Users\marko\UnityProjects\GeneralTrainingEnvironmentForMAS\GeneralTrainingEnvironmentForMAS\Assets\Resources\RobocodeBts\";
                string destinationFilePath = @"C:\Users\marko\UnityProjects\GeneralTrainingEnvironmentForMAS\GeneralTrainingEnvironmentForMAS\Assets\Resources\CollectorBts\";

                string jsonString = System.IO.File.ReadAllText(sourceFilePath);

                // Deserialize the JSON string to a dynamic object or a custom class

                TreeModel[] treeModels = JsonConvert.DeserializeObject<TreeModel[]>(jsonString);

                ClearFolder(destinationFilePath);

                if (treeModels.Length == 0) {
                    return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = "No behaviour trees sent in request" });
                }

                int currentIndex = 1;
                foreach (TreeModel treeModel in treeModels) {

                    Node.UpdateNoteIDs(treeModel.RootNode);

                    // Update node positions OPTION 1
                    Node.UpdateNodePositions(treeModel.RootNode);

                    // Update node positions OPTION 2
                    //int index = 0;
                    //UpdateNodePositions(treeModel.RootNode, 0, ref index, 140, 160);

                    treeModel.Name = treeModel.Name + currentIndex.ToString();

                    string behaviourTreeString = ConvertTreeToSO(treeModel, currentIndex++);

                    saveBehaviourTreeToFile(behaviourTreeString, destinationFilePath + treeModel.Name + ".asset");

                    //Console.WriteLine(behaviourTreeString);
                }

                // Create a request to localhost:4444
                try {
                    using (HttpClient client = new HttpClient()) {
                        client.Timeout = TimeSpan.FromMinutes(100);

                        HttpResponseMessage response = await client.GetAsync("http://localhost:4444");
                        if (response.IsSuccessStatusCode) {
                            string result = await response.Content.ReadAsStringAsync();

                            return Ok(new { Status = "Success", Message = "JSON parsing was successful.", Object = result });
                        }
                        else {
                            return BadRequest(new { Status = "Error", Message = $"Request failed with status code: {response.StatusCode}" });
                        }
                    }
                }
                catch (Exception ex) {
                    return BadRequest(new { Status = "Error", Message = "Failed to make request to localhost:4444.", Error = ex.Message });
                }
            }
            catch (Exception ex) {
                return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = ex.Message });
            }

        }

        public static void saveBehaviourTreeToFile(string behaviourTreeString, string filepath) {
            //File.WriteAllText(behaviourTreeString, filepath);

            using (StreamWriter outputFile = new StreamWriter(filepath)) {
                outputFile.Write(behaviourTreeString);
            }
        }

        public static string ConvertTreeToSO(TreeModel treeModel, int currentID) {
            if (treeModel.RootNode == null)
                throw new Exception("Root node is null");

            string behaviourTreeString = "%YAML 1.1\r\n%TAG !u! tag:unity3d.com,2011:";

            List<string> nodeFileIDs = new List<string>();

            Queue<Node> nodeQueue = new Queue<Node>();
            if (treeModel.RootNode != null)
                nodeQueue.Enqueue(treeModel.RootNode);

            while (nodeQueue.Count > 0) {
                Node node = nodeQueue.Dequeue();
                nodeFileIDs.Add(node.FileID.ToString());

                behaviourTreeString += NodeString(node, treeModel.BlackboardItems);

                if (node.Children != null)
                    foreach (Node child in node.Children) {
                        nodeQueue.Enqueue(child);
                    }
            }

            treeModel.FileID = Node.GenerateRandomFileID();

            behaviourTreeString += BehaviourTreeString(treeModel.Name, treeModel.RootNode.FileID, nodeFileIDs, treeModel.BlackboardItems, treeModel.FileID, currentID);


            return behaviourTreeString;
        }


        // {0} -> treeName
        // {1} -> rootNode
        // {2} -> node file ids'
        // {3} -> blackboard // TODO
        // {4} -> MonoBehaviourId // 18 characters
        public static string BehaviourTreeString(string treeName, long rootNodeFileID, List<string> nodeFileIds, List<BlackboardItem> blackboardItems, long fileID, int currentID) {

            string nodeFileIdsString = "";
            foreach (string nodeFileId in nodeFileIds) {
                nodeFileIdsString += String.Format("\r\n  - {{fileID: {0}}}", nodeFileId);
            }

            string blackboardItemsString = "";
            foreach (BlackboardItem blackboardItem in blackboardItems) {
                blackboardItemsString += String.Format("\r\n    {0}: {1}", blackboardItem.Name, blackboardItem.Value);
            }

            return String.Format("\r\n--- !u!114 &{4}\r\nMonoBehaviour:\r\n  m_ObjectHideFlags: 0\r\n  m_CorrespondingSourceObject: {{fileID: 0}}\r\n  m_PrefabInstance: {{fileID: 0}}\r\n  m_PrefabAsset: {{fileID: 0}}\r\n  m_GameObject: {{fileID: 0}}\r\n  m_Enabled: 1\r\n  m_EditorHideFlags: 0\r\n  m_Script: {{fileID: 11500000, guid: 2d285eb63c2cdd74180de7cfceaa96ad, type: 3}}\r\n  m_Name: {0}\r\n  m_EditorClassIdentifier: \r\n  id: {5}\r\n  rootNode: {{fileID: {1}}}\r\n  treeState: 0\r\n  nodes: {2}\r\n  blackboard: {3}",
                treeName, rootNodeFileID.ToString(), nodeFileIdsString, blackboardItemsString, fileID.ToString(), currentID);
        }

        public static string NodeString(Node node, List<BlackboardItem> blackboardItems) {
            string blackboardItemsString = "";
            foreach (BlackboardItem blackboardItem in blackboardItems) {
                blackboardItemsString += String.Format("\r\n  {0}: {1}", blackboardItem.Name, blackboardItem.Value);
            }

            string nodePropertiesString = "";
            if (node.Properties != null) {
                foreach (Property nodeProperty in node.Properties) {
                    nodePropertiesString += String.Format("\r\n  {0}: {1}", nodeProperty.Name, nodeProperty.Value);
                }
            }

            string nodeChildrenString = "";

            bool childrenCond = node.Children?.Count > 1 || (node.Children?.Count == 1 && (node.Name == "Selector" || node.Name == "Sequencer"));
            if (node.Children != null) {
                foreach (Node nodeChild in node.Children)
                    if (childrenCond)
                        nodeChildrenString += String.Format("\r\n  - {{ fileID: {0} }}", nodeChild.FileID.ToString());
                    else
                        nodeChildrenString += String.Format("{{ fileID: {0} }}", nodeChild.FileID.ToString());
            }

            return String.Format("\r\n--- !u!114 &{6}\r\nMonoBehaviour:\r\n  m_ObjectHideFlags: 0\r\n  m_CorrespondingSourceObject: {{fileID: 0}}\r\n  m_PrefabInstance: {{fileID: 0}}\r\n  m_PrefabAsset: {{fileID: 0}}\r\n  m_GameObject: {{fileID: 0}}\r\n  m_Enabled: 1\r\n  m_EditorHideFlags: 0\r\n  m_Script: {{fileID: 11500000, guid: {0}, type: 3}}\r\n  m_Name: {8}\r\n  m_EditorClassIdentifier: \r\n  state: 0\r\n  started: 0\r\n  guid: {1}\r\n  position: {2}\r\n  blackboard:{3}\r\n  description: \r\n  drawGizmos: 0{4}{7}{5}",
                Node.GetNodeTypeGuid(Node.NodeTypeStringToNodeType(node.Name)), node.Guid.ToString(), String.Format("{{x: {0}, y: {1}}}", node.NodePosition.X.ToString(), node.NodePosition.Y), blackboardItemsString, nodePropertiesString, nodeChildrenString, node.FileID, childrenCond ? "\r\n  children:  " : node.Children?.Count == 1 ? "\r\n  child:  " : "", node.Name);
        }
        public static void ClearFolder(string path) {
            string[] files = System.IO.Directory.GetFiles(path);

            // Loop through and delete each file
            foreach (string file in files) {
                System.IO.File.Delete(file);

            }
        }

    }
}
