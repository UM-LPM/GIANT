using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ActionObservationCollectors;
using Unity.VisualScripting;
using System;
using UnityEditor;

namespace AgentControllers.AIAgentControllers.NeuralNetworkAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "NeuralNetworkAgentController", menuName = "AgentControllers/AIAgentControllers/NeuralNetworkAgentController")]
    public class NeuralNetworkAgentController : AIAgentController
    {
        public NNModel ModelAsset;
        public ActionObservationProcessor ActionObservationProcessor { get; set; }

        private Model Model;
        private IWorker Worker;

        public override void Initialize(Dictionary<string, object> initParams)
        {
            if (ModelAsset == null)
            {
                throw new System.Exception("ModelAsset is not set!");
                // TODO Add error reporting here
            }

            if (!initParams.ContainsKey("actionObservationProcessor"))
            {
                throw new System.Exception("ActionObservationProcessor is not set!");
                // TODO Add error reporting here
            }

            ActionObservationProcessor = (ActionObservationProcessor)initParams["actionObservationProcessor"];
            Model = ModelLoader.Load(ModelAsset);
            Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, Model);
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {
            Observation observation = ActionObservationProcessor.CollectObservation();
            RunInference(actionsOut, observation.ObservationDataTensor, observation.ActionMasksDataTensor);
        }

        override public AgentController Clone()
        {
            NeuralNetworkAgentController clone = Instantiate(this);
            clone.ModelAsset = ModelAsset;
            if (ActionObservationProcessor != null)
                clone.ActionObservationProcessor = ActionObservationProcessor.Clone();
            return clone;
        }

        public void RunInference(ActionBuffer actionsOut, Tensor obs0Tensor, Tensor actionMasksTensor)
        {
            // Set inputs
            Worker.SetInput("obs_0", obs0Tensor);
            Worker.SetInput("action_masks", actionMasksTensor);

            // Run inference
            Worker.Execute();

            try
            {
                Tensor discreteActions = Worker.PeekOutput("discrete_actions");
                ActionObservationProcessor.MapModelPredictionToActionBuffer(in actionsOut, discreteActions, null);
                discreteActions.Dispose();
            }
            catch (System.Exception ex)
            {
                // TODO Add error reporting here
            }

            try
            {
                Tensor continousActions = Worker.PeekOutput("continuous_actions");
                //actionsOut.ContinuousActions = continousActions.ToReadOnlyArray();
                continousActions.Dispose();
            }
            catch (System.Exception ex)
            {
                // TODO Add error reporting here
            }
        }

        public static int[] TensorToIntArray(Tensor tensor)
        {
            float[] floatArray = tensor.ToReadOnlyArray();
            int[] intArray = new int[floatArray.Length];

            for (int i = 0; i < floatArray.Length; i++)
            {
                intArray[i] = Mathf.RoundToInt(floatArray[i]); // or (int)floatArray[i] for truncation
            }

            return intArray;
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            AssetDatabase.AddObjectToAsset(ModelAsset, parent);
        }
    }
}