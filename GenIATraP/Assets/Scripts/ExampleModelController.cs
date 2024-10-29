using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using System.Collections.Generic;
//using Unity.Barracuda;
using UnityEngine;

public class ExampleModelController : MonoBehaviour // TODO Remove in the future
{
    public AgentComponent[] AgentComponents { get; set; }

    private void Awake()
    {
        AgentComponents = GetComponentsInChildren<AgentComponent>();
    }

    private void Start()
    {
        foreach (AgentComponent agentComponent in AgentComponents)
        {
            if (agentComponent.AgentController == null)
            {
                throw new System.Exception("AgentController is not set!");
                // TODO Add error reporting here
            }

            // Different initialization for different types of AgentControllers
            switch (agentComponent.AgentController)
            {
                case NeuralNetworkAgentController:
                    // Problem specific initialization parameters
                    agentComponent.AgentController.Initialize(new Dictionary<string, object>
                    {
                        { "observationCollector", new DummyActionObservationProcessor(85, 6, true) }
                    });
                    break;
                case BehaviorTreeAgentController:
                    agentComponent.AgentController = ((BehaviorTreeAgentController)agentComponent.AgentController).Clone();
                    ((BehaviorTreeAgentController)agentComponent.AgentController).Bind(BehaviorTreeAgentController.CreateBehaviourTreeContext(agentComponent.gameObject));
                    ((BehaviorTreeAgentController)agentComponent.AgentController).InitNodeCallFrequencyCounter();
                    break;
                // TODO Implement other AgentController types
                default:
                    throw new System.Exception("Unknown AgentController type!");
                    // TODO Add error reporting here
            }
        }
    }

    public void FixedUpdate()
    {
        ActionBuffer actionBuffer = new ActionBuffer();
        foreach (AgentComponent agentComponent in AgentComponents)
        {
            agentComponent.AgentController.GetActions(in actionBuffer);
            agentComponent.ActionExecutor.ExecuteActions(actionBuffer);

            actionBuffer.ResetActions();
        }
    }
}
