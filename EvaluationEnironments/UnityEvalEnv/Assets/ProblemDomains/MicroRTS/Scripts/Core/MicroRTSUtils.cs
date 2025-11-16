using UnityEngine;

namespace Problems.MicroRTS.Core
{
    public static class MicroRTSUtils
    {
        // Direction constants matching project implementation
        // DIRECTION_NONE=0, UP=1, RIGHT=2, DOWN=3, LEFT=4 (same as action values)
        public const int DIRECTION_NONE = 0;
        public const int DIRECTION_UP = 1;
        public const int DIRECTION_RIGHT = 2;
        public const int DIRECTION_DOWN = 3;
        public const int DIRECTION_LEFT = 4;

        public static Vector2Int GetDirectionOffset(int direction)
        {
            switch (direction)
            {
                case DIRECTION_UP:
                    return new Vector2Int(0, -1); // moving up -> decrease Y
                case DIRECTION_RIGHT:
                    return new Vector2Int(1, 0); // moving right -> increase X
                case DIRECTION_DOWN:
                    return new Vector2Int(0, 1); // moving down -> increase Y
                case DIRECTION_LEFT:
                    return new Vector2Int(-1, 0); // moving left -> decrease X
                default:
                    return Vector2Int.zero;
            }
        }
    }
}

