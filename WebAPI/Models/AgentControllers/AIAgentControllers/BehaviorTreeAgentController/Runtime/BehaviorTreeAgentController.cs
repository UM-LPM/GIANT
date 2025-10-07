
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController.BombClash;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Robostrike;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    [Serializable]
    public class BehaviorTreeAgentController : AIAgentController
    {
        public BTNode RootNode;
        public BTNode.State TreeState = BTNode.State.Running;
        public List<BTNode> Nodes = new List<BTNode>();
        public Blackboard Blackboard = new Blackboard(); // Blackboard for all Nodes

        public BehaviorTreeAgentController(string name)
            : base(name)
        {
            this.Blackboard = new Blackboard();
        }

        public override void MapTreeModelToAgentController(TreeModel treeModel)
        {
            if (treeModel == null || treeModel.RootNode == null)
            {
                return;
            }

            MapNodes(treeModel.RootNode);
        }

        private void MapNodes(TreeModelNode treeModelNode)
        {
            if (treeModelNode == null)
            {
                return;
            }

            // Create a queue of nodes to process
            Queue<NodeModelToProcess> nodeModelsToProcessQueue = new Queue<NodeModelToProcess>();
            nodeModelsToProcessQueue.Enqueue(new NodeModelToProcess() { Node = null, TreeModelNode = treeModelNode});
            
            while(nodeModelsToProcessQueue.Count > 0)
            {
                NodeModelToProcess nodesModelToProcess = nodeModelsToProcessQueue.Dequeue();

                BTNode child = CreateNode(nodesModelToProcess.Node, nodesModelToProcess.TreeModelNode);

                if(this.RootNode == null)
                {
                    this.RootNode = child;
                }

                if (nodesModelToProcess.TreeModelNode.Children != null)
                {
                    foreach (TreeModelNode childTreeModelNode in nodesModelToProcess.TreeModelNode.Children)
                    {
                        nodeModelsToProcessQueue.Enqueue(new NodeModelToProcess() { Node = child, TreeModelNode = childTreeModelNode });
                    }
                }
            }
        }

        private BTNode CreateNode(BTNode parent, TreeModelNode treeModelNode)
        {
            BTNode node;

            switch (treeModelNode.Name)
            {
                case "RootNode":
                    node = new RootNode(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Decorators
                case "Repeat":
                    node = new Repeat(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Inverter":
                    node = new Inverter(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Succeeder":
                    node = new Succeed(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Failer":
                    node = new Failure(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Repeater":
                    node = new Repeat(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Encapsulator":
                    node = new Encapsulator(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Composites
                case "Sequencer":
                    node = new Sequencer(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Selector":
                    node = new Selector(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "RandomSelector":
                    node = new RandomSelector(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "InterruptSelector":
                    node = new InterruptSelector(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Parallel":
                    node = new Parallel(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Conditions
                case "AmmoLevelBellow":
                    node = new AmmoLevelBellow(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "ShieldLevelBellow":
                    node = new ShieldLevelBellow(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "HealthLevelBellow":
                    node = new HealthLevelBellow(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "RayHitObject":
                    node = new RayHitObject(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "GridCellContainsObject":
                    node = new GridCellContainsObject(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Actions
                case "MoveForward":
                    node = new MoveForward(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "MoveSide":
                    node = new MoveSide(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "Rotate":
                    node = new Rotate(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Actions - Robostrike
                case "Shoot":
                    node = new Shoot(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "RotateTurret":
                    node = new RotateTurret(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Actions - Bomberman
                case "PlaceBomb":
                    node = new PlaceBomb(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                // Actions Dodgeball
                case "PickUpBall":
                    node = new PickUpBall(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "ThrowBall":
                    node = new ThrowBall(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "BallInHand":
                    node = new BallInHand(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "BallInRange":
                    node = new BallInRange(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                case "GoalAhead":
                    node = new GoalAhead(treeModelNode.Guid, treeModelNode.Name, treeModelNode.Properties, treeModelNode.NodePosition);
                    break;
                default:
                    throw new Exception("Node type not recognized");
            }

            Nodes.Add(node);

            if(parent != null)
            {
                parent.AddChild(node);
            }

            return node;
        }
    }

    class NodeModelToProcess
    {
        public BTNode Node { get; set; }
        public TreeModelNode TreeModelNode { get; set; }
    }
}