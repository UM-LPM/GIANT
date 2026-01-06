using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Context
    {
        public GameObject gameObject;
        public Transform transform;

        public static Context CreateFromGameObject(GameObject gameObject)
        {
            // Fetch all commonly used components
            Context context = new Context();
            context.gameObject = gameObject; // Entity
            context.transform = gameObject.transform;

            return context;
        }
    }
}
