namespace GameMeanMachine.Unity.WindRose
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
                            }
                        }
                    }
                }
            }
        }
    }
}