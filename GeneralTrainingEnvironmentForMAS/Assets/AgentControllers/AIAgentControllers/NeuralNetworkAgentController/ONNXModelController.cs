using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors;
using AITechniques.BehaviorTrees;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class ONNXModelController : MonoBehaviour
{
    public AgentController AgentController;

    private ActionExecutor ActionExecutor;

    void Start()
    {
        if (AgentController is NeuralNetworkAgentController)
        {
            Dictionary<string, object> initParams = new Dictionary<string, object>
            {
                { "observationCollector", new DummyObservationCollector(85, 6, true) }
            };

            AgentController.Initialize(initParams);
        }
        else if(AgentController is BehaviorTreeAgentController)
        {
            AgentController = ((BehaviorTreeAgentController)AgentController).Clone();
            ((BehaviorTreeAgentController)AgentController).Bind(BehaviourTree.CreateBehaviourTreeContext(gameObject));
            ((BehaviorTreeAgentController)AgentController).InitNodeCallFrequencyCounter();
        }

        ActionExecutor = GetComponent<ActionExecutor>();
        if(ActionExecutor == null)
        {
            throw new System.Exception("ActionExecutor component not found on the Agent.");
            // TODO: Add error reporting here
        }
    }

    public void FixedUpdate()
    {
        ActionBuffer actionBuffer = new ActionBuffer();
        AgentController.GetActions(in actionBuffer);

        ActionExecutor.ExecuteActions(actionBuffer);
    }


    /*public NNModel modelAsset;
    private Model model;
    private IWorker worker;

    private Rigidbody Rigidbody;

    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
        Rigidbody = GetComponent<Rigidbody>();
    }

    // Method to run inference
    public void RunInference(float[] obs0Data, float[] actionMasksData)
    {
        // Prepare input tensors
        Tensor obs0Tensor = new Tensor(1, 1, 1, 85, obs0Data);
        Tensor actionMasksTensor = new Tensor(1, 1, 1, 6, actionMasksData);

        // Set inputs
        worker.SetInput("obs_0", obs0Tensor);
        worker.SetInput("action_masks", actionMasksTensor);

        // Run inference
        worker.Execute();

        // Retrieve outputs
        Tensor versionNumber = worker.PeekOutput("version_number");
        Tensor memorySize = worker.PeekOutput("memory_size");
        //Tensor discreteActions = worker.PeekOutput("discrete_actions");
        //Tensor continousActions = worker.PeekOutput("continuous_actions");
        Tensor discreteActionOutputShape = worker.PeekOutput("discrete_action_output_shape");
        Tensor deterministicDiscreteActions = worker.PeekOutput("deterministic_discrete_actions");

        ActionBuffer actionBuffer = new ActionBuffer(new float[] { }, new int[] { });

        try
        {
            Tensor discreteActions = worker.PeekOutput("discrete_actions");
            actionBuffer.DiscreteActions = TensorToIntArray(discreteActions);
            discreteActions.Dispose();
        }
        catch(System.Exception ex)
        {}

        try
        {
            Tensor continousActions = worker.PeekOutput("continuous_actions");
            actionBuffer.ContinuousActions = continousActions.ToReadOnlyArray();
            continousActions.Dispose();
        }
        catch (System.Exception ex)
        { }

        // Process or use the outputs as needed
        Debug.Log("Version Number: " + versionNumber[0]);
        Debug.Log("Memory Size: " + memorySize[0]);
        Debug.Log("Discrete Actions: " + actionBuffer.DiscreteActions[0] + ", " + actionBuffer.DiscreteActions[1]);
        //Debug.Log("Continous Actions: " + continousActions.length);
        Debug.Log("Deterministic Discrete Actions: " + deterministicDiscreteActions[0] + ", " + deterministicDiscreteActions[1]);


        // Move Agent based on the output
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rotateAxis = actionBuffer.DiscreteActions[1];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = this.transform.forward * 1;
                break;
            case 2:
                dirToGo = this.transform.forward * -1;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = this.transform.up * -1f;
                break;
            case 2:
                rotateDir = this.transform.up * 1f;
                break;
        }

        // Movement Version 2
        this.Rigidbody.MovePosition(this.Rigidbody.position + (dirToGo * 10 * Time.fixedDeltaTime));
        Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * 100, 0.0f);
        this.Rigidbody.MoveRotation(this.Rigidbody.rotation * turnRotation);

        // Dispose tensors
        obs0Tensor.Dispose();
        actionMasksTensor.Dispose();
        versionNumber.Dispose();
        memorySize.Dispose();
        //discreteActions.Dispose();
        discreteActionOutputShape.Dispose();
        deterministicDiscreteActions.Dispose();
    }

    void OnDestroy()
    {
        worker.Dispose();
    }

    public void CallRunInference()
    {
        // Prepare example input data
        float[] obs0Data = new float[85]; // Replace with actual input data for 'obs_0'
        float[] actionMasksData = new float[6]; // Replace with actual input data for 'action_masks'

        // Randomly initialize this values
        for (int i = 0; i < obs0Data.Length; i++)
        {
            obs0Data[i] = Random.Range(0.0f, 1.0f);
        }

        for (int i = 0; i < actionMasksData.Length; i++)
        {
            actionMasksData[i] = Random.Range(0.0f, 1.0f);
        }

        // Run inference
        RunInference(obs0Data, actionMasksData);
    }

    public void FixedUpdate()
    {
        CallRunInference();
    }

    public int[] TensorToIntArray(Tensor tensor)
    {
        float[] floatArray = tensor.ToReadOnlyArray();
        int[] intArray = new int[floatArray.Length];

        for (int i = 0; i < floatArray.Length; i++)
        {
            intArray[i] = Mathf.RoundToInt(floatArray[i]); // or (int)floatArray[i] for truncation
        }

        return intArray;
    }*/
}
