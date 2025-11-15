using UnityEngine;
using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSUnitComponent : MonoBehaviour
    {
        [Header("MicroRTS Unit Data")]
        [SerializeField] private Unit unit;

        public Unit Unit
        {
            get => unit;
            set => unit = value;
        }

        public long UnitID => unit != null ? unit.ID : -1;

        public int PlayerID => unit != null ? unit.Player : -1;

        public string UnitTypeName => unit != null && unit.Type != null ? unit.Type.name : "";

        public int HitPoints => unit != null ? unit.HitPoints : 0;

        public int MaxHitPoints => unit != null ? unit.MaxHitPoints : 0;

        public int Resources => unit != null ? unit.Resources : 0;

        public int GridX => unit != null ? unit.X : -1;

        public int GridY => unit != null ? unit.Y : -1;

        public void Initialize(Unit microRTSUnit)
        {
            if (microRTSUnit == null)
            {
                throw new System.ArgumentNullException(nameof(microRTSUnit), "Cannot initialize with null Unit");
            }
            unit = microRTSUnit;
        }

        public void SyncPositionFromGrid(Vector3 worldPosition)
        {
            if (transform != null)
            {
                transform.position = worldPosition;
            }
        }

        public bool IsAlive()
        {
            return unit != null && unit.HitPoints > 0;
        }
    }
}