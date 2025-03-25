using System;

namespace Base
{
    [Serializable]
    public class AgentScenario
    {
        public string AgentSceneName;
        public GameScenario[] GameScenarios;

        public bool ContainsGameScenario(string gameSceneName)
        {
            foreach (GameScenario gs in GameScenarios)
            {
                if (gs.GameSceneName == gameSceneName)
                    return true;
            }

            return false;
        }
    }
}