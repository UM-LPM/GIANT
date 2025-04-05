namespace Problems.PlanetConquest
{
    public class PlanetComponent
    {
        public int CapturedTeamID { get; set; }
        public PlanetType Type { get; set; }
    }

    public enum PlanetType
    {
        Lava,
        Ice
    }
}