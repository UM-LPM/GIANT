using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Enums;

namespace WebAPI.Models {
    public class TreeModelNode
    {
        public long FileID { get; set; } // random value between [111111111111111111,999999999999999999]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "Node name (type) is required.")]
        public string? Name { get; set; }
        public List<TreeModelNode>? Children { get; set; }
        public List<Property>? Properties { get; set; }

        public Position? NodePosition { get; set; }


        public static void UpdateNoteIDs(TreeModelNode rootNode) {
            Queue<TreeModelNode> nodeQueue = new Queue<TreeModelNode>();
            if (rootNode != null)
                nodeQueue.Enqueue(rootNode);

            while (nodeQueue.Count > 0) {
                TreeModelNode node = nodeQueue.Dequeue();

                node.FileID = GenerateRandomFileID();
                node.Guid = Guid.NewGuid();

                if (node.Children != null)
                    foreach (TreeModelNode child in node.Children) {
                        nodeQueue.Enqueue(child);
                    }
            }
        }

        public static long GenerateRandomFileID() {
            return new System.Random().NextInt64(100000000000000000, 999999999999999999);
        }

        public static void UpdateNodePositions(TreeModelNode rootNode) {
            // Update Nodes' position so the nodes will be graphically represented as a organized tree, where root node is at the top and leaves are at the bottom

            int nodeHeight = 140;
            int nodeWidth = 170;

            var result = new Dictionary<int, int>();
            var queue = new Queue<(TreeModelNode node, int depth)>();

            if (rootNode != null) {
                queue.Enqueue((rootNode, 0));
            }

            while (queue.Count > 0) {
                var (node, depth) = queue.Dequeue();

                if (!result.ContainsKey(depth)) {
                    result[depth] = 0;
                }

                result[depth] = result[depth] + 1;

                node.NodePosition = new Position(result[depth] * nodeWidth, depth * nodeHeight);

                if (node.Children != null) {
                    foreach (var child in node.Children) {
                        queue.Enqueue((child, depth + 1));
                    }
                }
            }
        }

        public static int UpdateNodePositions(TreeModelNode node, int depth, ref int index, int verticalSpacing, int horizontalSpacing) {
            if (node == null) {
                return 0;
            }

            // Calculate and assign the node's position
            int y = depth * verticalSpacing;
            node.NodePosition = new Position(index * horizontalSpacing, y);

            // Recursively update positions for all children and find the total number of descendants
            int descendants = 0;
            if (node.Children != null) {
                foreach (var child in node.Children) {
                    descendants += UpdateNodePositions(child, depth + 1, ref index, verticalSpacing, horizontalSpacing);
                }
            }

            // The position of the next sibling node or cousin node should be to the right of all descendants of this node
            index += Math.Max(1, descendants);

            return descendants + 1; // Include this node and all its descendants
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
            }
            throw new Exception("Invalid NodeType");
        }

    }
}
