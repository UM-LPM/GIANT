using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Configurator : MonoBehaviour
{
    public MainConfiguration MainConfiguration { get; set; }
    private void Awake()
    {
        // Read the configuration file from the same location of this file
        string path = Application.dataPath + "/conf.json";
        MainConfiguration = ReadConfigurationFromFile(path);

        if(MainConfiguration != null)
        {
            // Automatically load comunicator scene

        }
    }

    public MainConfiguration ReadConfigurationFromFile(string path)
    {
        return MainConfiguration.Deserialize(path);
    }

    public void SaveConfigurationToFile(string path)
    {
        MainConfiguration.Serialize(MainConfiguration, path);
    }
}