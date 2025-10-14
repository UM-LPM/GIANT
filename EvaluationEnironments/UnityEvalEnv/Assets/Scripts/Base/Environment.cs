using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Base
{
    public class Environment
    {
        public EnvironmentControllerBase EnvironmentController { get; private set; }

        public Scene Scene { get; private set; }
        public PhysicsScene PhysicsScene { get; private set; }
        public PhysicsScene2D PhysicsScene2D { get; private set; }
        public GameObject RootObject { get; private set; }
        private bool IsDebug = false;


        public Environment(GameObject environmentPrefab, string sceneName, Match match, bool isDebug = false)
        {
            // Create independent physics scene
            EnvironmentControllerBase tempEnvController = environmentPrefab.GetComponent<EnvironmentControllerBase>();
            if (tempEnvController == null)
            {
                DebugSystem.LogError("Environment prefab must have an EnvironmentControllerBase component.");
                return;
            }

            var parameters = new CreateSceneParameters(tempEnvController.GameType == GameType._2D ? LocalPhysicsMode.Physics2D : LocalPhysicsMode.Physics3D);
            Scene = SceneManager.CreateScene(sceneName, parameters);
            PhysicsScene = Scene.GetPhysicsScene();
            PhysicsScene2D = Scene.GetPhysicsScene2D();

            // Instantiate environment prefab into this scene
            RootObject = Object.Instantiate(environmentPrefab);
            SceneManager.MoveGameObjectToScene(RootObject, Scene);


            EnvironmentController = RootObject.GetComponent<EnvironmentControllerBase>();
            if (EnvironmentController == null)
            {
                DebugSystem.LogError("Environment prefab must have an EnvironmentControllerBase component.");
            }

            EnvironmentController.Match = match;
            EnvironmentController.PhysicsScene = PhysicsScene;
            EnvironmentController.PhysicsScene2D = PhysicsScene2D;

            IsDebug = isDebug;
        }

        public void Step()
        {
            EnvironmentController.OnStep();
        }

        public void ResetEnvironment()
        {
            foreach (var resettable in RootObject.GetComponentsInChildren<IResettable>())
            {
                resettable.OnReset();
            }
        }

        public bool IsDone()
        {
            return EnvironmentController.IsSimulationFinished();
        }

        public void Terminate()
        {
            if (Scene.IsValid())
            {
                if(IsDebug)
                    DebugSystem.Log($"Terminating environment: {Scene.name}");
                foreach (var rootObj in Scene.GetRootGameObjects())
                {
                    // Print all child objects being destroyed
                    if(IsDebug)
                        foreach (var obj in rootObj.GetComponentsInChildren<Transform>())
                        {
                            DebugSystem.Log($"Destroying object: {obj.name}, position: {obj.position}");
                        }
                    Object.Destroy(rootObj);
                }
                SceneManager.UnloadSceneAsync(Scene);
            }
        }
    }
}