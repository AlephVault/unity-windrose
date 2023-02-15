using System;
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
            DOWN, LEFT, RIGHT, UP, FRONT = DOWN, DEFAULT = DOWN
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

            private static Tuple<int, int>[][] relatives =
            {
                new[] {
                    new Tuple<int, int>(0, 1),
                    new Tuple<int, int>(0, -1),
                    new Tuple<int, int>(-1, 0),
                    new Tuple<int, int>(1, 0)
                },
                new[] {
                    new Tuple<int, int>(0, -1),
                    new Tuple<int, int>(0, 1),
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(-1, 0)
                },
                new[] {
                    new Tuple<int, int>(-1, 0),
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(0, -1),
                    new Tuple<int, int>(0, 1)
                },
                new[] {
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(-1, 0),
                    new Tuple<int, int>(0, 1),
                    new Tuple<int, int>(0, -1)
                },
            };

            public static Tuple<int, int> Delta(this Direction orientation, Tuple<int, int> offset)
            {
                int x = 0;
                int y = 0;
                int d;
                switch (orientation)
                {
                    case Direction.UP:
                        d = 0;
                        break;
                    case Direction.DOWN:
                        d = 1;
                        break;
                    case Direction.LEFT:
                        d = 2;
                        break;
                    case Direction.RIGHT:
                        d = 3;
                        break;
                    default:
                        return null;
                }

                x += relatives[d][0].Item1 * offset.Item2;
                y += relatives[d][0].Item2 * offset.Item2;
                x += relatives[d][3].Item1 * offset.Item1;
                y += relatives[d][3].Item2 * offset.Item1;
                return new Tuple<int, int>(x, y);
            }

            public static Tuple<int, int> Delta(this Direction orientation, params Direction[] steps)
            {
                int x = 0;
                int y = 0;
                int d;
                switch (orientation)
                {
                    case Direction.UP:
                        d = 0;
                        break;
                    case Direction.DOWN:
                        d = 1;
                        break;
                    case Direction.LEFT:
                        d = 2;
                        break;
                    case Direction.RIGHT:
                        d = 3;
                        break;
                    default:
                        return null;
                }
                
                foreach (Direction step in steps)
                {
                    Tuple<int, int> offset;
                    switch (step)
                    {
                        case Direction.UP:
                            offset = relatives[d][0];
                            break;
                        case Direction.DOWN:
                            offset = relatives[d][1];
                            break;
                        case Direction.LEFT:
                            offset = relatives[d][2];
                            break;
                        case Direction.RIGHT:
                            offset = relatives[d][3];
                            break;
                        default:
                            return null;
                    }

                    x += offset.Item1;
                    y += offset.Item2;
                }
                return new Tuple<int, int>(x, y);
            }
        }
    }
}
