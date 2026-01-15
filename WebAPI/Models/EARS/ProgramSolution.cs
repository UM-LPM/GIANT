using AgentControllers;
using AgentControllers.AIAgentControllers.ADiSAgentController;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using JsonSubTypes;
using Newtonsoft.Json;
using static AgentControllers.AIAgentControllers.BehaviorTreeAgentController.BTNode;

namespace WebAPI.Models.EARS
{
    public class ProgramSolution
    {
        public int ID { get; set; }
        public List<ProgramSolutionPart> SolutionParts { get; set; }

        public ProgramSolution()
        {
            SolutionParts = new List<ProgramSolutionPart>();
        }
    }

    [JsonConverter(typeof(JsonSubtypes), "Name")]
    [JsonSubtypes.KnownSubType(typeof(BehaviorTree), "BehaviorTree")]
    [JsonSubtypes.KnownSubType(typeof(ADiS), "ADiS")]
    public abstract class ProgramSolutionPart
    { 
        public int ID { get; set; }

        public abstract void Configure();
        public abstract AgentController MapToAgentController();
    }

    public abstract class Tree : ProgramSolutionPart
    { }

    public class BehaviorTree : Tree
    {
        public string Name { get; set; }
        public BTProgramSolutionPartNode RootNode { get; set;}

        public override void Configure()
        {
            // Update node IDs
            BTProgramSolutionPartNode.UpdateNoteIDs(RootNode!);

            // Update node positions
            BTProgramSolutionPartNode.UpdateNodePositions(RootNode);
        }

        public override AgentController MapToAgentController()
        {
            BehaviorTreeAgentController agentController = new BehaviorTreeAgentController("BehaviorTreeAgentController");
            agentController.MapProgramToAgentController(this);

            return agentController;
        }
    }

    public class ADiS : ProgramSolutionPart
    {
        public string Name { get; set; }
        public List<ProgramSolutionPartActivator> Activators { get; set; }
        public List<ProgramSolutionPartAction> Actions { get; set; }
        public List<ProgramSolutionPartConnection> Connections { get; set; }

        // Layout parameters
        const int connectionSpacing = 300;
        const int minNodeSpacing = 180; // must be >= max node width

        const int connectionY = 0;
        const int activatorY = -250;
        const int actionY = 250;

        const int startX = 0;

        public override void Configure()
        {
            // Update node positions
            UpdateNodePositions();
        }

        public override AgentController MapToAgentController()
        {
            ADiSAgentController agentController = new ADiSAgentController("ADiSAgentController");
            agentController.AgentControllerId = ID;
            agentController.MapProgramToAgentController(this);

            return agentController;
        }

        public void UpdateNodePositions()
        {
            if (Connections == null || Activators == null || Actions == null)
                return;

            var activatorByGuid = Activators.ToDictionary(a => a.Guid);
            var actionByGuid = Actions.ToDictionary(a => a.Guid);

            var activatorDesiredX = new Dictionary<Guid, List<int>>();
            var actionDesiredX = new Dictionary<Guid, List<int>>();

            // -------------------------------------------------
            // 1. Place Connections (anchors, no overlap)
            // -------------------------------------------------
            for (int i = 0; i < Connections.Count; i++)
            {
                int cx = startX + i * connectionSpacing;
                Connections[i].NodePosition = new Position(cx, connectionY);

                if (Connections[i].ActivatorConnections != null)
                {
                    foreach (var ac in Connections[i].ActivatorConnections)
                    {
                        if (!activatorByGuid.ContainsKey(ac.Activator))
                            continue;

                        activatorDesiredX.TryAdd(ac.Activator, new List<int>());
                        activatorDesiredX[ac.Activator].Add(cx);
                    }
                }

                if (Connections[i].Actions != null)
                {
                    foreach (var actionGuid in Connections[i].Actions)
                    {
                        if (!actionByGuid.ContainsKey(actionGuid))
                            continue;

                        actionDesiredX.TryAdd(actionGuid, new List<int>());
                        actionDesiredX[actionGuid].Add(cx);
                    }
                }
            }

            // -------------------------------------------------
            // 2. Place Activators (single row, collision-free)
            // -------------------------------------------------
            PlaceRow(
                Activators,
                activatorDesiredX,
                activatorY,
                startX - connectionSpacing
            );

            // -------------------------------------------------
            // 3. Place Actions (single row, collision-free)
            // -------------------------------------------------
            PlaceRow(
                Actions,
                actionDesiredX,
                actionY,
                startX + Connections.Count * connectionSpacing + connectionSpacing
            );
        }

        private void PlaceRow<T>(
            List<T> nodes,
            Dictionary<Guid, List<int>> desiredXs,
            int y,
            int fallbackX
        ) where T : class
        {
            var ordered = nodes
                .Select(node =>
                {
                    Guid guid = (Guid)node.GetType().GetProperty("Guid")!.GetValue(node)!;

                    int preferredX = desiredXs.TryGetValue(guid, out var xs)
                        ? xs.Sum() / xs.Count
                        : fallbackX;

                    return new
                    {
                        Node = node,
                        PreferredX = preferredX
                    };
                })
                .OrderBy(n => n.PreferredX)
                .ToList();

            int currentX = int.MinValue;

            foreach (var item in ordered)
            {
                int x = item.PreferredX;

                if (x < currentX + minNodeSpacing)
                    x = currentX + minNodeSpacing;

                currentX = x;

                item.Node
                    .GetType()
                    .GetProperty("NodePosition")!
                    .SetValue(item.Node, new Position(x, y));
            }
        }
    }
}
