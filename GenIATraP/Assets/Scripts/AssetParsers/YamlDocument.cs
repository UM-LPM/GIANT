using System.Collections.Generic;

public class YamlFile
{
    public List<YamlDocument> Documents { get; set; }

    public YamlFile()
    {
        Documents = new List<YamlDocument>();
    }
}

public class YamlDocument
{
    public string DocumentId { get; set; } // = fileID
    public string DocumentType { get; set; }
    public Dictionary<string, object> DocumentProperties { get; set; }

    public YamlDocument()
    {
        DocumentProperties = new Dictionary<string, object>();
    }
}