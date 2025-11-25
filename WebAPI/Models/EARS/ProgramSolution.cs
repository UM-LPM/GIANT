using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using JsonSubTypes;
using Newtonsoft.Json;

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
}
