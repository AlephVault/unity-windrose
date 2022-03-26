﻿using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.Base;
using UnityEditor;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace Tiles
            {
                namespace Strategies
                {
                    namespace Base
                    {
                        /// <summary>
                        ///   Layout strategies tell whether the cell is blocking or not.
                        ///   Blocking cells cannot be walked through (as determined in
                        ///   the Layout map strategy).
                        /// </summary>
                        [CreateAssetMenu(fileName = "NewLayoutTileStrategy", menuName = "Wind Rose/Tile Strategies/Layout", order = 201)]
                        public class LayoutTileStrategy : TileStrategy
                        {
                            /// <summary>
                            ///   The block mode used to configure this tile. An agreement
                            ///   on both tile's block mode and objects strategy will be
                            ///   computed to have the best possible implementation of the
                            ///   block settings to use in the map and from this tile.
                            /// </summary>
                            [SerializeField]
                            private BlockMode blockMode = BlockMode.Standard;

                            /// <summary>
                            ///   For standard-configured tiles, this setting tells
                            ///   whether the tile will block entry attempts in every
                            ///   direction or not (this will be ignored on exit attempts,
                            ///   and those attempts will be always deemed successful on
                            ///   the exit stage).
                            /// </summary>
                            [SerializeField]
                            private bool blocks;
                            
                            /// <summary>
                            ///   For non-standard configured tiles, this setting
                            ///   tells whether the tile will block entry attempts
                            ///   in the down side (up direction). For symmetric
                            ///   settings, this setting tells also whether the tile
                            ///   will block exit attempts in the down side (down
                            ///   direction).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksDownEntry;

                            /// <summary>
                            ///   For non-standard configured tiles, this setting
                            ///   tells whether the tile will block entry attempts
                            ///   in the left side (right direction). For symmetric
                            ///   settings, this setting tells also whether the tile
                            ///   will block exit attempts in the left side (left
                            ///   direction).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksLeftEntry;

                            /// <summary>
                            ///   For non-standard configured tiles, this setting
                            ///   tells whether the tile will block entry attempts
                            ///   in the right side (left direction). For symmetric
                            ///   settings, this setting tells also whether the tile
                            ///   will block exit attempts in the right side (right
                            ///   direction).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksRightEntry;

                            /// <summary>
                            ///   For non-standard configured tiles, this setting
                            ///   tells whether the tile will block entry attempts
                            ///   in the up side (down direction). For symmetric
                            ///   settings, this setting tells also whether the tile
                            ///   will block exit attempts in the up side (up
                            ///   direction).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksUpEntry;

                            /// <summary>
                            ///   For custom configured tiles, this setting tells
                            ///   whether the tile will block exit attempts in the
                            ///   down side (down direction). This setting is not
                            ///   used in the custom-symmetric (since corresponding
                            ///   <see cref="blocksDownEntry"/> is used instead, by
                            ///   also accounting for exit way) or the custom-enter
                            ///   mode (since the exit is always allowed there).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksDownExit;

                            /// <summary>
                            ///   For custom configured tiles, this setting tells
                            ///   whether the tile will block exit attempts in the
                            ///   left side (left direction). This setting is not
                            ///   used in the custom-symmetric (since corresponding
                            ///   <see cref="blocksLeftEntry"/> is used instead, by
                            ///   also accounting for exit way) or the custom-enter
                            ///   mode (since the exit is always allowed there).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksLeftExit;

                            /// <summary>
                            ///   For custom configured tiles, this setting tells
                            ///   whether the tile will block exit attempts in the
                            ///   right side (right direction). This setting is not
                            ///   used in the custom-symmetric (since corresponding
                            ///   <see cref="blocksRightEntry"/> is used instead, by
                            ///   also accounting for exit way) or the custom-enter
                            ///   mode (since the exit is always allowed there).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksRightExit;

                            /// <summary>
                            ///   For custom configured tiles, this setting tells
                            ///   whether the tile will block exit attempts in the
                            ///   up side (up direction). This setting is not used
                            ///   in the custom-symmetric (since corresponding
                            ///   <see cref="blocksUpEntry"/> is used instead, by
                            ///   also accounting for exit way) or the custom-enter
                            ///   mode (since the exit is always allowed there).
                            /// </summary>
                            [SerializeField]
                            private BlockType blocksUpExit;

                            /// <summary>
                            ///   Tells a standard-compatible block setting for this
                            ///   tile. Typically, the setting comes from a convergence
                            ///   of several values (by checking that at lease one of
                            ///   them blocks, or releases, or none does anything)
                            ///   when not coming from a standard setting as well.
                            /// </summary>
                            public BlockType Blocks {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                            return blocksDownEntry.Or(blocksUpEntry)
                                                   .Or(blocksLeftEntry).Or(blocksRightEntry);
                                        case BlockMode.Custom:
                                            return blocksDownEntry.Or(blocksUpEntry)
                                                   .Or(blocksLeftEntry).Or(blocksRightEntry)
                                                   .Or(blocksDownExit).Or(blocksUpExit)
                                                   .Or(blocksLeftExit).Or(blocksRightExit);
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }

                            /// <summary>
                            ///   Tells a custom-enter or custom-symmetric block setting
                            ///   for this tile. Typically, the setting comes from a convergence
                            ///   of the enter and exit modes, or taking only taking the enter
                            ///   mode. This only applies for the down direction.
                            /// </summary>
                            public BlockType BlocksDown
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                            return blocksDownEntry;
                                        case BlockMode.Custom:
                                            return blocksDownEntry.Or(blocksDownExit);
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }

                            /// <summary>
                            ///   Tells a custom-enter or custom-symmetric block setting
                            ///   for this tile. Typically, the setting comes from a convergence
                            ///   of the enter and exit modes, or taking only taking the enter
                            ///   mode. This only applies for the left direction.
                            /// </summary>
                            public BlockType BlocksLeft
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                            return blocksLeftEntry;
                                        case BlockMode.Custom:
                                            return blocksLeftEntry.Or(blocksLeftExit);
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom-enter or custom-symmetric block setting
                            ///   for this tile. Typically, the setting comes from a convergence
                            ///   of the enter and exit modes, or taking only taking the enter
                            ///   mode. This only applies for the right direction.
                            /// </summary>
                            public BlockType BlocksRight
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                            return blocksRightEntry;
                                        case BlockMode.Custom:
                                            return blocksRightEntry.Or(blocksRightExit);
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom-enter or custom-symmetric block setting
                            ///   for this tile. Typically, the setting comes from a convergence
                            ///   of the enter and exit modes, or taking only taking the enter
                            ///   mode. This only applies for the Up direction.
                            /// </summary>
                            public BlockType BlocksUp
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                            return blocksUpEntry;
                                        case BlockMode.Custom:
                                            return blocksUpEntry.Or(blocksUpExit);
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the entry in the down direction.
                            /// </summary>
                            public BlockType BlocksDownEntry
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                        case BlockMode.Custom:
                                            return blocksDownEntry;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }

                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the entry in the left direction.
                            /// </summary>
                            public BlockType BlocksLeftEntry
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                        case BlockMode.Custom:
                                            return blocksLeftEntry;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the entry in the right direction.
                            /// </summary>
                            public BlockType BlocksRightEntry
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                        case BlockMode.Custom:
                                            return blocksRightEntry;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the entry in the up direction.
                            /// </summary>
                            public BlockType BlocksUpEntry
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                        case BlockMode.CustomSymmetric:
                                        case BlockMode.Custom:
                                            return blocksUpEntry;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the exit in the down direction.
                            /// </summary>
                            public BlockType BlocksDownExit
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                            return BlockType.Release;
                                        case BlockMode.CustomSymmetric:
                                            return blocksDownEntry;
                                        case BlockMode.Custom:
                                            return blocksDownExit;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }

                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the exit in the left direction.
                            /// </summary>
                            public BlockType BlocksLeftExit
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                            return BlockType.Release;
                                        case BlockMode.CustomSymmetric:
                                            return blocksLeftEntry;
                                        case BlockMode.Custom:
                                            return blocksLeftExit;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the exit in the right direction.
                            /// </summary>
                            public BlockType BlocksRightExit
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                            return BlockType.Release;
                                        case BlockMode.CustomSymmetric:
                                            return blocksRightEntry;
                                        case BlockMode.Custom:
                                            return blocksRightExit;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                            
                            /// <summary>
                            ///   Tells a custom block setting for this tile. This setup
                            ///   is intended for the exit in the up direction.
                            /// </summary>
                            public BlockType BlocksUpExit
                            {
                                get
                                {
                                    switch (blockMode)
                                    {
                                        case BlockMode.Standard:
                                            return blocks ? BlockType.Block : BlockType.Release;
                                        case BlockMode.CustomEnter:
                                            return BlockType.Release;
                                        case BlockMode.CustomSymmetric:
                                            return blocksUpEntry;
                                        case BlockMode.Custom:
                                            return blocksUpExit;
                                        default:
                                            return BlockType.LeaveUnchanged;
                                    }
                                }
                            }
                        }
                        
#if UNITY_EDITOR
#endif
                    }
                }
            }
        }
    }
}
