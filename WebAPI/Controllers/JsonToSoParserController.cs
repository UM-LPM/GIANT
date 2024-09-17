using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace WebAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class JsonToSoParserController : ControllerBase {

        // POST api/<JsonToSoParserController>
        [HttpPost]
        public async Task<IActionResult> ParseJson([FromBody] string[] evalEnvInstances/*[FromBody] TreeModel[] treeModels*/) {
            try {
                string sourceFilePath = @".\RequestData\jsonBody.json";

                string destinationFilePath = @".\..\GeneralTrainingEnvironmentForMAS\Assets\Resources\RobostrikeBts\";
                //string destinationFilePath = @".\..\GeneralTrainingEnvironmentForMAS\Assets\Resources\CollectorBts\"; 

                string jsonString = System.IO.File.ReadAllText(sourceFilePath);

                // Deserialize the JSON string to a dynamic object or a custom class

                TreeModel[] treeModels = JsonConvert.DeserializeObject<TreeModel[]>(jsonString);

                ClearFolder(destinationFilePath);

                if (treeModels.Length == 0) {
                    Util.WriteErrorToFile("Failed to parse JSON", "No behaviour trees sent in request", "1_JsonToSoParserControllerError");
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
                    int numOfIndividuals = treeModels.Length;
                    int numOfInstances = evalEnvInstances.Length;

                    int numOfIndividualsPerInstance = numOfIndividuals / numOfInstances;
                    int remainder = numOfIndividuals % numOfInstances;

                    // Create a single HttpClient instance
                    using (HttpClient client = new HttpClient()) {
                        client.Timeout = TimeSpan.FromMinutes(100);

                        Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numOfInstances];
                        for (int i = 0; i < numOfInstances; i++) {
                            // Assuming UnityEvalRequestData is defined elsewhere
                            tasks[i] = client.PostAsync(evalEnvInstances[i], new StringContent(JsonConvert.SerializeObject(new UnityEvalRequestData() { evalRangeStart = i * numOfIndividualsPerInstance, evalRangeEnd = i * numOfIndividualsPerInstance + numOfIndividualsPerInstance + (i == numOfInstances - 1 ? remainder : 0) }), Encoding.UTF8, "application/json"));
                        }

                        // Wait for all tasks to complete
                        await Task.WhenAll(tasks);

                        FitnessIndividual[] FinalPopFitnesses = new FitnessIndividual[numOfIndividuals];
                        int[][] FinalBtsNodeCallFrequencies = new int[numOfIndividuals][];
                        foreach (Task<HttpResponseMessage> task in tasks) {
                            HttpResponseMessage response = await task;
                            if (response.IsSuccessStatusCode) {
                                string result = await response.Content.ReadAsStringAsync();
                                UnityHttpServerResponse responseObject = JsonConvert.DeserializeObject<UnityHttpServerResponse>(result);
                                if (responseObject == null) {
                                    Util.WriteErrorToFile("Failed to parse JSON", "Response object is null", "2_JsonToSoParserControllerError");
                                    return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = "Response object is null" });
                                }

                                for (int i = responseObject.EvalRequestData.evalRangeStart; i < responseObject.EvalRequestData.evalRangeEnd; i++) {
                                    FinalPopFitnesses[i] = responseObject.PopFitness[i - responseObject.EvalRequestData.evalRangeStart];
                                    FinalBtsNodeCallFrequencies[i] = responseObject.BtsNodeCallFrequencies[i - responseObject.EvalRequestData.evalRangeStart];
                                }
                            }
                            else {
                                Util.WriteErrorToFile("Request failed", $"Request failed with status code: {response.StatusCode}", "3_JsonToSoParserControllerError");
                                return BadRequest(new { Status = "Error", Message = $"Request failed with status code: {response.StatusCode}" });
                            }
                        }

                        return Ok(new { Status = "Success", Message = "JSON parsing was successful.", Object = new HttpServerResponse() { PopFitness = FinalPopFitnesses, 
                        BtsNodeCallFrequencies = FinalBtsNodeCallFrequencies
                        } });
                    }
                    /*using (HttpClient client = new HttpClient()) {
                        client.Timeout = TimeSpan.FromMinutes(100);
                        //client.PostAsync("http://localhost:4444", new StringContent(""));
                        //Task< HttpResponseMessage> response =  client.GetAsync("http://localhost:4444");
                        //response.Wait();

                        HttpResponseMessage response = await client.GetAsync(evalEnvInstances[0]);
                        if (response.IsSuccessStatusCode) {
                            string result = await response.Content.ReadAsStringAsync();

                            return Ok(new { Status = "Success", Message = "JSON parsing was successful.", Object = result });
                        }
                        else {
                            return BadRequest(new { Status = "Error", Message = $"Request failed with status code: {response.StatusCode}" });
                        }
                    }*/
                }
                catch (Exception ex) {
                    Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "4_JsonToSoParserControllerError");
                    return BadRequest(new { Status = "Error", Message = "Failed to make request to localhost:4444.", Error = ex.Message });
                }
            }
            catch (Exception ex) {
                Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "5_JsonToSoParserControllerError");
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

public class HttpServerResponse {
    public FitnessIndividual[] PopFitness { get; set; }
    public int[][] BtsNodeCallFrequencies { get; set; }
}

public class UnityHttpServerResponse {
    public FitnessIndividual[] PopFitness { get; set; }
    public int[][] BtsNodeCallFrequencies { get; set; }
    public UnityEvalRequestData EvalRequestData { get; set; }
}

public class UnityEvalRequestData {
    public int evalRangeStart { get; set; }
    public int evalRangeEnd { get; set; }
}


public class FitnessIndividual {
    public float FinalFitness { get; set; } // Used to store final fitness (sum of all fitnesses from different game scenarios)
    public float FinalFitnessStats { get; set; } // Used to store final fitness calculated statistic (mean, std deviation, min, max,...)
    public Dictionary<string, Fitness> Fitnesses { get; set; } // Used to store fitnesses from different game scenarios

}

public class Fitness {
    public float Value { get; set; }
    public Dictionary<string, float> IndividualValues { get; set; }
}

public class Util
{
    public static void WriteErrorToFile(string message, string errorDetail, string filename)
    {
        string path = @".\ErrorLogs\" + filename + ".txt";
        // Delete the file if it exists.
        if(File.Exists(path))
        {
            File.Delete(path);
        }

        // Create a new file and write the error message to it.
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Message: " + message + "\nErrorDetail: " + errorDetail + "\n");
        }
    }
}