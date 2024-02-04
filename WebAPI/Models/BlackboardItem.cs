namespace WebAPI.Models {
    public class BlackboardItem {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public BlackboardItem(string name, string value) {
            Name = name;
            Value = value;
        }

    }
}
