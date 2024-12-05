using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using Newtonsoft.Json;
using AgentOrganizations;
using System.Text;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JsonToSoParserController : ControllerBase {

        public static JsonSerializerSettings JSON_SERIALIZATION_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        [HttpPost]
        public async Task<IActionResult> ParseJson([FromBody] RequestBodyParams requestBodyParams)
        {
            string? error = ValidateInput(requestBodyParams);
            if(error != null)
            {
                Util.WriteErrorToFile("Failed to parse JSON", error, "0_JsonToSoParserControllerError");
                return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = error });
            }

            try
            {
                string jsonString = System.IO.File.ReadAllText(requestBodyParams.SourceFilePath!);

                // Deserialize the JSON string to a dynamic object or a custom class
                TreeModel[] treeModels = JsonConvert.DeserializeObject<TreeModel[]>(jsonString);

                ClearFolder(requestBodyParams.DestinationFilePath!);

                if (treeModels.Length == 0)
                {
                    Util.WriteErrorToFile("Failed to parse JSON", "No behaviour trees sent in request", "1_JsonToSoParserControllerError");
                    return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = "No behaviour trees sent in request" });
                }

                int currentIndex = 0;
                Individual[] individuals = new Individual[treeModels.Length];

                foreach (TreeModel treeModel in treeModels)
                {
                    // Update node IDs
                    TreeModelNode.UpdateNoteIDs(treeModel.RootNode!);

                    // Update node positions
                    TreeModelNode.UpdateNodePositions(treeModel.RootNode);

                    individuals[currentIndex] = new Individual(currentIndex, treeModel);
                    
                    currentIndex++;
                }

                // Save Individuals to files
                foreach (Individual individual in individuals)
                {
                    string individualString = JsonConvert.SerializeObject(individual, JSON_SERIALIZATION_SETTINGS);
                    individualString = individualString.Replace(", WebAPI", ", Assembly-CSharp");
                    individualString = individualString.Replace("System.Private.CoreLib", "mscorlib");
                    individualString = individualString.Replace("UnityEngine.Vector2, Assembly-CSharp", "UnityEngine.Vector2, UnityEngine.CoreModule");
                    saveBehaviourTreeToFile(individualString, requestBodyParams.DestinationFilePath + "" + individual.name + ".json");
                }

                // Create a request to CoordinatorURI
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(100);

                        var task = client.PostAsync(requestBodyParams.CoordinatorURI, new StringContent(JsonConvert.SerializeObject(new CoordinatorEvalRequestData() { EvalEnvInstances = requestBodyParams.EvalEnvInstanceURIs, EvalRangeStart = 0, EvalRangeEnd = treeModels.Length, LastEvalIndividualFitnesses = requestBodyParams.LastEvalIndividualFitnesses }), Encoding.UTF8, "application/json"));
                        task.Wait();

                        HttpResponseMessage responseMessage = await task;
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            string result = await responseMessage.Content.ReadAsStringAsync();

                            if (result == null)
                            {
                                Util.WriteErrorToFile("Failed request", "Request failed result is null", "3_JsonToSoParserControllerError");
                                return BadRequest(new { Status = "Error", Message = $"Request failed result is null" });
                            }

                            CoordinatorEvaluationResult? response = JsonConvert.DeserializeObject<CoordinatorEvaluationResult>(result);

                            if(response == null)
                            {
                                Util.WriteErrorToFile("Failed request", "Request failed response is null", "4_JsonToSoParserControllerError");
                                return BadRequest(new { Status = "Error", Message = $"Request failed response is null" });
                            }

                            return Ok(new { Status = "Success", Message = "JSON parsing was successful.", Object = response });
                        }
                        else
                        {
                            Util.WriteErrorToFile("Failed request", $"Request failed with status code: {responseMessage.StatusCode}", "4_JsonToSoParserControllerError");
                            return BadRequest(new { Status = "Error", Message = $"Request failed with status code: {responseMessage.StatusCode}" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "5_JsonToSoParserControllerError");
                    return BadRequest(new { Status = "Error", Message = "Failed to make request to CoordinatorURI.", Error = ex.Message });
                }
            }
            catch (Exception ex)
            {
                Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "6_JsonToSoParserControllerError");
                return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = ex.Message });
            }
        }

        public static string? ValidateInput(RequestBodyParams requestBodyParams)
        {
            if (requestBodyParams == null)
            {
                return "RequestBodyParams is null";
            }

            if (requestBodyParams.SourceFilePath == null || requestBodyParams.SourceFilePath.Length == 0)
            {
                return "SourceFilePath is null";
            }

            if (requestBodyParams.DestinationFilePath == null || requestBodyParams.DestinationFilePath.Length == 0)
            {
                return "DestinationFilePath is null";
            }

            if(requestBodyParams.EvalEnvInstanceURIs == null || requestBodyParams.EvalEnvInstanceURIs.Length == 0)
            {
                return "EvalEnvInstanceURIs is null";
            }

            if(requestBodyParams.LastEvalIndividualFitnesses != null && requestBodyParams.LastEvalIndividualFitnesses.Length == 0)
            {
                return "LastEvalIndividualFitnesses is empty";
            }

            return null;
        }

        public static void saveBehaviourTreeToFile(string behaviourTreeString, string filepath) {
            using (StreamWriter outputFile = new StreamWriter(filepath)) {
                outputFile.Write(behaviourTreeString);
            }
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
