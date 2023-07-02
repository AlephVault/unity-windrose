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
                                ///   Specifies the layout to use to determine how
                                ///   the block settings are interpreted in different
                                ///   directions.
                                /// </summary>
                                public enum BlockMode
                                {
                                    /// <summary>
                                    ///   This setting involves one single block flag.
                                    ///   This block flag tells what happens when an
                                    ///   object attempts to *enter* that node (being
                                    ///   it after the edge of the movement start), on
                                    ///   any direction for that movement. On exit,
                                    ///   there will be no block (so the enter flags
                                    ///   will become all blocked, and the exit flags
                                    ///   will become all free).
                                    /// </summary>
                                    Standard,
                                    /// <summary>
                                    ///   This setting involves four flags, one per
                                    ///   direction to tell how it treats entry attempts,
                                    ///   while exit attempts will be always free / ok.
                                    /// </summary>
                                    CustomEnter,
                                    /// <summary>
                                    ///   This setting involves four flags, one per
                                    ///   direction to tell how it treats both entry
                                    ///   and exit attempts in that direction.
                                    /// </summary>
                                    CustomSymmetric,
                                    /// <summary>
                                    ///   This setting involves eight flags, two per
                                    ///   direction (and, therein, one for entry and
                                    ///   one for exit attempts respectively). Each
                                    ///   flag independently tells what to do on each
                                    ///   attempt direction (top, down, left, bottom)
                                    ///   and way (enter, leave).
                                    /// </summary>
                                    Custom
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}