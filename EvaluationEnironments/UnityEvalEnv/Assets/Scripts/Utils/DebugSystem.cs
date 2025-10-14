using System;
using UnityEngine;

namespace Utils
{
    [Flags]
    public enum DebugCategory
    {
        None = 0,
        Info = 1 << 0,
        Success = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3,
        Detailed = 1 << 4,
        Verbose = 1 << 5,
        Temporary = 1 << 6,
        All = ~0
    }


    public static class DebugSystem
    {
        // Active categories (default: all enabled)
        private static DebugCategory ActiveCategories = DebugCategory.All;

        // Colors
        private const string INFO_COLOR = "#00BFFF";      // DeepSkyBlue
        private const string SUCCESS_COLOR = "#32CD32";   // LimeGreen
        private const string WARNING_COLOR = "#FFD700";   // Gold
        private const string ERROR_COLOR = "#FF4500";     // OrangeRed
        private const string DETAIL_COLOR = "#ADFF2F";    // GreenYellow
        private const string VERBOSE_COLOR = "#d99df2";   // Light Purple
        private const string TEMPORARY_COLOR = "#947853"; // Light Brown


        // Enable/Disable entire categories
        public static void EnableCategory(DebugCategory category, bool enabled)
        {
            if (enabled)
                ActiveCategories |= category;   // turn bit on
            else
                ActiveCategories &= ~category;  // turn bit off
        }

        public static void EnableAll(bool enabled)
        {
            ActiveCategories = enabled ? DebugCategory.All : DebugCategory.None;
        }

        public static bool IsCategoryEnabled(DebugCategory category)
        {
            return (ActiveCategories & category) != 0;
        }

        // --- Logging Methods ---

        public static void Log(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Info))
                Debug.Log(FormatMessage("INFO", message, INFO_COLOR));
        }

        public static void LogSuccess(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Success))
                Debug.Log(FormatMessage("SUCCESS", message, SUCCESS_COLOR));
        }

        public static void LogWarning(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Warning))
                Debug.LogWarning(FormatMessage("WARN", message, WARNING_COLOR));
        }

        public static void LogError(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Error))
                Debug.LogError(FormatMessage("ERROR", message, ERROR_COLOR));
        }

        public static void LogDetailed(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Detailed))
                Debug.Log(FormatMessage("DETAIL", message, DETAIL_COLOR));
        }

        public static void LogVerbose(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Verbose))
                Debug.Log(FormatMessage("VERBOSE", message, VERBOSE_COLOR));
        }

        public static void LogTemporary(string message)
        {
            if (IsCategoryEnabled(DebugCategory.Temporary))
                Debug.Log(FormatMessage("TEMP", message, TEMPORARY_COLOR));
        }

        // Helper for color formatting
        private static string FormatMessage(string tag, string message, string hexColor)
        {
            return $"<color={hexColor}><b>[{tag}]</b></color> {message}";
        }
    }

}