using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSActionAssignment
    {
        public Unit unit;
        public int actionType;
        public int direction;
        public int assignmentTime;

        public MicroRTSActionAssignment(Unit unit, int actionType, int direction, int assignmentTime)
        {
            this.unit = unit;
            this.actionType = actionType;
            this.direction = direction;
            this.assignmentTime = assignmentTime;
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
                    return unit.Type.produceTime;
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

