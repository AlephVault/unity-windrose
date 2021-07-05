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
        }
    }
}
