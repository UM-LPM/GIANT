using System;
using TheKiwiCoder;
using UnityEngine;

[Serializable]
public class RobostrikeAgentConfig
{
    public GameObject AgentPrefab;
    public Transform AgentStartSpawnPosition;
    public BehaviourTree BehaviourTree;
    public Material AgentMaterial;
}