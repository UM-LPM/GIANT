namespace WebAPI.Models {
    public class Property {
        public string? Name { get; set; }
        public int Value { get; set; }

        public Property(string name, int value) {
            Name = name;
            Value = value;
        }
    }
}
