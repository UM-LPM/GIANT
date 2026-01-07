using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "ADiSAgentController", menuName = "AgentControllers/AIAgentControllers/ADiSAgentController")]
    public class ADiSAgentController : AIAgentController
    {
        public List<Connection> Connections = new List<Connection>();

        public List<ADiSComponent> Components = new List<ADiSComponent>();

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            throw new NotImplementedException();
        }

        public override AgentController Clone()
        {
            var clone = Instantiate(this);
            clone.Connections = new List<Connection>();
            foreach (var connection in Connections)
            {
                clone.Connections.Add(connection.Clone() as Connection);
            }

            return clone;
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {

            // 1. Find all activated connections
            var activatedConnections = new List<Connection>();

            foreach (var connection in Connections)
            {
                connection.ToggleIsExecuting(false);
                if (connection.IsActivated())
                {
                    activatedConnections.Add(connection);
                }
            }

            // 2. From all activated connections get the one with the highest weight (or first if multiple have the same weight)
            var selectedConnection = activatedConnections
                .OrderByDescending(c => c.Weight)
                .FirstOrDefault();

            // 3. Get and set the actions from the selected connection
            if(selectedConnection != null)
            {
#if UNITY_EDITOR
                selectedConnection.IsActivated(); // Enable this only in editor to show active activators
#endif
                selectedConnection.ToggleIsExecuting(true);
                selectedConnection.GetActions(actionsOut);
            }
        }

        public void BindAndInit(Context context)
        {
            foreach (var connection in Connections)
            {
                connection.BindAndInit(context);
            }
        }

        public static Context CreateADiSContext(GameObject agentGameObject)
        {
            return Context.CreateFromGameObject(agentGameObject);
        }

        public ADiSComponent CreateComponent(System.Type type)
        {
            ADiSComponent component = ScriptableObject.CreateInstance(type) as ADiSComponent;
            component.name = type.Name;
            component.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "ADiS (CreateComponent)");
            if(component is Connection connection)
            {
                Connections.Add(connection);
            }

            Components.Add(component);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(component, this);
            }

            Undo.RegisterCreatedObjectUndo(component, "ADiS (CreateComponent)");

            AssetDatabase.SaveAssets();
            return component;
        }

        public void DeleteComponent(ADiSComponent component)
        {
            Undo.RecordObject(this, "ADiS (DeleteComponent)");
            Components.Remove(component);

            Undo.DestroyObjectImmediate(component);

            AssetDatabase.SaveAssets();
        }

        public ActivatorConnection AddComponent(ADiSComponent parent, ADiSComponent child)
        {
            if (parent is Connection connection && child is Action action)
            {
                connection.Actions.Add(action);
                return null;
            }

            if (parent is Activator activator && child is Connection targetConnection)
            {
                var activatorConnection = CreateActivatorConnection(activator);
                targetConnection.ActivatorConnections.Add(activatorConnection);

                return activatorConnection;
            }

            return null;
        }

        private ActivatorConnection CreateActivatorConnection(Activator activator)
        {
            var ac = ScriptableObject.CreateInstance<ActivatorConnection>();
            ac.Activator = activator;
            ac.IsNegated = false;
            ac.name = "ActivatorConnection";
            ac.guid = GUID.Generate().ToString();

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(ac, this);
                AssetDatabase.SaveAssets();
            }

            return ac;
        }

        public void RemoveComponent(ADiSComponent parent, ADiSComponent child)
        {
            if(parent is Connection connection1 && child is Action action1)
            {
                connection1.Actions.Remove(action1);
                return;
            }
            
            if (parent is Activator activator1 && child is Connection connection2)
            {
                var activatorConnection = connection2.ActivatorConnections.Where(ac => ac.Activator == activator1).First();
                connection2.ActivatorConnections.Remove(activatorConnection);
                AssetDatabase.RemoveObjectFromAsset(activatorConnection);
            }
        }
    }
}