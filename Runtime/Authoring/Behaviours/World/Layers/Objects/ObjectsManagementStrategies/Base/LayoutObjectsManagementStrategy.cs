using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose
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
                                    /// <summary>
                                    ///   The block mode for this strategy. Used for the agreement.
                                    /// </summary>
                                    [SerializeField]
                                    private BlockMode blockMode = BlockMode.Standard;

                                    // A pointer to the function to use, for the chosen block mode
                                    // (on initialization), to initialize the tile block masks.
                                    private Action<uint, uint> computeCellDataCallback;
                                    
                                    // A pointer to the function to use, for the chosen block mode
                                    // (on initialization), to check the tile block masks.
                                    private Func<uint, uint, uint, uint, Direction, bool> isAdjacencyBlockedCallback;
                                    
                                    // For standard block mode, the block mask to use (this spans
                                    // only for entry in all directions - exit is always allowed).
                                    private Bitmask blockMask;

                                    // For non-standard block modes, the black mask to use in down
                                    // entry (custom, custom-enter), or down entry and exit (custom
                                    // symmetric).
                                    private Bitmask downEntryBlockMask;

                                    // For non-standard block modes, the black mask to use in left
                                    // entry (custom, custom-enter), or left entry and exit (custom
                                    // symmetric).
                                    private Bitmask leftEntryBlockMask;

                                    // For non-standard block modes, the black mask to use in right
                                    // entry (custom, custom-enter), or right entry and exit (custom
                                    // symmetric).
                                    private Bitmask rightEntryBlockMask;

                                    // For non-standard block modes, the black mask to use in up
                                    // entry (custom, custom-enter), or up entry and exit (custom
                                    // symmetric).
                                    private Bitmask upEntryBlockMask;

                                    // For custom block mode, the block mask to use in down exit.
                                    private Bitmask downExitBlockMask;

                                    // For custom block mode, the block mask to use in left exit.
                                    private Bitmask leftExitBlockMask;

                                    // For custom block mode, the block mask to use in right exit.
                                    private Bitmask rightExitBlockMask;
                                    
                                    // For custom block mode, the block mask to use in up exit.
                                    private Bitmask upExitBlockMask;

                                    // Block check callback for standard type.
                                    private bool IsAdjacencyBlockedStandard(uint x, uint y, uint width, uint height, Direction direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return blockMask.GetColumn(x - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.DOWN:
                                                return blockMask.GetRow(x, x + width - 1, y - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.RIGHT:
                                                return blockMask.GetColumn(x + width, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.UP:
                                                return blockMask.GetRow(x, x + width - 1, y + height, Bitmask.CheckType.ANY_SET);
                                            default:
                                                return true;
                                        }
                                    }

                                    // Block check callback for custom enter type.
                                    private bool IsAdjacencyBlockedCustomEnter(uint x, uint y, uint width, uint height, Direction direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return rightEntryBlockMask.GetColumn(x - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.DOWN:
                                                return upEntryBlockMask.GetRow(x, x + width - 1, y - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.RIGHT:
                                                return leftEntryBlockMask.GetColumn(x + width, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.UP:
                                                return downEntryBlockMask.GetRow(x, x + width - 1, y + height, Bitmask.CheckType.ANY_SET);
                                            default:
                                                return true;
                                        }
                                    }

                                    // Block check callback for custom symmetric type.
                                    private bool IsAdjacencyBlockedCustomSymmetric(uint x, uint y, uint width, uint height, Direction direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return rightEntryBlockMask.GetColumn(x - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET) ||
                                                       leftEntryBlockMask.GetColumn(x, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.DOWN:
                                                return upEntryBlockMask.GetRow(x, x + width - 1, y - 1, Bitmask.CheckType.ANY_SET) ||
                                                       downEntryBlockMask.GetRow(x, x + width - 1, y, Bitmask.CheckType.ANY_SET);
                                            case Direction.RIGHT:
                                                return leftEntryBlockMask.GetColumn(x + width, y, y + height - 1, Bitmask.CheckType.ANY_SET) ||
                                                       rightEntryBlockMask.GetColumn(x + width - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.UP:
                                                return downEntryBlockMask.GetRow(x, x + width - 1, y + height, Bitmask.CheckType.ANY_SET) ||
                                                       upEntryBlockMask.GetRow(x, x + width - 1, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            default:
                                                return true;
                                        }
                                    }

                                    // Block check callback for custom type.
                                    private bool IsAdjacencyBlockedCustom(uint x, uint y, uint width, uint height, Direction direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return rightEntryBlockMask.GetColumn(x - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET) ||
                                                       leftExitBlockMask.GetColumn(x, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.DOWN:
                                                return upEntryBlockMask.GetRow(x, x + width - 1, y - 1, Bitmask.CheckType.ANY_SET) ||
                                                       downExitBlockMask.GetRow(x, x + width - 1, y, Bitmask.CheckType.ANY_SET);
                                            case Direction.RIGHT:
                                                return leftEntryBlockMask.GetColumn(x + width, y, y + height - 1, Bitmask.CheckType.ANY_SET) ||
                                                       rightExitBlockMask.GetColumn(x + width - 1, y, y + height - 1, Bitmask.CheckType.ANY_SET);
                                            case Direction.UP:
                                                return downEntryBlockMask.GetRow(x, x + width - 1, y + height, Bitmask.CheckType.ANY_SET) ||
                                                       upExitBlockMask.GetRow(x, x + width - 1, y + height - 1, Bitmask.CheckType.ANY_SET);
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
                                    ///     See <see cref="ObjectsManagementStrategy.CanAllocateMovement(ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanAllocateMovement(ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction, bool continued)
                                    {
                                        // Then check for cells being blocked
                                        return !isAdjacencyBlockedCallback(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, direction);
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Always allows to clear the current movement.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanClearMovement(ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
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
                                        computeCellDataCallback(x, y);
                                    }
                                    
                                    // Computes cell data for the Standard case.
                                    private void ComputeCellDataStandard(uint x, uint y)
                                    {
                                        bool blocks = false;
                                        foreach (UnityEngine.Tilemaps.Tilemap tilemap in StrategyHolder.Tilemaps)
                                        {
                                            UnityEngine.Tilemaps.TileBase tile = tilemap.GetTile(new Vector3Int((int)x, (int)y, 0));
                                            LayoutTileStrategy layoutTileStrategy = BundledTile.GetStrategyFrom<LayoutTileStrategy>(tile);
                                            if (layoutTileStrategy)
                                            {
                                                switch (layoutTileStrategy.Blocks)
                                                {
                                                    case BlockType.Block:
                                                        blocks = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocks = false;
                                                        break;
                                                }
                                            }
                                        }
                                        blockMask.SetCell(x, y, blocks);
                                    }
                                    
                                    // Computes cell data for the custom-symmetric / custom-enter case.
                                    private void ComputeCellDataCustomEnter(uint x, uint y)
                                    {
                                        bool blocksDown = false;
                                        bool blocksLeft = false;
                                        bool blocksRight = false;
                                        bool blocksUp = false;
                                        foreach (UnityEngine.Tilemaps.Tilemap tilemap in StrategyHolder.Tilemaps)
                                        {
                                            UnityEngine.Tilemaps.TileBase tile = tilemap.GetTile(new Vector3Int((int)x, (int)y, 0));
                                            LayoutTileStrategy layoutTileStrategy = BundledTile.GetStrategyFrom<LayoutTileStrategy>(tile);
                                            if (layoutTileStrategy)
                                            {
                                                switch (layoutTileStrategy.BlocksDownEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksDown = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksDown = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksLeftEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksLeft = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksLeft = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksRightEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksRight = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksRight = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksUpEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksUp = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksUp = false;
                                                        break;
                                                }
                                            }
                                        }
                                        downEntryBlockMask.SetCell(x, y, blocksDown);
                                        leftEntryBlockMask.SetCell(x, y, blocksLeft);
                                        rightEntryBlockMask.SetCell(x, y, blocksRight);
                                        upEntryBlockMask.SetCell(x, y, blocksUp);
                                    }

                                    // Computes cell data for the custom case.
                                    private void ComputeCellDataCustomTwoWays(uint x, uint y)
                                    {
                                        bool blocksDownEntry = false;
                                        bool blocksLeftEntry = false;
                                        bool blocksRightEntry = false;
                                        bool blocksUpEntry = false;
                                        bool blocksDownExit = false;
                                        bool blocksLeftExit = false;
                                        bool blocksRightExit = false;
                                        bool blocksUpExit = false;
                                        foreach (UnityEngine.Tilemaps.Tilemap tilemap in StrategyHolder.Tilemaps)
                                        {
                                            UnityEngine.Tilemaps.TileBase tile = tilemap.GetTile(new Vector3Int((int)x, (int)y, 0));
                                            LayoutTileStrategy layoutTileStrategy = BundledTile.GetStrategyFrom<LayoutTileStrategy>(tile);
                                            if (layoutTileStrategy)
                                            {
                                                switch (layoutTileStrategy.BlocksDownEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksDownEntry = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksDownEntry = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksLeftEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksLeftEntry = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksLeftEntry = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksRightEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksRightEntry = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksRightEntry = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksUpEntry)
                                                {
                                                    case BlockType.Block:
                                                        blocksUpEntry = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksUpEntry = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksDownExit)
                                                {
                                                    case BlockType.Block:
                                                        blocksDownExit = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksDownExit = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksLeftExit)
                                                {
                                                    case BlockType.Block:
                                                        blocksLeftExit = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksLeftExit = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksRightExit)
                                                {
                                                    case BlockType.Block:
                                                        blocksRightExit = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksRightExit = false;
                                                        break;
                                                }
                                                switch (layoutTileStrategy.BlocksUpExit)
                                                {
                                                    case BlockType.Block:
                                                        blocksUpExit = true;
                                                        break;
                                                    case BlockType.Release:
                                                        blocksUpExit = false;
                                                        break;
                                                }
                                            }
                                        }
                                        downEntryBlockMask.SetCell(x, y, blocksDownEntry);
                                        leftEntryBlockMask.SetCell(x, y, blocksLeftEntry);
                                        rightEntryBlockMask.SetCell(x, y, blocksRightEntry);
                                        upEntryBlockMask.SetCell(x, y, blocksUpEntry);
                                        downExitBlockMask.SetCell(x, y, blocksDownExit);
                                        leftExitBlockMask.SetCell(x, y, blocksLeftExit);
                                        rightExitBlockMask.SetCell(x, y, blocksRightExit);
                                        upExitBlockMask.SetCell(x, y, blocksUpExit);
                                    }

                                    /// <summary>
                                    ///   Initializes an array of blocking flags to know whether the movement
                                    ///   is allowed or not toward that cell.
                                    /// </summary>
                                    public override void InitGlobalCellsData()
                                    {
                                        uint width = StrategyHolder.Map.Width;
                                        uint height = StrategyHolder.Map.Height;
                                        switch (blockMode)
                                        {
                                            case BlockMode.Standard:
                                                blockMask = new Bitmask(width, height);
                                                computeCellDataCallback = ComputeCellDataStandard;
                                                isAdjacencyBlockedCallback = IsAdjacencyBlockedStandard;
                                                break;
                                            case BlockMode.CustomEnter:
                                                downEntryBlockMask = new Bitmask(width, height);
                                                leftEntryBlockMask = new Bitmask(width, height);
                                                rightEntryBlockMask = new Bitmask(width, height);
                                                upEntryBlockMask = new Bitmask(width, height);
                                                computeCellDataCallback = ComputeCellDataCustomEnter;
                                                isAdjacencyBlockedCallback = IsAdjacencyBlockedCustomEnter;
                                                break;
                                            case BlockMode.CustomSymmetric:
                                                downEntryBlockMask = new Bitmask(width, height);
                                                leftEntryBlockMask = new Bitmask(width, height);
                                                rightEntryBlockMask = new Bitmask(width, height);
                                                upEntryBlockMask = new Bitmask(width, height);
                                                computeCellDataCallback = ComputeCellDataCustomEnter;
                                                isAdjacencyBlockedCallback = IsAdjacencyBlockedCustomSymmetric;
                                                break;
                                            case BlockMode.Custom:
                                                downEntryBlockMask = new Bitmask(width, height);
                                                leftEntryBlockMask = new Bitmask(width, height);
                                                rightEntryBlockMask = new Bitmask(width, height);
                                                upEntryBlockMask = new Bitmask(width, height);
                                                downExitBlockMask = new Bitmask(width, height);
                                                leftExitBlockMask = new Bitmask(width, height);
                                                rightExitBlockMask = new Bitmask(width, height);
                                                upExitBlockMask = new Bitmask(width, height);
                                                computeCellDataCallback = ComputeCellDataCustomTwoWays;
                                                isAdjacencyBlockedCallback = IsAdjacencyBlockedCustom;
                                                break;
                                            default:
                                                throw new ArgumentException($"Invalid block mode: {blockMask}");
                                        }
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
