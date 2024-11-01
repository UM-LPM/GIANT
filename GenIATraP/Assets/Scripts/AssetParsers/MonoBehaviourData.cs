using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonoBehaviourData
{
    public int m_ObjectHideFlags;
    public SourceObject m_CorrespondingSourceObject;
    public SourceObject m_PrefabInstance;
    public SourceObject m_PrefabAsset;
    public SourceObject m_GameObject;
    public int m_Enabled;
    public int m_EditorHideFlags;
    public Script m_Script;
    public string m_Name;
    public string m_EditorClassIdentifier;
    public int state;
    public int started;
    public string guid;
    public Position position;
    public int callFrequencyCount;
    public string description;
    public int drawGizmos;
    public List<SourceObject> children; // Used for nodes with children
    public SourceObject child; // Used for nodes with a single child
    public int moveForwardDirection; // Specific to MoveForward behaviour
    public int moveSideDirection; // Specific to MoveSide behaviour
    public int rotateDirection; // Specific to Rotate behaviour
    public int restartOnSuccess; // Specific to Repeat behaviour
    public int restartOnFailure; // Specific to Repeat behaviour
}

[System.Serializable]
public class SourceObject
{
    public long fileID;
}

[System.Serializable]
public class Script
{
    public long fileID;
    public string guid;
    public int type;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
}

[System.Serializable]
public class AgentControllerData
{
    public List<MonoBehaviourData> AgentControllers;
    public string m_Name;
}
