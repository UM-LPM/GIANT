using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using System.Collections.Generic;

public class BehaviourTreeNodeDef
{
    public string m_fileID { get; set; }
    public MScript m_Script { get; set; }
    public string m_Name { get; set; }
    public string guid { get; set; }
    public Position position { get; set; }
    public Blackboard blackboard { get; set; }
    public int drawGizmos { get; set; }
    public Child child { get; set; }
    public List<Child> children { get; set; }
    public List<Child> nodes { get; set; }

    // TODO add dictionary for node specific properties
    public Dictionary<string, string> bt_properties { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> node_properties { get; set; } = new Dictionary<string, string>();

    public BehaviourTreeNodeDef(string m_id)
    {
        this.m_fileID = m_id;
    }
}

/*public class Position
{
    public float x { get; set; }
    public float y { get; set; }
}*/

public class Child
{
    public string fileID { get; set; }
}

public class MScript
{
    public string fileID { get; set; }
    public string guid { get; set; }
    public int type { get; set; }
}