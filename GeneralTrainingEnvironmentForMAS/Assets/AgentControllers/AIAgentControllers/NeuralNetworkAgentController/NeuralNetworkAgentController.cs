using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors;

namespace AgentControllers.AIAgentControllers.NeuralNetworkAgentController
{
    [CreateAssetMenu(menuName = "AgentControllers/AIAgentControllers/NeuralNetworkAgentController")]
    public class NeuralNetworkAgentController : AIAgentController
    {
        public ObservationCollector ObservationCollector { get; set; }
        public NNModel ModelAsset;

        private Model Model;
        private IWorker Worker;

        public override void Initialize(Dictionary<string, object> initParams)
        {
            if(ModelAsset == null)
            {
                throw new System.Exception("ModelAsset is not set!");
                // TODO: Add error reporting here
            }

            if (!initParams.ContainsKey("observationCollector"))
            {
                throw new System.Exception("observationCollector is not set!");
                // TODO: Add error reporting here
            }

            ObservationCollector = (ObservationCollector)initParams["observationCollector"];
            Model = ModelLoader.Load(ModelAsset);
            Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, Model);
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {
            Observation observation = ObservationCollector.CollectObservation();
            RunInference(actionsOut, observation.ObservationDataTensor, observation.ActionMasksDataTensor);
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
                ObservationCollector.MapModelPredictionToActionBuffer(in actionsOut, discreteActions, null);
                discreteActions.Dispose();
            }
            catch (System.Exception ex)
            {
                // TODO: Add error reporting here
            }

            try
            {
                Tensor continousActions = Worker.PeekOutput("continuous_actions");
                //actionsOut.ContinuousActions = continousActions.ToReadOnlyArray();
                continousActions.Dispose();
            }
            catch (System.Exception ex)
            {
                // TODO: Add error reporting here
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
    }
}