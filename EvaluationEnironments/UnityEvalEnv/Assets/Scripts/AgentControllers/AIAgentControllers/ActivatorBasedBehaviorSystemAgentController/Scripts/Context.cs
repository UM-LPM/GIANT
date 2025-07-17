using UnityEngine;
using UnityEngine.AI;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    // The context is a shared object every node has access to.
    // Commonly used components and subsytems should be stored here
    public class Context
    {
        public GameObject gameObject;
        public Transform transform;
        public Rigidbody physics;

        public static Context CreateFromGameObject(GameObject gameObject)
        {
            // Fetch all commonly used components
            Context context = new Context();
            context.gameObject = gameObject; // Entity
            context.transform = gameObject.transform;
            context.physics = gameObject.GetComponent<Rigidbody>();

            return context;
        }
    }
}