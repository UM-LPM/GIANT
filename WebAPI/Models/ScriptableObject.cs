public abstract class ScriptableObject
{
    public string name;
    public int hideFlags;

    public ScriptableObject(string name, int hideFlags = 0)
    {
        this.name = name;
        this.hideFlags = hideFlags;
    }
}