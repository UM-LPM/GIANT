using AgentControllers;
using Base;
using UnityEngine;

namespace Problems.BombClash
{
    public class BombermanActionExecutor : ActionExecutor
    {
        private BombermanEnvironmentController BombermanEnvironmentController;

        // Move Agent variables
        int upAxis = 0;
        int sideAxis = 0;

        private void Awake()
        {
            BombermanEnvironmentController = GetComponentInParent<BombermanEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as BombermanAgentComponent);
            PlaceBomb(agent as BombermanAgentComponent);
        }

        private void MoveAgent(BombermanAgentComponent agent)
        {
            upAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            sideAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");

            if (upAxis == 1)
            {
                agent.SetDirection(Vector2Int.up, agent.SpriteRendererUp);
            }
            else if (upAxis == 2)
            {
                agent.SetDirection(Vector2Int.down, agent.SpriteRendererDown);
            }
            else if (sideAxis == 1)
            {
                agent.SetDirection(Vector2Int.left, agent.SpriteRendererLeft);
            }
            else if (sideAxis == 2)
            {
                agent.SetDirection(Vector2Int.right, agent.SpriteRendererRight);
            }
            else
            {
                agent.SetDirection(Vector2Int.zero, agent.ActiveSpriteRenderer);
            }

            if (agent.NextAgentUpdateTime <= BombermanEnvironmentController.CurrentSimulationSteps && agent.MoveDirection != Vector2Int.zero)
            {
                if (BombermanEnvironmentController.AgentCanMove(agent))
                {
                    agent.transform.position += new Vector3Int(agent.MoveDirection.x, agent.MoveDirection.y, 0);

                    agent.NextAgentUpdateTime = BombermanEnvironmentController.CurrentSimulationSteps + BombermanEnvironmentController.AgentUpdateinterval;
                    BombermanEnvironmentController.CheckIfAgentOverPowerUp(agent);
                    agent.CheckIfNewSectorExplored();
                }
                else
                {
                    agent.SetDirection(Vector2Int.zero, agent.SpriteRendererDown);
                }
            }
        }

        public void PlaceBomb(BombermanAgentComponent agent)
        {
            if (agent.BombsRemaining > 0 && agent.ActionBuffer.GetDiscreteAction("placeBomb") == 1)
            {
                BombermanEnvironmentController.PlaceBomb(agent);
            }
        }
    }
}
