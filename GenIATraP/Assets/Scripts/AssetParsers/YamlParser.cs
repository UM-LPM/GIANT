using Newtonsoft.Json;
using System.Collections.Generic;
using static Unity.VisualScripting.Metadata;

public static class YamlParser
{
    public static YamlFile[] ReadYamlFilesFromFolder(string[] files)
    {
        YamlFile[] yamlFiles = new YamlFile[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            yamlFiles[i] = ReadYamlDocumentsFromFile(files[i]);
        }

        return yamlFiles;
    }

    public static YamlFile ReadYamlDocumentsFromFile(string filePath)
    {
        string fileContent = System.IO.File.ReadAllText(filePath);
        string[] fileLines = fileContent.Split('\n');

        YamlFile yamlFile = new YamlFile();

        YamlDocument yamlDocument = null;
        Dictionary<string, object> properties;
        for (int i = 0; i < fileLines.Length; i++)
        {
            if (fileLines[i].Length == 0)
            {
                continue;
            }

            if (fileLines[i].Contains("---"))
            {
                if (yamlDocument != null)
                {
                    yamlFile.Documents.Add(yamlDocument);
                }

                yamlDocument = new YamlDocument();
                string[] nodeHeaderArray = fileLines[i].Split(' ');
                nodeHeaderArray[2] = nodeHeaderArray[2].Replace('&', ' ').Trim();
                yamlDocument.DocumentId = nodeHeaderArray[2];

                yamlDocument.DocumentType = fileLines[++i].Split(":")[0].Trim();
            }
            else if (yamlDocument != null && fileLines[i].Contains("children"))
            {
                ParseChildren(yamlDocument, "children", fileLines, ref i);
            }
            else if (yamlDocument != null && fileLines[i].Contains("AgentControllers"))
            {
                ParseChildren(yamlDocument, "AgentControllers", fileLines, ref i);
            }
            else if (yamlDocument != null && fileLines[i].Contains("Nodes"))
            {
                ParseChildren(yamlDocument, "Nodes", fileLines, ref i);
            }
            else if (yamlDocument != null)
            {
                string[] line = fileLines[i].Split(':', 2);
                if (line[1].Trim().Contains("{"))
                {
                    line[1] = convertSringArrayToJson(line[1].Trim());
                    properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(line[1].Trim());
                    yamlDocument.AddProperty(line[0].Trim(), properties);
                }
                else
                {
                    yamlDocument.AddProperty(line[0].Trim(), line[1].Trim());
                }
            }
        }

        if (yamlDocument != null)
        {
            yamlFile.Documents.Add(yamlDocument);
        }

        return yamlFile;
    }

    public static void ParseChildren(YamlDocument yamlDocument, string parentName, string[] fileLines, ref int i)
    {
        List<Dictionary<string, object>> children = new List<Dictionary<string, object>>();
        Dictionary<string, object> child;
        while (fileLines[++i].Contains("- {fileID"))
        {
            fileLines[i] = fileLines[i].Trim().Replace("- {", "{");
            fileLines[i] = convertSringArrayToJson(fileLines[i].Trim());
            child = JsonConvert.DeserializeObject<Dictionary<string, object>>(fileLines[i]);
            children.Add(child);
        }
        yamlDocument.AddProperty(parentName, children);
        i--;
    }

    public static string convertSringArrayToJson(string input)
    {
        input = input.Replace(" ", "");
        input = input.Replace("{", "{\"");
        input = input.Replace("}", "\"}");
        input = input.Replace(",", "\",\"");
        input = input.Replace(":", "\":\"");
        return input;
    }
}

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

    public void AddProperty(string key, object value)
    {
        if(DocumentProperties.ContainsKey(key))
        {
            throw new System.Exception($"Key {key} already exists in the document properties.");
            // TODO Add error reporting here
        }
        DocumentProperties.Add(key, value);
    }
}