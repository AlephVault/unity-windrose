using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    using Types;
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
                                using Entities.Objects.Strategies;
                                using ScriptableObjects.Tiles;
                                using ScriptableObjects.Tiles.Strategies.Base;

                                /// <summary>
                                ///   <para>
                                ///     Layout management strategies involve the ability of tiles to tell whether
                                ///       nothing should move through them: movement will be forbidden on tiles
                                ///       that are marked as "blocking".
                                ///   </para>
                                ///   <para>
                                ///     Its counterpart is <see cref="Entities.Objects.Strategies.Base.LayoutObjectStrategy"/>.
                                ///   </para>
                                ///   <seealso cref="ObjectsManagementStrategy"/>
                                ///   <seealso cref="Entities.Objects.Strategies.Base.LayoutObjectStrategy"/>
                                /// </summary>
                                public class LayoutObjectsManagementStrategy : ObjectsManagementStrategy
                                {
                                    private Bitmask blockMask;

                                    private bool IsAdjacencyBlocked(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return blockMask.GetColumn(x - 1, y, y + height - 1, Bitmask.CheckType.ANY_BLOCKED);
                                            case Direction.DOWN:
                                                return blockMask.GetRow(x, x + width - 1, y - 1, Bitmask.CheckType.ANY_BLOCKED);
                                            case Direction.RIGHT:
                                                return blockMask.GetColumn(x + width, y, y + height - 1, Bitmask.CheckType.ANY_BLOCKED);
                                            case Direction.UP:
                                                return blockMask.GetRow(x, x + width - 1, y + height, Bitmask.CheckType.ANY_BLOCKED);
                                            default:
                                                return true;
                                        }
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Checking the ability to allocate movement involve the object is not trying
                                    ///       to move through tiles marked as blocked.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.CanAllocateMovement(Dictionary{Type, bool}, ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanAllocateMovement(Dictionary<Type, bool> otherComponentsResults, ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction, bool continued)
                                    {
                                        // Then check for cells being blocked
                                        return !IsAdjacencyBlocked(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, direction);
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Always allows to clear the current movement.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanClearMovement(Dictionary<Type, bool> otherComponentsResults, ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        // Just follows what the BaseStrategy tells
                                        return true;
                                    }

                                    /// <summary>
                                    ///   Computing the per-cell data involves keeping the nearest block flag.
                                    ///   This means: the last flag, among all tilemaps, in the <paramref name="x"/>
                                    ///     and <paramref name="y"/> position, will determine whether the cell
                                    ///     is blocked or not. Flag can only be extracted from a tile, if the tile
                                    ///     is a <see cref="BundledTile"/> and contains a <see cref="LayoutTileStrategy"/>.
                                    /// </summary>
                                    /// <param name="x">The X position to compute</param>
                                    /// <param name="y">The Y position to compute</param>
                                    /// <remarks>
                                    ///   If no tile is <see cref="BundledTile"/> with the expected
                                    ///   <see cref="LayoutTileStrategy"/>, the computed value is <c>false</c>.
                                    /// </remarks>
                                    public override void ComputeCellData(uint x, uint y)
                                    {
                                        bool blocks = false;
                                        foreach (UnityEngine.Tilemaps.Tilemap tilemap in StrategyHolder.Tilemaps)
                                        {
                                            UnityEngine.Tilemaps.TileBase tile = tilemap.GetTile(new Vector3Int((int)x, (int)y, 0));
                                            LayoutTileStrategy layoutTileStrategy = BundledTile.GetStrategyFrom<LayoutTileStrategy>(tile);
                                            if (layoutTileStrategy)
                                            {
                                                blocks = layoutTileStrategy.Blocks;
                                            }
                                        }
                                        blockMask.SetCell(x, y, blocks);
                                    }

                                    /// <summary>
                                    ///   Initializes an array of blocking flags to know whether the 
                                    /// </summary>
                                    public override void InitGlobalCellsData()
                                    {
                                        uint width = StrategyHolder.Map.Width;
                                        uint height = StrategyHolder.Map.Height;
                                        blockMask = new Bitmask(width, height);
                                    }

                                    protected override Type GetCounterpartType()
                                    {
                                        return typeof(Entities.Objects.Strategies.Base.LayoutObjectStrategy);
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
