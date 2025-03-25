using UnityEngine;

namespace Utils
{
    public static class UnityUtils
    {
        public static Vector3 RoundToDecimals(this Vector3 vector, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10.0f, decimalPlaces);
            vector.x = Mathf.Round(vector.x * multiplier) / multiplier;
            vector.y = Mathf.Round(vector.y * multiplier) / multiplier;
            vector.z = Mathf.Round(vector.z * multiplier) / multiplier;
            return vector;
        }

        public static float RoundToDecimals(this float value, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10.0f, decimalPlaces);
            return Mathf.Round(value * multiplier) / multiplier;
        }
    }
}