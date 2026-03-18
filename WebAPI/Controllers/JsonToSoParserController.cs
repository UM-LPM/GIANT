using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using Newtonsoft.Json;
using AgentOrganizations;
using System.Text;
using WebAPI.Models.EARS;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JsonToSoParserController : ControllerBase {
        private readonly MqttClientService _mqttClientService;

        public static JsonSerializerSettings JSON_SERIALIZATION_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public JsonToSoParserController(MqttClientService mqttClientService)
        {
            _mqttClientService = mqttClientService;
        }

        [HttpPost]
        public async Task<IActionResult> ParseJson([FromBody] RequestBodyParams requestBodyParams)
        {
            if (!_mqttClientService.GetMqttClient().IsConnected)
            {
                try
                {
                    await _mqttClientService.StartAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            string? error = ValidateInput(requestBodyParams);
            if(error != null)
            {
                Util.WriteErrorToFile("Failed to parse JSON", error, "0_JsonToSoParserControllerError");
                await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed to parse JSON"));
                return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = error });
            }

            try
            {
                string jsonString = System.IO.File.ReadAllText(requestBodyParams.ApiRequestDataSourceFilePath!);

                // Deserialize the JSON string to a dynamic object or a custom class
                ProgramSolution[][] programs = JsonConvert.DeserializeObject<ProgramSolution[][]>(jsonString)!;
                
                ClearFolder(requestBodyParams.DestinationFilePath!);

                if (programs.Length == 0)
                {
                    Util.WriteErrorToFile("Failed to parse JSON", "No behaviour trees sent in request", "1_JsonToSoParserControllerError");
                    await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed to parse JSON: No behaviour trees sent in request"));
                    return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = "No behaviour trees sent in request" });
                }

                Individual[][] individuals = new Individual[programs.Length][];

                int currentIndex = 0;
                for (int i = 0; i < programs.Length; i++)
                {
                    individuals[i] = new Individual[programs[i].Length];
                    for (int j = 0; j < programs[i].Length; j++)
                    {
                        var program = programs[i][j];
                        foreach (ProgramSolutionPart programPart in program.SolutionParts)
                        {
                            programPart.Configure();
                        }

                        individuals[i][j] = new Individual(currentIndex++, program);
                    }
                }

                for (int i = 0; i < individuals.Length; i++)
                {
                    // Create a folder for the generation if it doesn't exist
                    string individualGroupFolderPath = Path.Combine(requestBodyParams.DestinationFilePath!, i.ToString());
                    if (!Directory.Exists(individualGroupFolderPath))
                    {
                        Directory.CreateDirectory(individualGroupFolderPath);
                    }
                    else
                    {
                        ClearFolder(individualGroupFolderPath);
                    }

                    for (int j = 0; j < individuals[i].Length; j++)
                    {
                        var individual = individuals[i][j];

                        string individualString = JsonConvert.SerializeObject(individual, JSON_SERIALIZATION_SETTINGS);
                        individualString = individualString.Replace(", WebAPI", ", Assembly-CSharp");
                        individualString = individualString.Replace("System.Private.CoreLib", "mscorlib");
                        individualString = individualString.Replace("UnityEngine.Vector2, Assembly-CSharp", "UnityEngine.Vector2, UnityEngine.CoreModule");
                        saveIndividualToFile(individualString, requestBodyParams.DestinationFilePath + i + "\\" + individual.name + ".json");
                    }
                }

                // Create a request to CoordinatorURI
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(100);

                        var task = client.PostAsync(
                            requestBodyParams.CoordinatorURI,
                            new StringContent(
                                JsonConvert.SerializeObject(
                                    new CoordinatorEvalRequestData() {
                                        EvalEnvInstances = requestBodyParams.EvalEnvInstanceURIs,
                                        EvalRanges = null,
                                        LastEvalIndividualFitnesses = requestBodyParams.LastEvalFinalIndividualFitnesses
                                    }),
                                Encoding.UTF8, "application/json")
                            );
                        task.Wait();

                        HttpResponseMessage responseMessage = await task;
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            string result = await responseMessage.Content.ReadAsStringAsync();

                            if (result == null)
                            {
                                Util.WriteErrorToFile("Failed request", "Request failed result is null", "3_JsonToSoParserControllerError");
                                await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed request: Request failed result is null"));
                                return BadRequest(new { Status = "Error", Message = $"Request failed result is null" });
                            }

                            CoordinatorEvaluationResult? response = JsonConvert.DeserializeObject<CoordinatorEvaluationResult>(result);

                            if(response == null)
                            {
                                Util.WriteErrorToFile("Failed request", "Request failed response is null", "4_JsonToSoParserControllerError");
                                await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed request: Request failed response is null"));
                                return BadRequest(new { Status = "Error", Message = $"Request failed response is null" });
                            }

                            return Ok(new { Status = "Success", Message = "JSON parsing was successful.", CoordinatorEvaluationResult = response });
                        }
                        else
                        {
                            Util.WriteErrorToFile("Failed request", $"Request failed with status code: {responseMessage.StatusCode}", "4_JsonToSoParserControllerError");
                            await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject($"Failed request: Request failed with status code: {responseMessage.StatusCode}"));
                            return BadRequest(new { Status = "Error", Message = $"Request failed with status code: {responseMessage.StatusCode}" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "5_JsonToSoParserControllerError");
                    await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed to make request to CoordinatorURI: " + ex.Message));
                    return BadRequest(new { Status = "Error", Message = "Failed to make request to CoordinatorURI.", Error = ex.Message });
                }
            }
            catch (Exception ex)
            {
                Util.WriteErrorToFile("Failed to parse JSON.", ex.Message, "6_JsonToSoParserControllerError");
                await _mqttClientService.PublishAsync(MqttClientService.Topic, JsonConvert.SerializeObject("Failed to parse JSON: " + ex.Message));
                return BadRequest(new { Status = "Error", Message = "Failed to parse JSON.", Error = ex.Message });
            }
        }

        public static string? ValidateInput(RequestBodyParams requestBodyParams)
        {
            if (requestBodyParams == null)
            {
                return "RequestBodyParams is null";
            }

            if (requestBodyParams.ApiRequestDataSourceFilePath == null || requestBodyParams.ApiRequestDataSourceFilePath.Length == 0)
            {
                return "ApiRequestDataSourceFilePath is null";
            }

            if (requestBodyParams.DestinationFilePath == null || requestBodyParams.DestinationFilePath.Length == 0)
            {
                return "DestinationFilePath is null";
            }

            if(requestBodyParams.EvalEnvInstanceURIs == null || requestBodyParams.EvalEnvInstanceURIs.Length == 0)
            {
                return "EvalEnvInstanceURIs is null";
            }

            if(requestBodyParams.LastEvalFinalIndividualFitnesses != null && requestBodyParams.LastEvalFinalIndividualFitnesses.Length == 0)
            {
                return "LastEvalFinalIndividualFitnesses is empty";
            }

            return null;
        }

        public static void saveIndividualToFile(string individualString, string filepath) {
            using (StreamWriter outputFile = new StreamWriter(filepath)) {
                outputFile.Write(individualString);
            }
        }

        public static void ClearFolder(string path)
        {
            // Delete all files in the folder
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
            {
                System.IO.File.Delete(file);

            }

            // Delete all subfolders in the folder
            string[] folders = System.IO.Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                System.IO.Directory.Delete(folder, true);
            }
        }
    }
}
