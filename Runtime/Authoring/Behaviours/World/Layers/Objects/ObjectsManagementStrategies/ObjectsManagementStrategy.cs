using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies;
using System;
using System.Collections.Generic;
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
                            /// <summary>
                            ///   <para>
                            ///     Object management strategies provide the actual rule of the movement in
                            ///       the map. They will be related to a counterpart object strategy they
                            ///       will check and rule against.
                            ///   </para>
                            ///   <para>
                            ///     This class provides several methods that may / MUST (depending on the
                            ///       methods) be implemented, and are strict part of the logic they handle.
                            ///   </para>
                            ///   <para>
                            ///     This behavior may (and often will) have dependencies which will be other
                            ///       object management strategies and will provide implementations of the
                            ///       same methods: some of them will cache the results of their calculations
                            ///       to provide their data to dependent strategies to perform their, in turn,
                            ///       own calculations as well.
                            ///   </para>
                            ///   <para>
                            ///     This component will be attached to the same object having as a component
                            ///       an instance of <see cref="ObjectsManagementStrategyHolder"/>, and one
                            ///       of these strategy components MUST be selected as the main strategy
                            ///       in the holder component.
                            ///   </para>
                            /// </summary>
                            public abstract class ObjectsManagementStrategy : MonoBehaviour
                            {
                                private static Type baseCounterpartStrategyType = typeof(Entities.Objects.Strategies.ObjectStrategy);

                                /// <summary>
                                ///   Tells when the specified (returned) compatible or counterpart
                                ///     type is not a subclass of <see cref="Entities.Objects.Strategies.ObjectStrategy"/>.
                                /// </summary>
                                public class UnsupportedTypeException : Types.Exception
                                {
                                    public UnsupportedTypeException() { }
                                    public UnsupportedTypeException(string message) : base(message) { }
                                    public UnsupportedTypeException(string message, Exception inner) : base(message, inner) { }
                                }

                                /// <summary>
                                ///   The related management strategy holder on this game object.
                                /// </summary>
                                public ObjectsManagementStrategyHolder StrategyHolder { get; private set; }

                                /// <summary>
                                ///   The compatible strategy type, as returned by <see cref="GetCounterpartType"/>.
                                /// </summary>
                                public Type CounterpartType { get; private set; }

                                public void Awake()
                                {
                                    StrategyHolder = GetComponent<ObjectsManagementStrategyHolder>();
                                    CounterpartType = GetCounterpartType();
                                    if (CounterpartType == null || !AlephVault.Unity.Support.Utils.Classes.IsSameOrSubclassOf(CounterpartType, baseCounterpartStrategyType))
                                    {
                                        Destroy(gameObject);
                                        throw new UnsupportedTypeException(string.Format("The type returned by CounterpartType must be a subclass of {0}", baseCounterpartStrategyType.FullName));
                                    }
                                }

                                /// <summary>
                                ///   This method MUST be implemented to tell the strategy which type is to
                                ///     be considered the compatible one. Such type must be a subclass of 
                                ///     <see cref="Entities.Objects.Strategies.ObjectStrategy"/>.
                                /// </summary>
                                /// <returns>The type identified as compatible to this trategy</returns>
                                protected abstract Type GetCounterpartType();

                                /// <summary>
                                ///   This method may be implemented to initialize custom global data in the
                                ///     strategy (e.g. a bidimensional array related to the information provided
                                ///     by the tiles).
                                /// </summary>
                                public virtual void InitGlobalCellsData()
                                {
                                }

                                /// <summary>
                                ///   This method may be overriden to initialize all the individual cells data on
                                ///     the strategy. The default implementation involves invoking the callback
                                ///     which iterates over all the cells, and passing as argument the instance
                                ///     method reference to <see cref="ComputeCellData(uint, uint)"/>.
                                /// </summary>
                                /// <param name="allCellsIterator">
                                ///   A function that takes a data initializator function for a particular cell,
                                ///     and runs it for each (x, y) pair on the map
                                /// </param>
                                /// <remarks>Seriously, you will seldom to never need to override this method.</remarks>
                                public virtual void InitIndividualCellsData(Action<Action<uint, uint>> allCellsIterator)
                                {
                                    allCellsIterator(ComputeCellData);
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy compute the data for a
                                ///     single cell. This will happen at initialization and at cell update. The default
                                ///     implementation does nothing.
                                /// </summary>
                                /// <param name="x">The X position of the cell(s) being taken for computation</param>
                                /// <param name="y">The Y position of the cell(s) being taken for computation</param>
                                /// <remarks>
                                ///   One should take into account the <see cref="ObjectsManagementStrategyHolder.Tilemaps"/>
                                ///     property to iterate them to know the cells and fetch their data most likely by invoking
                                ///     <see cref="ScriptableObjects.Tiles.BundledTile.GetStrategyFrom{T}(UnityEngine.Tilemaps.TileBase)"/>
                                ///     to get their associated data.
                                /// </remarks>
                                public virtual void ComputeCellData(uint x, uint y)
                                {
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     was just detached to the strategy (is our opportunity to initialize all the associated data).
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object just attached</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                public virtual void AttachedStrategy(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                {
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     is being detached from the strategy (it is not yet detached, and is our opportunity to clear
                                ///     all the associated data).
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object being detached</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                public virtual void DetachedStrategy(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                {
                                }

                                /// <summary>
                                ///   <para>
                                ///     This method MUST be implemented to tell whether an object can allocate a movement.
                                ///   </para>
                                ///   <para>
                                ///     Allocating a movement is an explicit action given from the user, and it must be implemented
                                ///       accordingly (this method is perhaps the most fundamental to implement since it is the
                                ///       main rule of movement).
                                ///   </para>
                                /// </summary>
                                /// <param name="otherComponentsResults">
                                ///   A dictionary holding the calculated value, for this method, in the dependencies. You can -and often
                                ///     WILL- also take those values into account for this calculation</param>
                                /// <param name="strategy">The compatible strategy of the object being checked</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <returns>Whether it can cancel the movement or not</returns>
                                public abstract bool CanAllocateMovement(Dictionary<Type, bool> otherComponentsResults, Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Types.Direction direction, bool continued);

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     allocates its movement. Switch conditions will likely be applied on the <paramref name="stage"/>
                                ///     parameter:
                                ///   <list type="bullet">
                                ///     <item>
                                ///       <term>Before</term>
                                ///       <description>Before allocating.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>AfterMovementAllocation</term>
                                ///       <description>After the new position movement was allocated in the status.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>After</term>
                                ///       <description>
                                ///         After the whole allocation (including <see cref="Entities.Objects.MapObject.onMovementStarted"/>
                                ///           event handlers).
                                ///       </description>
                                ///     </item>
                                ///   </list>
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object allocating the movement</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <param name="direction">The direction of this new movement</param>
                                /// <param name="continuated">Whether the movement being allocated is considered a continuation of a former movement (in the same direction, and immediate)</param>
                                /// <param name="stage">A string value: "Before", "AfterMovementAllocation", "After"</param>
                                public virtual void DoAllocateMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Types.Direction direction, bool continuated, string stage)
                                {
                                }

                                /// <summary>
                                ///   <para>
                                ///     This method MUST be implemented to tell whether an object can cancel a movement.
                                ///   </para>
                                ///   <para>
                                ///     Cancelling a movement is often an explicit action given from the user -although depending on
                                ///       the game it may not be the case- and although the regular case is to check whether the
                                ///       current movement is not null, there may be other cases like e.g. checking whether you are NOT
                                ///       on a slipperdy tile.
                                ///   </para>
                                /// </summary>
                                /// <param name="otherComponentsResults">
                                ///   A dictionary holding the calculated value, for this method, in the dependencies. You can -and often
                                ///     WILL- also take those values into account for this calculation</param>
                                /// <param name="strategy">The compatible strategy of the object being checked</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <returns>Whether it can cancel the movement or not</returns>
                                public abstract bool CanClearMovement(Dictionary<Type, bool> otherComponentsResults, Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status);

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     cancels its movement. Switch conditions will likely be applied on the <paramref name="stage"/>
                                ///     parameter:
                                ///   <list type="bullet">
                                ///     <item>
                                ///       <term>Before</term>
                                ///       <description>Before cancelling the movement.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>AfterMovementClear</term>
                                ///       <description>After the movement was cleared in object's status.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>After</term>
                                ///       <description>
                                ///         After the whole clear (including <see cref="Entities.Objects.MapObject.onMovementCancelled"/>
                                ///           event handlers).
                                ///       </description>
                                ///     </item>
                                ///   </list>
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object cancelling the movement</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <param name="formerMovement">The movement being cancelled</param>
                                /// <param name="stage">A string value: "Before", "AfterMovementClear", "After"</param>
                                /// <remarks>You will never make use of this method directly. Movement strategy will make use of this one.</remarks>
                                public virtual void DoClearMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Types.Direction? formerMovement, string stage)
                                {
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     finishes its movement. Switch conditions will likely be applied on the <paramref name="stage"/>
                                ///     parameter:
                                ///   <list type="bullet">
                                ///     <item>
                                ///       <term>Before</term>
                                ///       <description>Before finishing the movement.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>AfterPositionChange</term>
                                ///       <description>After the new position was set in the object's status.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>AfterMovementClear</term>
                                ///       <description>After the movement was cleared in object's status.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>After</term>
                                ///       <description>
                                ///         After the whole movement (including <see cref="Entities.Objects.MapObject.onMovementFinished"/>
                                ///           event handlers).
                                ///       </description>
                                ///     </item>
                                ///   </list>
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object finishing the movement</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <param name="formerMovement">The movement being finished</param>
                                /// <param name="stage">A string value: "Before", "AfterPositionChange", "AfterMovementClear", "After"</param>
                                /// <remarks>You will never make use of this method directly. Movement strategy will make use of this one.</remarks>
                                public virtual void DoConfirmMovement(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Types.Direction? formerMovement, string stage)
                                {
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when the underlying object
                                ///     is being teleported. Switch conditions will likely be applied on the <paramref name="stage"/>
                                ///     parameter:
                                ///   <list type="bullet">
                                ///     <item>
                                ///       <term>Before</term>
                                ///       <description>Before doing the teleportation.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>AfterPositionChange</term>
                                ///       <description>After the (x, y) were set in the object's status.</description>
                                ///     </item>
                                ///     <item>
                                ///       <term>After</term>
                                ///       <description>
                                ///         After the whole teleport (including <see cref="Entities.Objects.MapObject.onTeleported"/>
                                ///           event handlers).
                                ///       </description>
                                ///     </item>
                                ///   </list>
                                /// </summary>
                                /// <param name="strategy">The compatible strategy of the object being teleported</param>
                                /// <param name="status">The status (position and movement) of the underlying object</param>
                                /// <param name="x">The X position of the teleport command</param>
                                /// <param name="y">The Y position of the teleport command</param>
                                /// <param name="stage">A string value: "Before", "After", "AfterPositionChange"</param>
                                public virtual void DoTeleport(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, uint x, uint y, string stage)
                                {
                                }

                                /// <summary>
                                ///   This method may be implemented to tell how will the strategy react when
                                ///     one object strategy (of a compatible type) notifies the change of one of their
                                ///     properties.
                                /// </summary>
                                /// <param name="strategy">The (related) strateg notifying the change</param>
                                /// <param name="status">The inner status (position and movement) of the underlying object</param>
                                /// <param name="property">The property being changed</param>
                                /// <param name="oldValue">The old value</param>
                                /// <param name="newValue">The new value</param>
                                public virtual void DoProcessPropertyUpdate(Entities.Objects.Strategies.ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, string property, object oldValue, object newValue)
                                {
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
