using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using WebAPI.Models;
using WebAPI.Models.EARS;

namespace AgentOrganizations
{
    [Serializable]
    public class Individual : ScriptableObject
    {
        public int IndividualId;
        public AgentController[] AgentControllers;

        public Individual(int individualId, ProgramSolution program) : base("Individual_" + individualId.ToString())
        {
            IndividualId = individualId;
            AgentControllers = new AgentController[program.SolutionParts.Count];

            MapProgramSolutionToIndividual(program);
        }

        private void MapProgramSolutionToIndividual(ProgramSolution program)
        {
            for (int i = 0; i < program.SolutionParts.Count; i++)
            {
                AgentControllers[i] = program.SolutionParts[i].MapToAgentController();
            }
        }
    }
}