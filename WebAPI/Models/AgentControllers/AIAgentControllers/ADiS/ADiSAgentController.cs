using System;
using WebAPI.Models.EARS;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public class ADiSAgentController : AIAgentController
    {
        public List<Connection> Connections = new List<Connection>();
        public List<ADiSComponent> Components = new List<ADiSComponent>();

        public ADiSAgentController(string name)
            : base(name)
        {
        }

        public override void MapProgramToAgentController(ProgramSolutionPart programPart)
        {
            if (programPart == null)
            {
                return;
            }

            if(programPart is ADiS adis)
            {
                Components.Clear();
                Connections.Clear();

                // Map Actions and Activators
                foreach(var action in adis.Actions)
                {
                    Components.Add(Action.GetAction(action.Guid, action.Name!, action.Properties, action.NodePosition));
                }

                foreach(var activator in adis.Activators)
                {
                    Components.Add(Activator.GetActivator(activator.Guid, activator.Name!, activator.Properties, activator.NodePosition));
                }

                // Map Connections
                foreach (var connection in adis.Connections)
                {
                    Connection newConnection = new Connection(Guid.NewGuid(), connection.Weight, connection.NodePosition);
                    // Map Actions
                    if(connection.Actions != null)
                    {
                        connection.Actions.ForEach(action =>
                        {
                            var foundAction = Components.OfType<Action>().FirstOrDefault(a => a.guid == action.ToString());
                            if (foundAction == null)
                                throw new Exception($"Action with GUID {action} not found in components.");

                            newConnection.Actions.Add(foundAction);
                        });
                    }

                    // Map ActivatorConnections
                    if(connection.ActivatorConnections != null)
                    {
                        connection.ActivatorConnections.ForEach(activatorConnection =>
                        {
                            var activator = Components.OfType<Activator>().FirstOrDefault(a => a.guid == activatorConnection.Activator.ToString());
                            if (activator == null)
                                throw new Exception($"Activator with GUID {activatorConnection} not found in components.");

                            newConnection.ActivatorConnections.Add(new ActivatorConnection(Guid.NewGuid(), activator, activatorConnection.IsNegated));
                        });
                    }
                    Connections.Add(newConnection);
                    Components.Add(newConnection);
                }
            }
        }

    }
}
