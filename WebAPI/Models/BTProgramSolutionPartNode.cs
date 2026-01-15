using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Enums;

namespace WebAPI.Models {
    public class BTProgramSolutionPartNode
    {
        public long FileID { get; set; } // random value between [111111111111111111,999999999999999999]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "Node name (type) is required.")]
        public string? Name { get; set; }
        public List<BTProgramSolutionPartNode>? Children { get; set; }
        public List<Property>? Properties { get; set; }

        public Position? NodePosition { get; set; }

        // Layout constants
        private const int NodeWidth = 150;
        private const int NodeHeight = 140;
        private const int HorizontalSpacing = 0;

        public static void UpdateNoteIDs(BTProgramSolutionPartNode rootNode) {
            Queue<BTProgramSolutionPartNode> nodeQueue = new Queue<BTProgramSolutionPartNode>();
            if (rootNode != null)
                nodeQueue.Enqueue(rootNode);

            while (nodeQueue.Count > 0) {
                BTProgramSolutionPartNode node = nodeQueue.Dequeue();

                node.FileID = GenerateRandomFileID();
                node.Guid = Guid.NewGuid();

                if (node.Children != null)
                    foreach (BTProgramSolutionPartNode child in node.Children) {
                        nodeQueue.Enqueue(child);
                    }
            }
        }

        public static long GenerateRandomFileID() {
            return new System.Random().NextInt64(100000000000000000, 999999999999999999);
        }


        public static void UpdateNodePositions(BTProgramSolutionPartNode root)
        {
            if (root == null)
                return;

            int nextLeafX = 0;
            LayoutNode(root, 0, ref nextLeafX);
        }

        private static void LayoutNode(BTProgramSolutionPartNode node, int depth, ref int nextLeafX)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                // Leaf node -> assign next available horizontal slot
                node.NodePosition = new Position(
                    nextLeafX * (NodeWidth + HorizontalSpacing),
                    depth * NodeHeight
                );

                nextLeafX++;
                return;
            }

            // Layout children first (post-order)
            foreach (var child in node.Children)
            {
                LayoutNode(child, depth + 1, ref nextLeafX);
            }

            // Center parent above its children
            var firstChild = node.Children[0];
            var lastChild = node.Children[^1];

            int centerX = (firstChild.NodePosition!.X + lastChild.NodePosition!.X) / 2;

            node.NodePosition = new Position(
                centerX,
                depth * NodeHeight
            );
        }


        public static string GetNodeTypeGuid(NodeType nodeType) {
            switch (nodeType) {
                case NodeType.Root:
                    return "163c147d123e4a945b688eddc64e3ea5";
                case NodeType.Repeat:
                    return "afb5496e8cd973748a10b3e3ef436ebd";
                case NodeType.Selector:
                    return "460be9e34c566ea45b9e282b1adcb028";
                case NodeType.Sequencer:
                    return "61431bba79d7d7843b82bf1de71703f5";
                case NodeType.Inverter:
                    return "e658b1bd308bc5c429f5a9b404a04943";
                case NodeType.Encapsulator:
                    return "88210b6ae4b65bc4f975f7a750c75612";
                case NodeType.MoveForward:
                    return "1fd1e85f30abba2499f6834e124b1450";
                case NodeType.MoveSide:
                    return "7e4181f6492e3fc45bf357f24d63fd4d";
                case NodeType.Rotate:
                    return "ef843663b73a3c544b520ab90e69c9f4";
                case NodeType.RayHitObject:
                    return "2896f3e48c4d62d40be88fb007bb6361";
                case NodeType.RotateTurret:
                    return "1191a1255814faa47b52cc65d43e6285";
                case NodeType.Shoot:
                    return "a3dc491c3b458a945b4bd32e24ed7627";
                case NodeType.PlaceBomb:
                    return "dfab410791810f74995e77be14f491cb";
                case NodeType.GridCellContainsObject:
                    return "5c69b16a0f5bc7e43adf37e9a2a453dc";
                case NodeType.HealthLevelBellow:
                    return "e7005ef9ca3dafc4b86928393e168f40";
                case NodeType.ShieldLevelBellow:
                    return "a380d7f9274338e4485e03ec126c5859";
                case NodeType.AmmoLevelBellow:
                    return "3541a6a84d600d443a32c4b10ae322a3";
                case NodeType.BallInHand:
                    return "b5314532df644ff4a9d9817783b7d1f3";
                case NodeType.BallInRange:
                    return "bba9a0211020ff544a7456dc3ec38148";
                case NodeType.PickUpBall:
                    return "d91480bd52c88d645bd43a55498470bf";
                case NodeType.ThrowBall:
                    return "0b6c54db52fce4942a44d87dfdf43a37";
            }
            return "";
        }

        public static NodeType NodeTypeStringToNodeType(string? nodeTypeString) {
            if (nodeTypeString == null)
                throw new ArgumentNullException("Node type string cannot be null");

            switch (nodeTypeString) {
                case "RootNode":
                    return NodeType.Root;
                case "Repeat":
                    return NodeType.Repeat;
                case "Selector":
                    return NodeType.Selector;
                case "Sequencer":
                    return NodeType.Sequencer;
                case "Inverter":
                    return NodeType.Inverter;
                case "Encapsulator":
                    return NodeType.Encapsulator;
                case "MoveForward":
                    return NodeType.MoveForward;
                case "MoveSide":
                    return NodeType.MoveSide;
                case "Rotate":
                    return NodeType.Rotate;
                case "RayHitObject":
                    return NodeType.RayHitObject;
                case "RotateTurret":
                    return NodeType.RotateTurret;
                case "Shoot":
                    return NodeType.Shoot;
                case "PlaceBomb":
                    return NodeType.PlaceBomb;
                case "GridCellContainsObject":
                    return NodeType.GridCellContainsObject;
                case "HealthLevelBellow":
                    return NodeType.HealthLevelBellow;
                case "ShieldLevelBellow":
                    return NodeType.ShieldLevelBellow;
                case "AmmoLevelBellow":
                    return NodeType.AmmoLevelBellow;
            }

            throw new Exception("Invalid NodeTypeString");
        }
        public static string NodeTypeToNodeTypeString(NodeType nodeType) {
            switch (nodeType) {
                case NodeType.Root:
                    return "RootNode";
                case NodeType.Repeat:
                    return "Repeat";
                case NodeType.Selector:
                    return "Selector";
                case NodeType.Sequencer:
                    return "Sequencer";
                case NodeType.Inverter:
                    return "Inverter";
                case NodeType.Encapsulator:
                    return "Encapsulator";
                case NodeType.MoveForward:
                    return "MoveForward";
                case NodeType.MoveSide:
                    return "MoveSide";
                case NodeType.Rotate:
                    return "Rotate";
                case NodeType.RayHitObject:
                    return "RayHitObject";
                case NodeType.HealthLevelBellow:
                    return "HealthLevelBellow";
                case NodeType.ShieldLevelBellow:
                    return "ShieldLevelBellow";
                case NodeType.AmmoLevelBellow:
                    return "AmmoLevelBellow";
                case NodeType.GridCellContainsObject:
                    return "GridCellContainsObject";
                case NodeType.PlaceBomb:
                    return "PlaceBomb";
            }
            throw new Exception("Invalid NodeType");
        }

    }
}
