using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSActionAssignment
    {
        public Unit unit;
        public int actionType;
        public int direction;
        public int assignmentTime;
        public UnitType unitType;
        public int targetX;
        public int targetY;

        public MicroRTSActionAssignment(Unit unit, int actionType, int assignmentTime, int? direction = null, int? targetX = null, int? targetY = null, UnitType unitType = null)
        {
            this.unit = unit;
            this.actionType = actionType;
            this.direction = direction ?? -1;
            this.assignmentTime = assignmentTime;
            this.unitType = unitType;
            this.targetX = targetX ?? -1;
            this.targetY = targetY ?? -1;
        }

        public int GetETA()
        {
            if (unit == null || unit.Type == null) return 0;

            switch (actionType)
            {
                case ACTION_TYPE_MOVE:
                    return unit.MoveTime;
                case ACTION_TYPE_ATTACK:
                    return unit.AttackTime;
                case ACTION_TYPE_HARVEST:
                    return unit.HarvestTime;
                case ACTION_TYPE_RETURN:
                    return unit.MoveTime;
                case ACTION_TYPE_PRODUCE:
                    return unitType != null ? unitType.produceTime : 0;
                default:
                    return 0;
            }
        }

        public const int ACTION_TYPE_MOVE = 1;
        public const int ACTION_TYPE_ATTACK = 2;
        public const int ACTION_TYPE_HARVEST = 3;
        public const int ACTION_TYPE_RETURN = 4;
        public const int ACTION_TYPE_PRODUCE = 5;
    }
}

