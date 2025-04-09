# Platform Setup (Developer)

The platform consists of three parts: **Unity**, **Web API**, and **EARS framework**. To set up the platform follow the instructions listed below for each component.

## Unity Setup
1. Install Unity Hub from the official [website](https://unity.com/download).
2. From the Unity Hub install the Unity LTS version for 2022 or higher.
3. Clone the [repository](https://github.com/UM-LPM/GIANT/) on your local machine from the GitHub.
4. Add and open the cloned project in the Unity Hub.
5. Configure Unity Editor (Edit -> Project Settings -> Physics) if the settings don't match the ones displayed in the image below.
   
   ![Unity Editor Configuration](/docs/images/unity_editor_config.png)
6. To test run the Unity setup, navigate to **Assets -> Problem Domains -> Robostrike** and open **RobostrikeBaseScene** and in your API platform of choice create the following request:
   
   ```json
    POST request to: http://localhost:4000
    {
      "EvalEnvInstances": ["http://localhost:4444/"],
      "EvalRangeStart": 0,
      "EvalRangeEnd": 2
    }
   ```
7. The response from the request should look something like this:

   ```json
     {
        "IndividualFitnesses": [
            {
                "IndividualID": 0,
                "Value": -1.0,
                "IndividualMatchResults": [
                    {
                        "OpponentsIDs": [
                            1
                        ],
                        "MatchName": "0_RobostrikeGameScene_RobostrikeAgentScene_e69e2f5d-181e-43ed-9d35-9b30ac970d26",
                        "Value": -15.1665993,
                        "IndividualValues": {
                            "SectorExploration": -2.6666,
                            "SurvivalBonus": -10.0,
                            "PowerUp_Pickup_Shield": -2.5
                        }
                    },
                    {
                        "OpponentsIDs": [
                            1
                        ],
                        "MatchName": "0_RobostrikeGameScene_RobostrikeAgentScene_e58332d9-5300-471d-b3ba-9635a7ba7b6d",
                        "Value": -15.1665993,
                        "IndividualValues": {
                            "SectorExploration": -2.6666,
                            "PowerUp_Pickup_Shield": -2.5,
                            "SurvivalBonus": -10.0
                        }
                    }
                ],
                "AvgMatchResult": {
                    "OpponentsIDs": null,
                    "MatchName": "Avg Match Result",
                    "Value": 0.0,
                    "IndividualValues": {
                        "SectorExploration": -2.6666,
                        "SurvivalBonus": -10.0,
                        "PowerUp_Pickup_Shield": -2.5
                    }
                },
                "AdditionalValues": null
            },
            {
                "IndividualID": 1,
                "Value": 0.0,
                "IndividualMatchResults": [
                    {
                        "OpponentsIDs": [
                            0
                        ],
                        "MatchName": "0_RobostrikeGameScene_RobostrikeAgentScene_e69e2f5d-181e-43ed-9d35-9b30ac970d26",
                        "Value": -15.1665993,
                        "IndividualValues": {
                            "SectorExploration": -2.6666,
                            "PowerUp_Pickup_Shield": -2.5,
                            "SurvivalBonus": -10.0
                        }
                    },
                    {
                        "OpponentsIDs": [
                            0
                        ],
                        "MatchName": "0_RobostrikeGameScene_RobostrikeAgentScene_e58332d9-5300-471d-b3ba-9635a7ba7b6d",
                        "Value": -15.1665993,
                        "IndividualValues": {
                            "SectorExploration": -2.6666,
                            "SurvivalBonus": -10.0,
                            "PowerUp_Pickup_Shield": -2.5
                        }
                    }
                ],
                "AvgMatchResult": {
                    "OpponentsIDs": null,
                    "MatchName": "Avg Match Result",
                    "Value": 0.0,
                    "IndividualValues": {
                        "SectorExploration": -2.6666,
                        "PowerUp_Pickup_Shield": -2.5,
                        "SurvivalBonus": -10.0
                    }
                },
                "AdditionalValues": null
            }
        ]
    }
   ```

### Building Unity project (Windows)

1. The project can be built by navigating to File -> Build Settings, select the "Windows" platform and press **`Build`**.
    ![Unity Windows build settings 1](/docs/images/build_settings_windows_1.png)
2. After the build, the **`GIANT.exe`** file and other data files will be located in the selected destination folder.
    ![Unity Windows build settings 2](/docs/images/build_settings_windows_2.png)
3. The **`conf.json`** file should be placed in subfolder **`GIANT_DATA`**.
4. Program can be run by executing the GIANT.exe file. For **`headless`** mode, use the following command:
    ```bash
    ./GIANT.exe  -batchmode -nographics # No GUI
    ``` 

### Building Unity project (Linux)

1. In Unity Hub, under **`Installs`** select your Unity version and add the following module:
    - "Linux Build Support (either IL2CPP or Mono version can be selected)"
    ![Unity Linux build settings 1](/docs/images/build_settings_linux_1.png)
2. In Unity Editor configure **`Player Setings`** and select the appropriate **`Scripting Backend`**.
    ![Unity Linux build settings 2](/docs/images/build_settings_linux_2.png)
3. After the build, the **`GIANT.x86_64`** file and other data files will be in the selected destination folder.
    ![Unity Windows build settings 3](/docs/images/build_settings_linux_3.png) 
4. The **`conf.json`** file should be placed in subfolder **`GIANT_DATA`**.
5. To enable visualization, run the following command to set the **`DISPLAY`** variable:
    ```bash
    export DISPLAY=$(cat /etc/resolv.conf | grep nameserver | awk '{print $2}'):0
    ```
6. Program can be run with the following commands:
    ```bash
    ./GIANT.x86_64  # With GUI
    ./GIANT.x86_64  -batchmode -nographics # No GUI
    ``` 

## Web API Setup

1. Install [MS VisualStudio](https://visualstudio.microsoft.com/downloads/) or any other code editor that supports reading .cs files.
2. In the cloned repository navigate to the WebAPI folder, open WebAPI.sln, and start the API.
3. Navigate to **`http://localhost:5016/swagger/index.html`** to see if the API ran successfully.
4. API can be tested with a POST request to the http://localhost:5016/api/JsonToSoParser with the following request body:

   ```json
      {
          "CoordinatorURI": "http://localhost:4000",
          "EvalEnvInstanceURIs": ["http://localhost:4444"],
          "SourceFilePath": "your path...\\jsonBody.json",
          "DestinationFilePath": "your path...\\GIANT\\EvaluationEnironments\\UnityEvalEnv\\Assets\\Resources\\JSONs\\problem domain\\",
          "LastEvalIndividualFitnesses": null
      }
   ```

### Building Web API (Windows)

1. In the VisualStudio, after opening the WebAPI.sln, select build option (Debug, Release) and build the project.

### Building Web API (Linux)

1. Install the following dependencies:
    ```
    sudo apt-get update &&   sudo apt-get install -y dotnet-sdk-8.0
    sudo apt-get update && sudo apt-get install -y aspnetcore-runtime-8.0
     sudo apt-get install -y dotnet-runtime-8.0
    ```
2. Run **`dotnet build`** or **`dotnet run`** if you want to build or run the Web API.

## Machine Learning Framework (EARS) Setup

1. Install [IntelliJ IDEA](https://www.jetbrains.com/idea/) on your machine.
2. Clone [EARS repository](https://github.com/UM-LPM/EARS.git) from GitHub.
3. Open the cloned project in IntelliJ IDEA
4. Create a run configuration (If not already defined).
   ![EARS Run Configuration](/docs/images/ears_run_configuration.png)
5. Update file ./.idea/gradle.xml.

   ```xml
      <?xml version="1.0" encoding="UTF-8"?>
      <project version="4">
         <component name="GradleSettings">
            <option name="linkedExternalProjectsSettings">
               <GradleProjectSettings>
               <option name="delegatedBuild" value="false" /> <!--Add this line of code-->
               <option name="distributionType" value="DEFAULT_WRAPPED" />
               <option name="externalProjectPath" value="$PROJECT_DIR$" />
               <option name="gradleJvm" value="#JAVA_HOME" />
               <option name="modules">
                  <set>
                     <option value="$PROJECT_DIR$" />
                  </set>
               </option>
               </GradleProjectSettings>
            </option>
         </component>
      </project>
   ```
   
6. Update ./.idea/misc.xml.

   ```xml
   <component name="ProjectRootManager" version="2" languageLevel="JDK_16" project-jdk-name="17" project-jdk-type="JavaSDK" >
    <output url="file://$PROJECT_DIR$/out" /> <!--Add this line of code-->
   </component>
   ```
   
7. Install Swing UI Designer
8. Start WebAPI and run GPInterface configuration. 
9. Load **`ears_config_robostrike.json`** in the **Configuration** tab.
10. Press **Run** to start the evolution process. After the process is successfully executed the result should be displayed in the UI.

# Next Steps

- Read the following documentation to learn more about [Evaluation Environments](/docs/GIANT_evaluation_environment_overview.md), [Web API](/docs/GIANT_webapi_overview.md), and [Machine Learning Frameworks](/docs/GIANT_machine_learning_framework_overview.md).
- If you're already familiar with these, you can continue by checking all [existing problem domains](/docs/GIANT_problem_domains.md) or [adding a new problem domain](/docs/GIANT_add_new_problem_domain.md)  in the platform.


















