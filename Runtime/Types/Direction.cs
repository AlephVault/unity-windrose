using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Types
    {
        /// <summary>
        ///   These are all the supported directions.
        /// </summary>
        public enum Direction {
            DOWN, LEFT, RIGHT, UP, FRONT = DOWN
        }

        public static class DirectionMethods
        {
            public static int Axis(this Direction direction)
            {
                switch (direction)
                {
                    case Direction.UP:
                    case Direction.DOWN:
                        return 1;
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        return 0;
                    default:
                        return -1;
                }
            }

            public static bool SameAxis(this Direction direction, Direction other)
            {
                return Axis(direction) == Axis(other);
            }
            
            public static Direction? Opposite(this Direction? direction)
            {
                switch(direction)
                {
                    case Direction.UP:
                        return Direction.DOWN;
                    case Direction.DOWN:
                        return Direction.UP;
                    case Direction.LEFT:
                        return Direction.RIGHT;
                    case Direction.RIGHT:
                        return Direction.LEFT;
                    default:
                        return null;
                }
            }

            public static Direction Opposite(this Direction direction)
            {
                switch (direction)
                {
                    case Direction.UP:
                        return Direction.DOWN;
                    case Direction.DOWN:
                        return Direction.UP;
                    case Direction.LEFT:
                        return Direction.RIGHT;
                    case Direction.RIGHT:
                        return Direction.LEFT;
                    default:
                        return Direction.FRONT;
                }
            }
        }
    }
}
