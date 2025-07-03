using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.BoxTact
{
    public class BoxTactAgentComponent : AgentComponent
    {

        public Vector2 MoveDirection { get; private set; }
        public float NextAgentUpdateTime { get; set; }

        private Vector2 CurrentAgentPosition;
        List<Vector2> ExploredSectors;

        // Agent fitness variables
        public int SectorsExplored { get; set; }
        public int BoxesMoved { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            MoveDirection = Vector2.zero;
            ExploredSectors = new List<Vector2>();
            CurrentAgentPosition = new Vector2(transform.position.x, transform.position.y);
        }

        public void SetDirection(Vector2 newDirection)
        {
            MoveDirection = newDirection;
        }

        public void CheckIfNewSectorExplored()
        {
            CurrentAgentPosition.x = transform.position.x;
            CurrentAgentPosition.y = transform.position.y;
            // Check if there's already sector with agents x and y coordinates
            if (!ExploredSectors.Contains(CurrentAgentPosition))
            {
                ExploredSectors.Add(new Vector2(transform.position.x, transform.position.y));
                SectorsExplored++;
            }
        }
    }
}
