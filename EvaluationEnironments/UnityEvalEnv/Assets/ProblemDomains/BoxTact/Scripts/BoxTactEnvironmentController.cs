using Base;
using Configuration;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Problems.BoxTact
{
    public class BoxTactEnvironmentController : EnvironmentControllerBase
    {
        [Header("BoxTact Movement Configuration")]
        [SerializeField] public float AgentUpdateinterval = 0.1f;

        [Header("BoxTact Box configuration")]
        [SerializeField] public GameObject BoxPrefab;
        [SerializeField] public GameObject BoxTargetPrefab;


        private BoxTactBoxSpawner BoxSpawner;
        private BoxTactBoxTargetSpawner BoxTargetSpawner;

        private BoxTactBoxComponent[] Boxes;
        private BoxTactBoxTargetComponent[] BoxTargets;

        [HideInInspector] public Tilemap IndestructibleWalkableTiles { get; set; }


        // Temp variables 
        Collider2D[] hitColliders;
        Vector3 newBoxPos;
        Vector3 newAgentPos;

        int allBoxes;
        int boxesOnTargets;
        float maxBoxMoves;
        int boxesMovedToTarget;
        int sectorCount;

        float sectorExplorationFitness;
        float boxesMovedFitness;
        float boxesMovedToTargetFitness;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            BoxSpawner = GetComponent<BoxTactBoxSpawner>();
            if (BoxSpawner == null)
            {
                throw new Exception("BoxSpawner is not defined");
                // TODO Add error reporting here
            }

            BoxTargetSpawner = GetComponent<BoxTactBoxTargetSpawner>();
            if (BoxTargetSpawner == null)
            {
                throw new Exception("BoxTargetSpawner is not defined");
                // TODO Add error reporting here
            }

            if (SceneLoadMode == SceneLoadMode.LayerMode)
            {
                // Only one problem environment exists
                IndestructibleWalkableTiles = FindObjectOfType<SectorComponent>().GetComponent<Tilemap>();
            }
            else
            {
                // Each EnvironmentController contains its own problem environment
                IndestructibleWalkableTiles = GetComponentInChildren<SectorComponent>().GetComponent<Tilemap>();
            }

        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn boxes and targets
            Boxes = BoxSpawner.Spawn<BoxTactBoxComponent>(this);
            if (Boxes == null || Boxes.Length == 0)
            {
                throw new Exception("No boxes spawned");
                // TODO Add error reporting here
            }

            BoxTargets = BoxTargetSpawner.Spawn<BoxTactBoxTargetComponent>(this);
            if (BoxTargets == null || BoxTargets.Length == 0)
            {
                throw new Exception("No targets spawned");
                // TODO Add error reporting here
            }

            if (Boxes.Length != BoxTargets.Length)
            {
                throw new Exception("Number of boxes and targets do not match");
                // TODO Add error reporting here
            }

            CheckBoxTargetOverlap();
        }

        protected override void OnPostFixedUpdate()
        {
            CheckEndingState();
        }

        public override void CheckEndingState()
        {
            // Check if all boxes are on their targets
            if (Boxes.All(box => BoxTargets.Any(target => target.transform.position == box.transform.position)))
            {
                FinishGame();
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public bool AgentCanMove(BoxTactAgentComponent agent)
        {
            newAgentPos = agent.transform.position + new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0);
            hitColliders = Physics2D.OverlapBoxAll(newAgentPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);

            foreach (Collider2D collider in hitColliders)
            {
                BoxTactBoxComponent boxComponent = collider.GetComponent<BoxTactBoxComponent>();
                if (boxComponent != null)
                {
                    return MoveBox(boxComponent, agent, boxComponent.transform.position, agent.MoveDirection);
                }
                
                if (!collider.isTrigger)
                    return false;
            }

            return true;
        }

        bool MoveBox(BoxTactBoxComponent box, BoxTactAgentComponent agent, Vector3 pos, Vector2 moveDirection)
        {
            newBoxPos = pos + new Vector3(moveDirection.x, moveDirection.y, 0);
            hitColliders = Physics2D.OverlapBoxAll(newBoxPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
            if (hitColliders.Length > 0)
            {
                return false;
            }

            box.transform.Translate(moveDirection);
            box.LastAgentThatMoved = agent;
            agent.BoxesMoved++;

            // Change the box color if is on a target
            if (BoxTargets.Any(target => target.transform.position == box.transform.position))
            {
                box.BoxSpriteRenderer.color = new Color(.7f, .7f, .7f, 1f);
            }
            else
            {
                box.BoxSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }

            return true;
        }

        private void SetAgentsFitness()
        {
            allBoxes = Boxes.Length;
            boxesOnTargets = Boxes.Count(box => BoxTargets.Any(target => target.transform.position == box.transform.position));

            sectorCount = CountTiles(IndestructibleWalkableTiles);

            foreach (BoxTactAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)sectorCount;
                sectorExplorationFitness = (float)Math.Round(BoxTactFitness.FitnessValues[nameof(BoxTactFitness.FitnessKeys.SectorExploration)] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, nameof(BoxTactFitness.FitnessKeys.SectorExploration));

                // Boxes moved
                // Calculate the maximum number of times that agent can move any box
                if (AgentUpdateinterval > 0)
                {
                    maxBoxMoves = CurrentSimulationTime / AgentUpdateinterval;
                    boxesMovedFitness = agent.BoxesMoved / maxBoxMoves;
                    boxesMovedFitness = (float)Math.Round(BoxTactFitness.FitnessValues[nameof(BoxTactFitness.FitnessKeys.BoxesMoved)] * boxesMovedFitness, 4);
                    agent.AgentFitness.UpdateFitness(boxesMovedFitness, nameof(BoxTactFitness.FitnessKeys.BoxesMoved));
                }

                // Boxed placed
                boxesMovedToTarget = Boxes.Count(box => box.LastAgentThatMoved == agent && BoxTargets.Any(target => target.transform.position == box.transform.position));
                boxesMovedToTargetFitness = boxesMovedToTarget / (float)allBoxes;
                boxesMovedToTargetFitness = (float)Math.Round(BoxTactFitness.FitnessValues[nameof(BoxTactFitness.FitnessKeys.BoxesMovedToTarget)] * boxesMovedToTargetFitness, 4);
                agent.AgentFitness.UpdateFitness(boxesMovedToTargetFitness, nameof(BoxTactFitness.FitnessKeys.BoxesMovedToTarget));

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + sectorCount + "= " + sectorExplorationFitness);
                Debug.Log("Boxes moved: " + agent.BoxesMoved + " / " + maxBoxMoves + "= " + boxesMovedFitness);
                Debug.Log("Boxes moved to target: " + boxesMovedToTarget + " / " + allBoxes + "= " + boxesMovedToTargetFitness);
                Debug.Log("========================================");
            }
        }

        public void CheckBoxTargetOverlap()
        {
            // Check if any box target overlaps with a box
            foreach (BoxTactBoxTargetComponent target in BoxTargets)
            {
                foreach (BoxTactBoxComponent box in Boxes)
                {
                    if (target.transform.position == box.transform.position)
                    {
                        box.BoxSpriteRenderer.color = new Color(.7f, .7f, .7f, 1f);
                    }
                    else
                    {
                        box.BoxSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                    }
                }
            }
        }

        int CountTiles(Tilemap tilemap)
        {
            if (tilemap == null) return 0;

            int count = 0;
            BoundsInt bounds = tilemap.cellBounds;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos)) // Check if there's a tile at this position
                {
                    count++;
                }
            }
            return count;
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                BoxTactFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("AgentUpdateinterval"))
                {
                    AgentUpdateinterval = float.Parse(conf.ProblemConfiguration["AgentUpdateinterval"]);
                }
            }
        }
    }
}
