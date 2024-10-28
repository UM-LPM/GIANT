using Unity.Barracuda;

namespace AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors
{
    /// <summary>
    /// Dummy observation collector for testing purposes. It randomly generates dummy observation data and action masks data.
    /// </summary>
    public class DummyObservationCollector : ObservationCollector
    {
        public int ObservationDataSize { get; private set; }
        public int ActionMasksDataSize { get; private set; }
        public bool SetRandomData { get; private set; }

        private float[] observationData;
        private float[] actionMasksData;

        public DummyObservationCollector(int observationDataSize, int actionMasksDataSize, bool setRandomData)
        {
            ObservationDataSize = observationDataSize;
            ActionMasksDataSize = actionMasksDataSize;
            SetRandomData = setRandomData;

            observationData = new float[ObservationDataSize];
            actionMasksData = new float[ActionMasksDataSize];
        }

        public override Observation CollectObservation()
        {
            Observation observation = new Observation();

            // Dummy observation data
            for (int i = 0; i < ObservationDataSize; i++)
            {
                if(SetRandomData)
                {
                    observationData[i] = UnityEngine.Random.Range(0f, 1f);
                }
                else
                {
                    observationData[i] = i;
                }
            }

            // Dummy action masks data
            for (int i = 0; i < ActionMasksDataSize; i++)
            {
                if(SetRandomData)
                {
                    actionMasksData[i] = UnityEngine.Random.Range(0f, 1f);
                }
                else
                {
                    actionMasksData[i] = 1;
                }
            }

            // Prepare input tensors
            observation.ObservationDataTensor = new Tensor(1, 1, 1, ObservationDataSize, observationData);
            observation.ActionMasksDataTensor = new Tensor(1, 1, 1, ActionMasksDataSize, actionMasksData);

            return observation;
        }

        public override void MapModelPredictionToActionBuffer(in ActionBuffer actionBuffer, Tensor discreteActions, Tensor continousActions)
        {
            if(discreteActions != null && discreteActions.length != 2)
            {
                throw new System.Exception("Discrete actions tensor must have a length of 2!");
                // TODO Add error reporting here
            }

            actionBuffer.DiscreteActions.Add("moveForwardDirection", (int)discreteActions[0]);
            actionBuffer.DiscreteActions.Add("rotateDirection", (int)discreteActions[1]);
        }
    }
}