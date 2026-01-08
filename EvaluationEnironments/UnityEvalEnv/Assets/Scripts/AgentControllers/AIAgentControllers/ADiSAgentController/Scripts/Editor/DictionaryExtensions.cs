using System.Collections.Generic;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    static class DictionaryExtensions
    {
        public static List<TValue> GetOrCreate<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> dict,
            TKey key)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                dict[key] = list;
            }

            return list;
        }
    }
}
