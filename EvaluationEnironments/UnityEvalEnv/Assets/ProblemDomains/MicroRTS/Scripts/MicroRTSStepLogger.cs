using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Problems.MicroRTS.Core;
using Utils;

namespace Problems.MicroRTS
{
    public class MicroRTSStepLogger : MonoBehaviour
    {
        [SerializeField] private bool enableStepLogging = true;
        [SerializeField] private string logDirectory = "Logs/MicroRTS";

        private StreamWriter logWriter;
        private string logFilePath;
        private bool isInitialized = false;
        private int lastLoggedCycle = -1;

        public bool IsEnabled => enableStepLogging && isInitialized;

        public void Initialize(string customLogDirectory = null)
        {
            if (!enableStepLogging)
            {
                return;
            }

            try
            {
                string directory = customLogDirectory ?? logDirectory;

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFilePath = Path.Combine(directory, $"microrts_steps_{timestamp}.log");

                logWriter = new StreamWriter(logFilePath, append: false, encoding: Encoding.UTF8)
                {
                    AutoFlush = false
                };

                isInitialized = true;
                LogCycleStart(0);
                DebugSystem.Log($"Step logging initialized: {logFilePath}");
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to initialize step logger: {ex.Message}");
                isInitialized = false;
            }
        }

        public void LogCycleStart(int cycle)
        {
            if (!IsEnabled || cycle == lastLoggedCycle) return;

            try
            {
                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "cycle_start" },
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") }
                };
                WriteJsonLine(entry);
                lastLoggedCycle = cycle;
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log cycle start: {ex.Message}");
            }
        }

        public void LogCycleEnd(int cycle)
        {
            if (!IsEnabled) return;

            try
            {
                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "cycle_end" },
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") }
                };
                WriteJsonLine(entry);
                Flush();
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log cycle end: {ex.Message}");
            }
        }

        public void LogActionScheduled(Unit unit, MicroRTSActionAssignment assignment, int cycle)
        {
            if (!IsEnabled || unit == null || assignment == null) return;

            try
            {
                string actionTypeName = GetActionTypeName(assignment.actionType);
                int fromX = unit.X;
                int fromY = unit.Y;
                int toX = fromX;
                int toY = fromY;

                if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_MOVE)
                {
                    var offset = MicroRTSUtils.GetDirectionOffset(assignment.direction);
                    toX = fromX + offset.x;
                    toY = fromY + offset.y;
                }
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_ATTACK)
                {
                    toX = assignment.targetX;
                    toY = assignment.targetY;
                }
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_HARVEST)
                {
                    var offset = MicroRTSUtils.GetDirectionOffset(assignment.direction);
                    toX = fromX + offset.x;
                    toY = fromY + offset.y;
                }

                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "action_scheduled" },
                    { "unit_id", unit.ID },
                    { "unit_type", unit.Type?.name ?? "Unknown" },
                    { "player", unit.Player },
                    { "action_type", actionTypeName },
                    { "from", new int[] { fromX, fromY } },
                    { "to", new int[] { toX, toY } },
                    { "assignment_time", assignment.assignmentTime }
                };

                if (assignment.direction >= 0)
                {
                    entry["direction"] = assignment.direction;
                }
                if (assignment.targetX >= 0)
                {
                    entry["target_x"] = assignment.targetX;
                }
                if (assignment.targetY >= 0)
                {
                    entry["target_y"] = assignment.targetY;
                }
                if (assignment.unitType != null)
                {
                    entry["unit_type_produced"] = assignment.unitType.name;
                }

                WriteJsonLine(entry);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log action scheduled: {ex.Message}");
            }
        }

        public void LogActionCompleted(Unit unit, MicroRTSActionAssignment assignment, int cycle)
        {
            if (!IsEnabled || unit == null || assignment == null) return;

            try
            {
                string actionTypeName = GetActionTypeName(assignment.actionType);
                int fromX = unit.X;
                int fromY = unit.Y;
                int toX = fromX;
                int toY = fromY;

                if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_MOVE)
                {
                    var offset = MicroRTSUtils.GetDirectionOffset(assignment.direction);
                    fromX = unit.X - offset.x;
                    fromY = unit.Y - offset.y;
                    toX = unit.X;
                    toY = unit.Y;
                }
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_ATTACK)
                {
                    toX = assignment.targetX;
                    toY = assignment.targetY;
                }

                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "action_completed" },
                    { "unit_id", unit.ID },
                    { "unit_type", unit.Type?.name ?? "Unknown" },
                    { "player", unit.Player },
                    { "action_type", actionTypeName },
                    { "from", new int[] { fromX, fromY } },
                    { "to", new int[] { toX, toY } },
                    { "completion_time", cycle }
                };

                if (assignment.direction >= 0)
                {
                    entry["direction"] = assignment.direction;
                }
                if (assignment.targetX >= 0)
                {
                    entry["target_x"] = assignment.targetX;
                }
                if (assignment.targetY >= 0)
                {
                    entry["target_y"] = assignment.targetY;
                }

                WriteJsonLine(entry);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log action completed: {ex.Message}");
            }
        }

        public void LogUnitState(Unit unit, int cycle)
        {
            if (!IsEnabled || unit == null) return;

            try
            {
                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "unit_state" },
                    { "unit_id", unit.ID },
                    { "unit_type", unit.Type?.name ?? "Unknown" },
                    { "player", unit.Player },
                    { "position", new int[] { unit.X, unit.Y } },
                    { "hp", unit.HitPoints },
                    { "max_hp", unit.MaxHitPoints },
                    { "resources", unit.Resources }
                };
                WriteJsonLine(entry);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log unit state: {ex.Message}");
            }
        }

        public void LogUnitCreated(Unit unit, int cycle)
        {
            if (!IsEnabled || unit == null) return;

            try
            {
                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "unit_created" },
                    { "unit_id", unit.ID },
                    { "unit_type", unit.Type?.name ?? "Unknown" },
                    { "player", unit.Player },
                    { "position", new int[] { unit.X, unit.Y } },
                    { "hp", unit.HitPoints },
                    { "max_hp", unit.MaxHitPoints },
                    { "resources", unit.Resources }
                };
                WriteJsonLine(entry);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log unit created: {ex.Message}");
            }
        }

        public void LogUnitDestroyed(Unit unit, int cycle)
        {
            if (!IsEnabled || unit == null) return;

            try
            {
                var entry = new Dictionary<string, object>
                {
                    { "cycle", cycle },
                    { "event_type", "unit_destroyed" },
                    { "unit_id", unit.ID },
                    { "unit_type", unit.Type?.name ?? "Unknown" },
                    { "player", unit.Player },
                    { "position", new int[] { unit.X, unit.Y } }
                };
                WriteJsonLine(entry);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to log unit destroyed: {ex.Message}");
            }
        }

        public void LogCycle(int cycle, List<Unit> allUnits, Dictionary<Unit, MicroRTSActionAssignment> pendingActions)
        {
            if (!IsEnabled) return;

            LogCycleStart(cycle);

            foreach (var unit in allUnits)
            {
                if (unit != null && unit.HitPoints > 0)
                {
                    LogUnitState(unit, cycle);
                }
            }

            foreach (var kvp in pendingActions)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    LogActionScheduled(kvp.Key, kvp.Value, cycle);
                }
            }
        }

        public void Flush()
        {
            if (logWriter != null)
            {
                try
                {
                    logWriter.Flush();
                }
                catch (Exception ex)
                {
                    DebugSystem.LogError($"Failed to flush log: {ex.Message}");
                }
            }
        }

        public void Close()
        {
            if (logWriter != null)
            {
                try
                {
                    Flush();
                    logWriter.Close();
                    logWriter = null;
                    isInitialized = false;
                    DebugSystem.Log($"Step log closed: {logFilePath}");
                }
                catch (Exception ex)
                {
                    DebugSystem.LogError($"Failed to close log: {ex.Message}");
                }
            }
        }

        private void WriteJsonLine(object entry)
        {
            if (logWriter == null) return;

            try
            {
                string json = BuildJsonString(entry);
                logWriter.WriteLine(json);
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"Failed to write log entry: {ex.Message}");
            }
        }

        private string BuildJsonString(object obj)
        {
            if (obj == null) return "null";

            var dict = obj as Dictionary<string, object>;
            if (dict != null)
            {
                var sb = new StringBuilder();
                sb.Append("{");
                bool first = true;
                foreach (var kvp in dict)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"\"{EscapeJson(kvp.Key)}\":{ValueToJson(kvp.Value)}");
                    first = false;
                }
                sb.Append("}");
                return sb.ToString();
            }

            return JsonUtility.ToJson(obj);
        }

        private string ValueToJson(object value)
        {
            if (value == null) return "null";
            if (value is string) return $"\"{EscapeJson((string)value)}\"";
            if (value is bool) return ((bool)value) ? "true" : "false";
            if (value is int || value is long || value is float || value is double) return value.ToString();
            if (value is int[])
            {
                var arr = (int[])value;
                return "[" + string.Join(",", arr) + "]";
            }
            if (value is Dictionary<string, object>)
            {
                return BuildJsonString(value);
            }
            return $"\"{EscapeJson(value.ToString())}\"";
        }

        private string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private string GetActionTypeName(int actionType)
        {
            switch (actionType)
            {
                case MicroRTSActionAssignment.ACTION_TYPE_MOVE:
                    return "MOVE";
                case MicroRTSActionAssignment.ACTION_TYPE_ATTACK:
                    return "ATTACK";
                case MicroRTSActionAssignment.ACTION_TYPE_HARVEST:
                    return "HARVEST";
                case MicroRTSActionAssignment.ACTION_TYPE_RETURN:
                    return "RETURN";
                case MicroRTSActionAssignment.ACTION_TYPE_PRODUCE:
                    return "PRODUCE";
                default:
                    return $"Whoopsie, forgot this one ~-~-~-> {actionType} <-~-~-~";
            }
        }

        void OnDestroy()
        {
            Close();
        }

        void OnApplicationQuit()
        {
            Close();
        }
    }
}

