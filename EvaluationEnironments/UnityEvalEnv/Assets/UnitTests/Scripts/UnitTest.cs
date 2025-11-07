using AgentOrganizations;
using System;
using UnityEngine;

namespace UnitTests
{
    [Serializable]
    [CreateAssetMenu(fileName = "UnitTest", menuName = "UnitTests/UnitTest")]
    public class UnitTest : ScriptableObject
    {
        public string Name;
        public string ConfigFilePath;
        public Individual[] Individuals;
        public string ExpectedOutputFilePath;
    }
}