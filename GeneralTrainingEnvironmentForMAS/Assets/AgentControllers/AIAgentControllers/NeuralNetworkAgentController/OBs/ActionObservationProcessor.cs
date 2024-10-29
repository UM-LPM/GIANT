using Unity.Barracuda;

namespace AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors
{
    /// <summary>
    /// Class that collects the observation data and action masks data from the environment. Every problem needs to implement its own observation collector.
    /// </summary>
    [System.Serializable]
    public abstract class ActionObservationProcessor // TODO Rename this
    {
        /// <summary>
        /// Collects the observation data and action masks data from the environment.
        /// </summary>
        public abstract Observation CollectObservation();

        public abstract void MapModelPredictionToActionBuffer(in ActionBuffer actionBuffer, Tensor discreteActions, Tensor continousActions);
    }

    public class Observation
    {
        public Tensor ObservationDataTensor { get; set; }
        public Tensor ActionMasksDataTensor { get; set; }
    }
}