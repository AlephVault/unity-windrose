namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Base
                            {
                                /// <summary>
                                ///   The action to do in the block matrix for a given
                                ///   cell (using a blocking tile strategy).
                                /// </summary>
                                public enum BlockType
                                {
                                    /// <summary>
                                    ///   Marks the current cell and setting as released
                                    ///   (free).
                                    /// </summary>
                                    Release,
                                    /// <summary>
                                    ///   Leaves the current cell and setting unchanged.
                                    /// </summary>
                                    LeaveUnchanged,
                                    /// <summary>
                                    ///   Marks the current cell and setting as blocked.
                                    /// </summary>
                                    Block
                                }
                                
                                /// <summary>
                                ///   Some extension methods for the BlockType.
                                /// </summary>
                                public static class BlockTypeMethods
                                {
                                    /// <summary>
                                    ///   Merges two block types to the strongest condition
                                    ///   of both (Block > Release > LeaveUnchanged).
                                    /// </summary>
                                    /// <param name="type1">The first value to merge</param>
                                    /// <param name="type2">The second value to merge</param>
                                    /// <returns>The result</returns>
                                    public static BlockType Or(this BlockType type1, BlockType type2)
                                    {
                                        if (type1 == BlockType.Block || type2 == BlockType.Block)
                                        {
                                            return BlockType.Block;
                                        }

                                        if (type1 == BlockType.Release || type2 == BlockType.Release)
                                        {
                                            return BlockType.Release;
                                        }

                                        return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}