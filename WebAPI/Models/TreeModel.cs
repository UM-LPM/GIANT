namespace WebAPI.Models {
    public class TreeModel {

        public long FileID { get; set; }
        public string Name { get; set; }
        public Node? RootNode { get; set; }

        public List<BlackboardItem> BlackboardItems { get; set; }

        public TreeModel() {
            Name = string.Empty;
            BlackboardItems = new List<BlackboardItem> { };
        }
    }

}
