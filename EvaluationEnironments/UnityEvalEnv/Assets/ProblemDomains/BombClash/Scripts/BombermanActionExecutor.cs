using AgentControllers;
using Base;
using UnityEngine;

namespace Problems.Bombclash
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
            upAxis = agent.ActionBuffer.GetDiscreteAction("moveUpDirection");
            sideAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");

            if (upAxis == 1)
            {
                agent.SetDirection(Vector2.up, agent.SpriteRendererUp);
            }
            else if (upAxis == 2)
            {
                agent.SetDirection(Vector2.down, agent.SpriteRendererDown);
            }
            else if (sideAxis == 1)
            {
                agent.SetDirection(Vector2.left, agent.SpriteRendererLeft);
            }
            else if (sideAxis == 2)
            {
                agent.SetDirection(Vector2.right, agent.SpriteRendererRight);
            }
            else
            {
                agent.SetDirection(Vector2.zero, agent.ActiveSpriteRenderer);
            }

            if (agent.NextAgentUpdateTime <= BombermanEnvironmentController.CurrentSimulationTime && agent.MoveDirection != Vector2.zero)
            {
                if (BombermanEnvironmentController.AgentCanMove(agent))
                {
                    agent.transform.Translate(new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0));
                    agent.NextAgentUpdateTime = BombermanEnvironmentController.CurrentSimulationTime + BombermanEnvironmentController.AgentUpdateinterval;
                    BombermanEnvironmentController.CheckIfAgentOverPowerUp(agent);
                    agent.CheckIfNewSectorExplored();
                }
                else
                {
                    agent.SetDirection(Vector2.zero, agent.SpriteRendererDown);
                }
            }
        }

        public void PlaceBomb(BombermanAgentComponent agent)
        {
            if (agent.BombsRemaining > 0 && agent.ActionBuffer.GetDiscreteAction("placeBomb") == 1)
            {
                StartCoroutine(BombermanEnvironmentController.BombExplosionController.PlaceBomb(agent));
            }
        }
    }
}
