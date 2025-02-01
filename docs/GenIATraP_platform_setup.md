# Platform Setup (Developer)

The platform consists of three parts: Unity, Web API, and EARS framework. To set up the platform follow the instructions listed below for each component.

## Unity Setup
1. Install Unity Hub from the official [website](https://unity.com/download).
2. From the Unity Hub install the LTS Unity version for 2022 or higher.
3. Clone the [repository](https://github.com/UM-LPM/GeneralTrainingEnvironmentForMAS/tree/platform_refactor) on your local machine from the GitHub.
4. Add and open the cloned project in the Unity Hub.
5. Configure Unity Editor (Edit -> Project Settings -> Physics) if the settings don't match the ones displayed in the image below.
   
   ![Unity Editor Configuration](/docs/images/unity_editor_config.png)
6. To test run the Unity setup, navigate to **Assets -> Problem Domains -> Robostrike** and open **RobostrikeBaseScene** and in your API platform of choice create the following request:
   
   ```
    POST request to: http://localhost:4000
    {
      "EvalEnvInstances": ["http://localhost:4444/"],
      "EvalRangeStart": 0,
      "EvalRangeEnd": 2
    }
   ```
7. The response from the request should look something like this:

   ```
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

## Web API Setup

## Machine Learning framework (EARS) Setup
