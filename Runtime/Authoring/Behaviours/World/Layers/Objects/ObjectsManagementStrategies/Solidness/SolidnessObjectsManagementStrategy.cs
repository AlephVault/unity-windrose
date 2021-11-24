using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
                            namespace Solidness
                            {
                                using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies.Solidness;
                                using Types;

                                /// <summary>
                                ///   <para>
                                ///     Solidness management strategies involve how the objects occupy
                                ///       the cells in the map. This strategy will add the data of the
                                ///       objects solidness inside a <see cref="SolidMask"/>, which will
                                ///       be used to tell whether fully solid objects can walk through
                                ///       their cells or not.
                                ///   </para>
                                ///   <para>
                                ///     Its counterpart is <see cref="Entities.Objects.Strategies.Solidness.SolidnessObjectStrategy"/>.
                                ///   </para> 
                                /// </summary>
                                public class SolidnessObjectsManagementStrategy : ObjectsManagementStrategy
                                {
                                    private SolidMask solidMask;

                                    /// <summary>
                                    ///   The step type to use when processing objects' movements.
                                    /// </summary>
                                    [SerializeField]
                                    private StepType stepType = StepType.Safe;

                                    protected override Type GetCounterpartType()
                                    {
                                        return typeof(SolidnessObjectStrategy);
                                    }

                                    public override void InitGlobalCellsData()
                                    {
                                        uint width = StrategyHolder.Map.Width;
                                        uint height = StrategyHolder.Map.Height;
                                        solidMask = new SolidMask(width, height);
                                    }

                                    /*****************************************************************************
                                     * 
                                     * Object attachment.
                                     * 
                                     *****************************************************************************/

                                    /// <summary>
                                    ///   When the object is attached to the map, its solidness will be added to the
                                    ///     cell(s) it occupies.
                                    /// </summary>
                                    public override void AttachedStrategy(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                        SolidnessStatus solidness = solidnessStrategy.Solidness;
                                        IncrementBody(solidnessStrategy, status, solidness, solidnessStrategy.Mask);
                                    }

                                    /// <summary>
                                    ///   When the object is detached from the map, its solidness will be cleared from the
                                    ///     cell(s) it occupies.
                                    /// </summary>
                                    public override void DetachedStrategy(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                        SolidnessStatus solidness = solidnessStrategy.Solidness;
                                        DecrementBody(solidnessStrategy, status, solidness, solidnessStrategy.Mask);
                                    }

                                    /*****************************************************************************
                                     * 
                                     * Object movement.
                                     * 
                                     *****************************************************************************/

                                    /// <summary>
                                    ///   <para>
                                    ///     Allowing movement in certain direction involves checking for solidness according
                                    ///       to the objects occupying or not such cells and the solidness they add, and also
                                    ///       counting the current object's solidness.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.CanAllocateMovement(Dictionary{Type, bool}, Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool)"/>
                                    ///       for more information on this method's signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanAllocateMovement(Dictionary<Type, bool> otherComponentsResults, Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction, bool continued)
                                    {
                                        SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                        SolidnessStatus solidness = solidnessStrategy.Solidness;
                                        if (solidness.Irregular()) return false;
                                        return solidness.Traverses() || solidnessStrategy.TraversesOtherSolids || IsAdjacencyFree(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, direction);
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Always allows to clear the current movement.
                                    ///   </para>
                                    /// </summary>
                                    public override bool CanClearMovement(Dictionary<Type, bool> otherComponentsResults, Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        return true;
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Allocating a movement involves setting the solidness in the tiles "in front" of our object
                                    ///       (in terms of movement) according to the solidness of the current object's strategy. 
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.DoAllocateMovement(Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool, string)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override void DoAllocateMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction, bool continuated, string stage)
                                    {
                                        switch (stage)
                                        {
                                            case "AfterMovementAllocation":
                                                SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                                SolidnessStatus solidness = solidnessStrategy.Solidness;
                                                if (stepType == StepType.Safe)
                                                {
                                                    IncrementAdjacent(solidnessStrategy, status, solidness);
                                                }
                                                else if (stepType == StepType.Optimistic)
                                                {
                                                    DecrementBackSide(solidnessStrategy, status, solidness);
                                                    IncrementAdjacent(solidnessStrategy, status, solidness);
                                                }
                                                break;
                                        }
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Cancelling the movement involves releasing the solidness of the allocated movement, which involve the tiles
                                    ///       "in front" (in terms of movement) of the object.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.DoClearMovement(Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction?, string)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override void DoClearMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction? formerMovement, string stage)
                                    {
                                        switch (stage)
                                        {
                                            case "Before":
                                                SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                                SolidnessStatus solidness = solidnessStrategy.Solidness;
                                                if (stepType == StepType.Safe)
                                                {
                                                    DecrementAdjacent(solidnessStrategy, status, solidness);
                                                }
                                                else if (stepType == StepType.Optimistic)
                                                {
                                                    IncrementBackSide(solidnessStrategy, status, solidness);
                                                    DecrementAdjacent(solidnessStrategy, status, solidness);
                                                }
                                                break;
                                        }
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     When the movement is confirmed, the formed row/column being occupied is released. This is to reflect
                                    ///       the fact that those tiles are not being occupied anymore by this object.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.DoConfirmMovement(Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction?, string)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override void DoConfirmMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction? formerMovement, string stage)
                                    {
                                        switch (stage)
                                        {
                                            case "AfterPositionChange":
                                                SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                                SolidnessStatus solidness = solidnessStrategy.Solidness;
                                                if (stepType == StepType.Safe)
                                                {
                                                    DecrementOppositeAdjacent(solidnessStrategy, status, solidness);
                                                }
                                                else
                                                {
                                                    // Nothing to do here
                                                }
                                                break;
                                        }
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Refreshes the solidness of the object according to changes in the
                                    ///       "solidness" property. It clears the solidness from the previous
                                    ///       value, and sets the solidness of the new value.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.DoProcessPropertyUpdate(Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, string, object, object)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override void DoProcessPropertyUpdate(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, string property, object oldValue, object newValue)
                                    {
                                        // Debug.LogFormat("Type of strategy: {0} - strategy {1}", strategy.GetType().FullName, strategy);
                                        SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                        // Please note: we condition the retrieval of the mask, since the retrieval COPIES the mask value! And would not be needed
                                        //   if the solidness was NOT the Mask one.
                                        if (ReferenceEquals(property, SolidnessObjectStrategy.SolidnessProperty))
                                        {
                                            StrategyHolder.MovementCancel(strategy.StrategyHolder);
                                            DecrementBody(solidnessStrategy, status, (SolidnessStatus)oldValue, (SolidnessStatus)oldValue == SolidnessStatus.Mask ? solidnessStrategy.Mask : null);
                                            IncrementBody(solidnessStrategy, status, (SolidnessStatus)newValue, (SolidnessStatus)newValue == SolidnessStatus.Mask ? solidnessStrategy.Mask : null);
                                        }
                                        else if (ReferenceEquals(property, SolidnessObjectStrategy.MaskProperty))
                                        {
                                            StrategyHolder.MovementCancel(strategy.StrategyHolder);
                                            DecrementBody(solidnessStrategy, status, solidnessStrategy.Solidness, solidnessStrategy.Solidness == SolidnessStatus.Mask ? (SolidObjectMask)oldValue : null);
                                            IncrementBody(solidnessStrategy, status, solidnessStrategy.Solidness, solidnessStrategy.Solidness == SolidnessStatus.Mask ? (SolidObjectMask)newValue : null);
                                        }
                                    }

                                    /// <summary>
                                    ///   <para>
                                    ///     Processing the teleport of an object involves clearing the solidness in the tile(s) being left, and
                                    ///       setting the solidness in the tile(s) being occupied.
                                    ///   </para>
                                    ///   <para>
                                    ///     See <see cref="ObjectsManagementStrategy.DoTeleport(Objects.Strategies.ObjectStrategy, ObjectsManagementStrategyHolder.Status, uint, uint, string)"/>
                                    ///       for more information on this method signature and intention.
                                    ///   </para>
                                    /// </summary>
                                    public override void DoTeleport(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, uint x, uint y, string stage)
                                    {
                                        SolidnessObjectStrategy solidnessStrategy = (SolidnessObjectStrategy)strategy;
                                        SolidnessStatus solidness = solidnessStrategy.Solidness;
                                        switch (stage)
                                        {
                                            case "Before":
                                                DecrementBody(solidnessStrategy, status, solidness, solidnessStrategy.Mask);
                                                break;
                                            case "AfterPositionChange":
                                                IncrementBody(solidnessStrategy, status, solidness, solidnessStrategy.Mask);
                                                break;
                                        }
                                    }

                                    /**
                                     * 
                                     * Private methods of this particular strategy according to a particular object
                                     *   strategy, solidness, and status.
                                     * 
                                     */

                                    private void IncrementBody(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness, SolidObjectMask mask)
                                    {
                                        if (solidness.Irregular())
                                        {
                                            UpdateIrregularBody(mask, status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height);
                                        }
                                        else if (solidness.Occupies())
                                        {
                                            IncrementBody(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            DecrementBody(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height);
                                        }
                                    }

                                    private void DecrementBody(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness, SolidObjectMask mask)
                                    {
                                        if (solidness.Irregular())
                                        {
                                            UpdateIrregularBody(mask, status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, -1);
                                        }
                                        else if (solidness.Occupies())
                                        {
                                            DecrementBody(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            IncrementBody(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height);
                                        }
                                    }

                                    private void IncrementAdjacent(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness)
                                    {
                                        if (solidness.Occupies())
                                        {
                                            IncrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            DecrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                    }

                                    private void DecrementAdjacent(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness)
                                    {
                                        if (solidness.Occupies())
                                        {
                                            DecrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            IncrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                    }

                                    private void IncrementBackSide(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness)
                                    {
                                        if (solidness.Occupies())
                                        {
                                            IncrementBackSide(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            DecrementBackSide(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                    }

                                    private void DecrementBackSide(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness)
                                    {
                                        if (solidness.Occupies())
                                        {
                                            DecrementBackSide(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                        else if (solidness.Carves())
                                        {
                                            IncrementBackSide(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement);
                                        }
                                    }

                                    private void DecrementOppositeAdjacent(SolidnessObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, SolidnessStatus solidness)
                                    {
                                        if (solidness.Occupies())
                                        {
                                            DecrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement.Opposite());
                                        }
                                        else if (solidness.Carves())
                                        {
                                            IncrementAdjacent(status.X, status.Y, strategy.StrategyHolder.Object.Width, strategy.StrategyHolder.Object.Height, status.Movement.Opposite());
                                        }
                                    }

                                    /*****************************************************************************
                                     * 
                                     * Private methods of this particular strategy.
                                     * 
                                     *****************************************************************************/

                                    private void UpdateIrregularBody(SolidObjectMask mask, uint x, uint y, uint width, uint height, short factor = 1)
                                    {
                                        for (uint i = 0; i < width; i++)
                                        {
                                            for (uint j = 0; j < height; j++)
                                            {
                                                short amount = 0;
                                                switch (mask[i, j])
                                                {
                                                    case SolidnessStatus.Hole:
                                                        amount = -1;
                                                        break;
                                                    case SolidnessStatus.Solid:
                                                        amount = 1;
                                                        break;
                                                }
                                                solidMask.ChangeCellBy(i + x, j + y, (short)(amount * factor));
                                            }
                                        }
                                    }

                                    private void IncrementBody(uint x, uint y, uint width, uint height)
                                    {
                                        solidMask.IncSquare(x, y, width, height);
                                    }

                                    private void DecrementBody(uint x, uint y, uint width, uint height)
                                    {
                                        solidMask.DecSquare(x, y, width, height);
                                    }

                                    private bool IsHittingEdge(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return x == 0;
                                            case Direction.UP:
                                                return y + height == solidMask.height;
                                            case Direction.RIGHT:
                                                return x + width == solidMask.width;
                                            case Direction.DOWN:
                                                return y == 0;
                                        }
                                        return false;
                                    }

                                    private bool IsAdjacencyFree(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        /** Precondition: IsHittingEdge was already called to this point */
                                        switch (direction)
                                        {
                                            case Direction.LEFT:
                                                return solidMask.EmptyColumn(x - 1, y, height);
                                            case Direction.DOWN:
                                                return solidMask.EmptyRow(x, y - 1, width);
                                            case Direction.RIGHT:
                                                return solidMask.EmptyColumn(x + width, y, height);
                                            case Direction.UP:
                                                return solidMask.EmptyRow(x, y + height, width);
                                            default:
                                                return true;
                                        }
                                    }

                                    private void IncrementAdjacent(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        if (!IsHittingEdge(x, y, width, height, direction))
                                        {
                                            switch (direction)
                                            {
                                                case Direction.LEFT:
                                                    solidMask.IncColumn(x - 1, y, height);
                                                    break;
                                                case Direction.DOWN:
                                                    solidMask.IncRow(x, y - 1, width);
                                                    break;
                                                case Direction.RIGHT:
                                                    solidMask.IncColumn(x + width, y, height);
                                                    break;
                                                case Direction.UP:
                                                    solidMask.IncRow(x, y + height, width);
                                                    break;
                                            }
                                        }
                                    }

                                    private void DecrementAdjacent(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        if (!IsHittingEdge(x, y, width, height, direction))
                                        {
                                            switch (direction)
                                            {
                                                case Direction.LEFT:
                                                    solidMask.DecColumn(x - 1, y, height);
                                                    break;
                                                case Direction.DOWN:
                                                    solidMask.DecRow(x, y - 1, width);
                                                    break;
                                                case Direction.RIGHT:
                                                    solidMask.DecColumn(x + width, y, height);
                                                    break;
                                                case Direction.UP:
                                                    solidMask.DecRow(x, y + height, width);
                                                    break;
                                            }
                                        }
                                    }

                                    private void IncrementBackSide(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        if (!IsHittingEdge(x, y, width, height, direction))
                                        {
                                            switch (direction)
                                            {
                                                case Direction.RIGHT:
                                                    solidMask.IncColumn(x, y, height);
                                                    break;
                                                case Direction.UP:
                                                    solidMask.IncRow(x, y, width);
                                                    break;
                                                case Direction.LEFT:
                                                    solidMask.IncColumn(x + width - 1, y, height);
                                                    break;
                                                case Direction.DOWN:
                                                    solidMask.IncRow(x, y + height - 1, width);
                                                    break;
                                            }
                                        }
                                    }

                                    private void DecrementBackSide(uint x, uint y, uint width, uint height, Direction? direction)
                                    {
                                        if (!IsHittingEdge(x, y, width, height, direction))
                                        {
                                            switch (direction)
                                            {
                                                case Direction.RIGHT:
                                                    solidMask.DecColumn(x, y, height);
                                                    break;
                                                case Direction.UP:
                                                    solidMask.DecRow(x, y, width);
                                                    break;
                                                case Direction.LEFT:
                                                    solidMask.DecColumn(x + width - 1, y, height);
                                                    break;
                                                case Direction.DOWN:
                                                    solidMask.DecRow(x, y + height - 1, width);
                                                    break;
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
    }
}
