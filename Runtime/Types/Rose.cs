namespace AlephVault.Unity.WindRose
{
    namespace Types
    {
        /// <summary>
        ///   A rose is a tuple of any type, where each field is readonly.
        /// </summary>
        public class RoseTuple<T>
        {
            /// <summary>
            ///   Value for Down direction.
            /// </summary>
            public readonly T Down;
            
            /// <summary>
            ///   Value for Left direction.
            /// </summary>
            public readonly T Left;
            
            /// <summary>
            ///   Value for Right direction.
            /// </summary>
            public readonly T Right;
            
            /// <summary>
            ///   Value for Up direction.
            /// </summary>
            public readonly T Up;

            /// <summary>
            ///   Quick build of a rose tuple specifying values for all the directions.
            /// </summary>
            /// <param name="up">The value for Up direction</param>
            /// <param name="left">The value for Left direction</param>
            /// <param name="right">The value for Right direction</param>
            /// <param name="down">The value for Down direction</param>
            public RoseTuple(T up, T left, T right, T down)
            {
                Up = up;
                Left = left;
                Right = right;
                Down = down;
            }

            /// <summary>
            ///   Gets a value for a given direction.
            /// </summary>
            /// <param name="direction">The direction to get a value by</param>
            /// <returns>The value to return</returns>
            public T Get(Direction direction)
            {
                switch (direction)
                {
                    case Direction.UP:
                        return Up;
                    case Direction.LEFT:
                        return Left;
                    case Direction.RIGHT:
                        return Right;
                    case Direction.DOWN:
                        return Down;
                    default:
                        return Down;
                }
            }
        }
    }
}
