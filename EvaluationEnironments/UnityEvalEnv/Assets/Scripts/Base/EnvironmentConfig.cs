using UnityEngine;

namespace Base
{
    [System.Serializable]
    public class EnvironmentConfig
    {
        public string EnvironmentName;
        public GameObject EnvironmentPrefab;
        public bool IncludeEnvironment;
    }
}