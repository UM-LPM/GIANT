using AgentControllers;
using Base;
using UnityEngine;

namespace Problems.BombClash
{
    public class BombClashActionExecutor : ActionExecutor
    {
        private BombClashEnvironmentController BombClashEnvironmentController;

        // Move Agent variables
        int upAxis = 0;
        int sideAxis = 0;

        private void Awake()
        {
            BombClashEnvironmentController = GetComponentInParent<BombClashEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as BombClashAgentComponent);
            PlaceBomb(agent as BombClashAgentComponent);
        }

        private void MoveAgent(BombClashAgentComponent agent)
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

            if (agent.NextAgentUpdateTime <= BombClashEnvironmentController.CurrentSimulationSteps && agent.MoveDirection != Vector2Int.zero)
            {
                if (BombClashEnvironmentController.AgentCanMove(agent))
                {
                    agent.Move(agent.MoveDirection.x, agent.MoveDirection.y);

                    float speedIncrease = (1 - ((agent.MoveSpeed - BombClashEnvironmentController.AgentStartMoveSpeed) / 10.0f));
                    if (speedIncrease < BombClashEnvironmentController.MaxSpeedIncrease)
                        speedIncrease = BombClashEnvironmentController.MaxSpeedIncrease;
                    int nextUpdateIntervalDiff = (int)(speedIncrease * BombClashEnvironmentController.AgentUpdateinterval);

                    agent.NextAgentUpdateTime = BombClashEnvironmentController.CurrentSimulationSteps + nextUpdateIntervalDiff;
                    BombClashEnvironmentController.CheckIfAgentOverPowerUp(agent);
                    agent.CheckIfNewSectorExplored();
                }
                else
                {
                    agent.SetDirection(Vector2Int.zero, agent.SpriteRendererDown);
                }
            }
        }

        public void PlaceBomb(BombClashAgentComponent agent)
        {
            if (agent.BombsRemaining > 0 && agent.ActionBuffer.GetDiscreteAction("placeBomb") == 1)
            {
                BombClashEnvironmentController.PlaceBomb(agent);
            }
        }
    }
}
