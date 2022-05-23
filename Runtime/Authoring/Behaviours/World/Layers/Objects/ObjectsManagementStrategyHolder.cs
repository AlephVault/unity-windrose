using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                        using ObjectsManagementStrategies;
                        using AlephVault.Unity.Layout.Utils;

                        /// <summary>
                        ///   <para>
                        ///     Object management strategy holders refer the strategy that will
                        ///       provide the rules of movement of this map. 
                        ///   </para>
                        ///   <para>
                        ///     While this component is tied to a map, the same game object must
                        ///       have at least one <see cref="ObjectsManagementStrategy"/>
                        ///       component attached so it may be selected as primary strategy
                        ///       in this component: it will determine the said rules of
                        ///       movement.
                        ///   </para>
                        /// </summary>
                        [RequireComponent(typeof(ObjectsLayer))]
                        public class ObjectsManagementStrategyHolder : MonoBehaviour
                        {
                            /// <summary>
                            ///   Tells when the object's dimensions are unsuitable for this map.
                            /// </summary>
                            public class InvalidDimensionsException : Types.Exception
                            {
                                public readonly uint Width;
                                public readonly uint Height;
                                public InvalidDimensionsException(uint width, uint height) { Width = width; Height = height; }
                                public InvalidDimensionsException(string message, uint width, uint height) : base(message) { Width = width; Height = height; }
                                public InvalidDimensionsException(string message, uint width, uint height, System.Exception inner) : base(message, inner) { Width = width; Height = height; }
                            }

                            /// <summary>
                            ///   Tells when the object's position is unsuitable for this map.
                            /// </summary>
                            public class InvalidPositionException : Types.Exception
                            {
                                public readonly uint X;
                                public readonly uint Y;
                                public InvalidPositionException(uint x, uint y) { X = x; Y = y; }
                                public InvalidPositionException(string message, uint x, uint y) : base(message) { X = x; Y = y; }
                                public InvalidPositionException(string message, uint x, uint y, System.Exception inner) : base(message, inner) { X = x; Y = y; }
                            }

                            /// <summary>
                            ///   Tells when the main strategy is not among the present strategies
                            ///     in the underlying game object.
                            /// </summary>
                            public class InvalidStrategyComponentException : Types.Exception
                            {
                                public InvalidStrategyComponentException() { }
                                public InvalidStrategyComponentException(string message) : base(message) { }
                                public InvalidStrategyComponentException(string message, Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   Tells when an object (intended to be added to this map)
                            ///     lacks of the required strategy.
                            /// </summary>
                            public class ObjectLacksOfCompatibleStrategy : Types.Exception
                            {
                                public ObjectLacksOfCompatibleStrategy(string message) : base(message) { }
                                public ObjectLacksOfCompatibleStrategy(string message, System.Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   Tells when this game object has more than one strategy
                            ///     of each type. This one should be deprecated, and
                            ///     strategies should also make use of
                            ///     <see cref="DisallowMultipleComponent"/>.
                            /// </summary>
                            public class DuplicatedComponentException : Types.Exception
                            {
                                public DuplicatedComponentException(string message) : base(message) { }
                                public DuplicatedComponentException(string message, System.Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   Tells when an object that is already attached is trying to
                            ///     be attached... again.
                            /// </summary>
                            public class AlreadyAttachedException : Types.Exception
                            {
                                public AlreadyAttachedException(string message) : base(message) { }
                                public AlreadyAttachedException(string message, System.Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   Tells when the map tries to interact with an object that
                            ///     is not attached to it.
                            /// </summary>
                            public class NotAttachedException : Types.Exception
                            {
                                public NotAttachedException(string message) : base(message) { }
                                public NotAttachedException(string message, System.Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   Tells when the object trying to be attached has a main
                            ///     object strategy not compatible with the map's main
                            ///     strategy.
                            /// </summary>
                            public class StrategyNotAllowedException : Types.Exception
                            {
                                public StrategyNotAllowedException(string message) : base(message) { }
                                public StrategyNotAllowedException(string message, System.Exception inner) : base(message, inner) { }
                            }

                            /// <summary>
                            ///   The map this manager... manages. Also, the map is used by the
                            ///     strategies to fetch per-cell information.
                            /// </summary>
                            public Map Map { get; private set; }

                            /// <summary>
                            ///   The root strategy. You will typically add just one strategy to
                            ///     this game object, and it may also add other strategies as well
                            ///     (that are registered via <see cref="RequireComponent"/>), and
                            ///     you will select such strategy as the value of this property.
                            ///   The strategy in this property will be the actual ruler of the
                            ///     movement of objects inside this map.
                            /// </summary>
                            [SerializeField]
                            private ObjectsManagementStrategy strategy;

                            /// <summary>
                            ///   See <see cref="strategy"/>.
                            /// </summary>
                            public ObjectsManagementStrategy Strategy { get { return strategy; } }

                            /// <summary>
                            ///   If set to true, the strategy check is bypassed. This means that
                            ///     checks will always pass while Bypass is true, but also means
                            ///     that related events (e.g. movement started, stopped, cancelled)
                            ///     will also trigger. The logic behind the strategy, if any, must
                            ///     keep the inner state consistent (like solidness strategy does
                            ///     by accumulating indices / positions).
                            /// </summary>
                            public bool Bypass = false;

                            /**
                             * This is the list of tilemaps from the map.
                             */
                            private UnityEngine.Tilemaps.Tilemap[] fetchedTilemaps;

                            /**
                             * This is the list of sorted strategy componentes here.
                             */
                            private ObjectsManagementStrategy[] sortedStrategies;

                            /**
                             * On initialization, the strategy will fetch its map to, actually, know it.
                             * Also it will fetch the active tilemaps, and build its strategy.
                             */
                            private void Awake()
                            {
                                if (strategy == null || !(new HashSet<ObjectsManagementStrategy>(GetComponents<ObjectsManagementStrategy>()).Contains(strategy)))
                                {
                                    Destroy(gameObject);
                                    throw new InvalidStrategyComponentException("The selected strategy component must be non-null and present among the current map's components");
                                }
                                // We enumerate all the strategies attached. We will iterate their calls and cache their results, if any.
                                sortedStrategies = (from component in Behaviours.SortByDependencies(GetComponents<ObjectsManagementStrategy>()) select (component as ObjectsManagementStrategy)).ToArray();

                                // We cannot allow a strategy type being added (depended) twice.
                                if (sortedStrategies.Length != new HashSet<Type>(from sortedStrategy in sortedStrategies select sortedStrategy.GetType()).Count)
                                {
                                    Destroy(gameObject);
                                    throw new DuplicatedComponentException("Cannot add more than one strategy instance per strategy type to an objects managemnt strategy holder");
                                }
                                
                                // We require the map also here.
                                Map = Behaviours.RequireComponentInParent<Map>(gameObject);
                            }

                            /**
                             * Iterates and collects the same boolean call to each strategy into a dictionary. Returns the
                             *   value according to the main strategy.
                             */
                            private bool Collect(Func<Dictionary<ObjectsManagementStrategy, bool>, ObjectsManagementStrategy, bool> predicate)
                            {
                                Dictionary<ObjectsManagementStrategy, bool> collected = new Dictionary<ObjectsManagementStrategy, bool>();
                                foreach (ObjectsManagementStrategy subStrategy in sortedStrategies)
                                {
                                    collected[subStrategy] = predicate(collected, subStrategy);
                                }
                                return collected[Strategy];
                            }

                            /**
                             * Iterates on each strategy and calls a function.
                             */
                            private void Traverse(Action<ObjectsManagementStrategy> traverser)
                            {
                                foreach (ObjectsManagementStrategy subStrategy in sortedStrategies)
                                {
                                    traverser(subStrategy);
                                }
                            }

                            /**
                             * Given a particular strategy component, obtain the appropriate objectStrategy component from a main object
                             *   strategy.
                             */
                            private Entities.Objects.Strategies.ObjectStrategy GetCompatible(Entities.Objects.Strategies.ObjectStrategy target, ObjectsManagementStrategy source)
                            {
                                return target.GetComponent(source.CounterpartType) as Entities.Objects.Strategies.ObjectStrategy;
                            }

                            /**
                             * Gets the main strategy of the target holder according to our main strategy.
                             */
                            private Entities.Objects.Strategies.ObjectStrategy GetMainCompatible(Entities.Objects.ObjectStrategyHolder target)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = target.GetComponent(strategy.CounterpartType) as Entities.Objects.Strategies.ObjectStrategy;
                                if (objectStrategy == null)
                                {
                                    throw new ObjectLacksOfCompatibleStrategy("Related object strategy holder component lacks of compatible strategy component for the current map strategy");
                                }
                                return objectStrategy;
                            }

                            /*************************************************************************************************
                             * 
                             * Initializing the strategy.
                             * 
                             *************************************************************************************************/

                            /// <summary>
                            ///    This method is not needed for the end user. The map invokes
                            ///      this method to initialize the appropriate data on the
                            ///      strategy (e.g. layout and global data).
                            /// </summary>
                            public void Initialize()
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.InitGlobalCellsData();
                                    strategy.InitIndividualCellsData(delegate (Action<uint, uint> callback)
                                    {
                                        for (uint y = 0; y < Map.Height; y++)
                                        {
                                            for (uint x = 0; x < Map.Width; x++)
                                            {
                                                callback(x, y);
                                            }
                                        }
                                    });
                                });
                            }

                            /**
                             * Method to initialize the tilemaps.
                             */
                            private void PrepareTilemaps()
                            {
                                if (fetchedTilemaps == null)
                                {
                                    fetchedTilemaps = Tilemaps.ToArray();
                                }
                            }

                            /// <summary>
                            ///   Returns the map's tilemaps.
                            /// </summary>
                            /// <seealso cref="Layers.Floor.FloorLayer.Tilemaps"/>
                            public IEnumerable<UnityEngine.Tilemaps.Tilemap> Tilemaps
                            {
                                get
                                {
                                    return Map.FloorLayer.Tilemaps;
                                }
                            }

                            /// <summary>
                            ///   Gets a tile in one of the tilemaps.
                            /// </summary>
                            /// <param name="tilemap">The tilemap index, in the order they were added (in the editor, or manually)</param>
                            /// <param name="x">The x position inside the tilemap</param>
                            /// <param name="y">The y position inside the tilemap</param>
                            /// <returns>The tile in that position</returns>
                            public UnityEngine.Tilemaps.TileBase GetTile(int tilemap, int x, int y)
                            {
                                PrepareTilemaps();
                                return fetchedTilemaps[tilemap].GetTile(new Vector3Int(x, y, 0));
                            }

                            /// <summary>
                            ///   Sets a tile in one of the tilemaps (in a particular position).
                            ///     It also causes a strategy recomputation.
                            /// </summary>
                            /// <param name="tilemap">The tilemap index, in the order they were added (in the editor, or manually)</param>
                            /// <param name="x">The x position inside the tilemap</param>
                            /// <param name="y">The y position inside the tilemap</param>
                            /// <param name="tile">The tile to set</param>
                            public void SetTile(int tilemap, ushort x, ushort y, UnityEngine.Tilemaps.TileBase tile)
                            {
                                PrepareTilemaps();
                                fetchedTilemaps[tilemap].SetTile(new Vector3Int((int)x, (int)y, 0), tile);
                                Strategy.ComputeCellData(x, y);
                            }

                            /*************************************************************************************************
                             * 
                             * Attaching an object (strategy).
                             * 
                             *************************************************************************************************/

                            /// <summary>
                            ///   The status of an object inside the map.
                            ///   This involves position and current movement.
                            /// </summary>
                            public class Status
                            {
                                /// <summary>
                                ///   The current direction the object is movement to
                                ///     (or null if it is not moving).
                                /// </summary>
                                public Types.Direction? Movement;
                                /// <summary>
                                ///   The object's current X position.
                                /// </summary>
                                public ushort X;
                                /// <summary>
                                ///   The object's current Y position.
                                /// </summary>
                                public ushort Y;

                                public Status(ushort x, ushort y, Types.Direction? movement = null)
                                {
                                    X = x;
                                    Y = y;
                                    Movement = movement;
                                }

                                /// <summary>
                                ///   Creates a clone of this object.
                                /// </summary>
                                /// <returns>The cloned object</returns>
                                public Status Copy()
                                {
                                    return new Status(X, Y, Movement);
                                }
                            }

                            private Dictionary<Entities.Objects.Strategies.ObjectStrategy, Status> attachedStrategies = new Dictionary<Entities.Objects.Strategies.ObjectStrategy, Status>();

                            private void RequireAttached(Entities.Objects.Strategies.ObjectStrategy strategy)
                            {
                                if (strategy == null)
                                {
                                    throw new ArgumentNullException("Cannot attach a null object strategy to a map");
                                }

                                if (!attachedStrategies.ContainsKey(strategy))
                                {
                                    throw new NotAttachedException("This strategy is not attached to the map");
                                }
                            }

                            private void RequireNotAttached(Entities.Objects.Strategies.ObjectStrategy strategy)
                            {
                                if (attachedStrategies.ContainsKey(strategy))
                                {
                                    throw new AlreadyAttachedException("This strategy is already attached to the map");
                                }
                            }

                            /// <summary>
                            ///   Gets the <see cref="Status"/> for a given object's strategy holder.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The given strategy holder</param>
                            public Status StatusFor(Entities.Objects.ObjectStrategyHolder objectStrategyHolder)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);
                                if (attachedStrategies.ContainsKey(objectStrategy))
                                {
                                    return attachedStrategies[objectStrategy];
                                }
                                else
                                {
                                    throw new NotAttachedException("The object is not attached to this map");
                                }
                            }

                            /// <summary>
                            ///   Attaches the object strategy to the current map strategy.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] to attach</param>
                            /// <param name="x">The X position to attach the object to</param>
                            /// <param name="y">The Y position to attach the object to</param>
                            /// <remarks>
                            ///   The object must have a compatible main strategy, and valid dimensions
                            ///   to fit in the given position. It is also an error to try to attach an
                            ///   object that is already attached.
                            /// </remarks>
                            public void Attach(Entities.Objects.ObjectStrategyHolder objectStrategyHolder, ushort x, ushort y)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                // Require it not attached
                                RequireNotAttached(objectStrategy);

                                // Do we accept or reject the strategy being attached? (no per-strategy-component call is needed here)
                                if (!AlephVault.Unity.Support.Utils.Classes.IsSameOrSubclassOf(objectStrategy.GetType(), Strategy.CounterpartType))
                                {
                                    throw new StrategyNotAllowedException("This strategy is not allowed on this map because is not a valid counterpart of the current map strategy.");
                                }

                                // Do we accept or reject the strategy being attached? (with a custom logic per strategy component)
                                if (!CanAttachStrategy(objectStrategy, out string reason))
                                {
                                    throw new StrategyNotAllowedException(
                                        "This strategy is not allowed on this map due to an " +
                                        "attachment rejection: " + reason
                                    );
                                }

                                // Does it fit regarding bounds?
                                if (x > Map.Width - objectStrategyHolder.Object.Width || y > Map.Height - objectStrategyHolder.Object.Height)
                                {
                                    throw new InvalidPositionException("Object coordinates and dimensions are not valid inside intended map's dimensions", x, y);
                                }

                                // Store its position
                                Status status = new Status(x, y);
                                attachedStrategies[objectStrategy] = status;

                                // Notify the map strategy, so data may be updated
                                AttachedStrategy(objectStrategy, status);

                                // Finally, notify the client strategy.
                                objectStrategy.Object.onAttached.Invoke(Map);
                            }
                            
                            /**
                             * Iterates all the strategies to tell whether it can be attached or not.
                             */
                            private bool CanAttachStrategy(
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy,
                                out string reason)
                            {
                                reason = "";
                                if (Bypass) return true;

                                string underlyingReason = "";
                                if (!Collect(delegate(Dictionary<ObjectsManagementStrategy, bool> collected,
                                    ObjectsManagementStrategy strategy)
                                {
                                    if (!strategy.CanAttachStrategy(
                                        collected,
                                        GetCompatible(objectStrategy, strategy),
                                        ref underlyingReason)
                                    )
                                    {
                                        return false;
                                    }
                                    return true;
                                }))
                                {
                                    reason = underlyingReason;
                                    return false;
                                }

                                return true;
                            }

                            /**
                             * Iterates over each strategy and calls its AttachedStrategy appropriately
                             *   (from less to more dependent strategies).
                             */
                            private void AttachedStrategy(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.AttachedStrategy(GetCompatible(objectStrategy, strategy), status);
                                });
                            }

                            /// <summary>
                            ///   Detaches the object strategy from the current map strategy.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] to detach</param>
                            /// <remarks>It is an error to detach an object that is not attached. Also, the object must have a compatible strategy.</remarks>
                            public void Detach(Entities.Objects.ObjectStrategyHolder objectStrategyHolder)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                // Require it attached to the map
                                RequireAttached(objectStrategy);
                                Status status = attachedStrategies[objectStrategy];

                                // Cancels the movement, if any
                                ClearMovement(objectStrategy, status);

                                // Notify the map strategy, so data may be cleaned
                                DetachedStrategy(objectStrategy, status);

                                // Clear its position
                                attachedStrategies.Remove(objectStrategy);

                                // Finally, notify the client strategy.
                                objectStrategy.Object.onDetached.Invoke();
                            }

                            /**
                             * Iterates over each strategy and calls its DetachedStrategy appropriately
                             *   (from less to more dependent strategies).
                             */
                            private void DetachedStrategy(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.DetachedStrategy(GetCompatible(objectStrategy, strategy), status);
                                });
                            }

                            /// <summary>
                            ///   Invokes the start of a movement for a given object.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object's strategy holder to move</param>
                            /// <param name="direction">The direction to move to</param>
                            /// <param name="continuated">If this movement should be considered a continuation of a previous movement</param>
                            /// <returns>Whether the movement could be started</returns>
                            /// <remarks>It is an error to detach an object that is not attached. Also, the object must have a compatible strategy.</remarks>
                            public bool MovementStart(Entities.Objects.ObjectStrategyHolder objectStrategyHolder, Types.Direction direction, bool continuated = false)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                // Require it attached to the map
                                RequireAttached(objectStrategy);

                                Status status = attachedStrategies[objectStrategy];

                                return AllocateMovement(objectStrategy, status, direction, continuated);
                            }

                            /**
                             * Executes the actual movement allocation.
                             */
                            private bool AllocateMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, Types.Direction direction, bool continuated = false)
                            {
                                if (CanAllocateMovement(objectStrategy, status, direction, continuated))
                                {
                                    DoAllocateMovement(objectStrategy, status, direction, continuated, "Before");
                                    status.Movement = direction;
                                    DoAllocateMovement(objectStrategy, status, direction, continuated, "AfterMovementAllocation");
                                    objectStrategy.Object.onMovementStarted.Invoke(direction);
                                    DoAllocateMovement(objectStrategy, status, direction, continuated, "After");
                                    return true;
                                }
                                else
                                {
                                    objectStrategy.Object.onMovementRejected.Invoke(direction);
                                    return false;
                                }
                            }

                            /**
                             * Iterates all the strategies to tell whether it can allocate the movement or not.
                             */
                            private bool CanAllocateMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, Types.Direction direction, bool continuated = false)
                            {
                                if (status.Movement != null) return false;

                                Entities.Objects.MapObject mapObject = objectStrategy.StrategyHolder.Object;

                                switch (direction)
                                {
                                    case Types.Direction.LEFT:
                                        if (status.X == 0) return false;
                                        break;
                                    case Types.Direction.UP:
                                        if (status.Y + mapObject.Height >= strategy.StrategyHolder.Map.Height) return false;
                                        break;
                                    case Types.Direction.RIGHT:
                                        if (status.X + mapObject.Width >= strategy.StrategyHolder.Map.Width) return false;
                                        break;
                                    case Types.Direction.DOWN:
                                        if (status.Y == 0) return false;
                                        break;
                                }

                                if (Bypass) return true;

                                return Collect(delegate (Dictionary<ObjectsManagementStrategy, bool> collected, ObjectsManagementStrategy strategy)
                                {
                                    return strategy.CanAllocateMovement(collected, GetCompatible(objectStrategy, strategy), status, direction, continuated);
                                });
                            }

                            /**
                             * Iterates all the strategies for the different stages of movement allocation.
                             */
                            private void DoAllocateMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, Types.Direction direction, bool continuated, string stage)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.DoAllocateMovement(GetCompatible(objectStrategy, strategy), status, direction, continuated, stage);
                                });
                            }

                            /*************************************************************************************************
                             * 
                             * Cancelling the movement of an object (strategy).
                             * 
                             *************************************************************************************************/

                            /// <summary>
                            ///   Cancels the movement of the current object.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] to which cancel the current movement</param>
                            /// <returns>Whether the current movement could be cancelled</returns>
                            /// <remarks>It is an error to detach an object that is not attached. Also, the object must have a compatible strategy.</remarks>
                            public bool MovementCancel(Entities.Objects.ObjectStrategyHolder objectStrategyHolder)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                // Require it attached to the map
                                RequireAttached(objectStrategy);

                                return ClearMovement(objectStrategy, attachedStrategies[objectStrategy]);
                            }

                            /**
                             * Executes the actual movement clearing.
                             */
                            private bool ClearMovement(Entities.Objects.Strategies.ObjectStrategy strategy, Status status)
                            {
                                if (CanClearMovement(strategy, status))
                                {
                                    Types.Direction? formerMovement = status.Movement;
                                    DoClearMovement(strategy, status, formerMovement, "Before");
                                    status.Movement = null;
                                    DoClearMovement(strategy, status, formerMovement, "AfterMovementClear");
                                    strategy.Object.onMovementCancelled.Invoke(formerMovement);
                                    DoClearMovement(strategy, status, formerMovement, "Before");
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            /**
                             * Iterates all the strategies to tell whether it can clear the movement or not.
                             */
                            private bool CanClearMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status)
                            {
                                if (Bypass) return true;

                                return Collect(delegate (Dictionary<ObjectsManagementStrategy, bool> collected, ObjectsManagementStrategy strategy)
                                {
                                    return strategy.CanClearMovement(collected, GetCompatible(objectStrategy, strategy), status);
                                });
                            }

                            /**
                             * Iterates all the strategies for the different stages of movement clearing.
                             */
                            private void DoClearMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, Types.Direction? formerMovement, string stage)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.DoClearMovement(GetCompatible(objectStrategy, strategy), status, formerMovement, stage);
                                });
                            }

                            /*************************************************************************************************
                             * 
                             * Finishing the movement of an object (strategy), if any.
                             * 
                             *************************************************************************************************/

                            /// <summary>
                            ///   Finishes the current movement of the object.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] to which finish the movement</param>
                            /// <returns>Whether the current movement could be finished</returns>
                            /// <remarks>It is an error to detach an object that is not attached. Also, the object must have a compatible strategy.</remarks>
                            public bool MovementFinish(Entities.Objects.ObjectStrategyHolder objectStrategyHolder)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                // Require it attached to the map
                                RequireAttached(objectStrategy);

                                Status status = attachedStrategies[objectStrategy];

                                if (status.Movement != null)
                                {
                                    Types.Direction formerMovement = status.Movement.Value;
                                    Strategy.DoConfirmMovement(objectStrategy, status, formerMovement, "Before");
                                    switch (formerMovement)
                                    {
                                        case Types.Direction.UP:
                                            status.Y++;
                                            break;
                                        case Types.Direction.DOWN:
                                            status.Y--;
                                            break;
                                        case Types.Direction.LEFT:
                                            status.X--;
                                            break;
                                        case Types.Direction.RIGHT:
                                            status.X++;
                                            break;
                                    }
                                    DoConfirmMovement(objectStrategy, status, formerMovement, "AfterPositionChange");
                                    status.Movement = null;
                                    DoConfirmMovement(objectStrategy, status, formerMovement, "AfterMovementClear");
                                    objectStrategy.Object.onMovementFinished.Invoke(formerMovement);
                                    DoConfirmMovement(objectStrategy, status, formerMovement, "After");
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            /**
                             * Iterates all the strategies for the different stages of movement allocation.
                             */
                            private void DoConfirmMovement(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, Types.Direction? formerMovement, string stage)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.DoConfirmMovement(GetCompatible(objectStrategy, strategy), status, formerMovement, stage);
                                });
                            }

                            /*************************************************************************************************
                             * 
                             * Teleports the object strategy to another position in the map
                             * 
                             *************************************************************************************************/


                            /// <summary>
                            ///   Teleports the object to another (X, Y) position.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] to teleport</param>
                            /// <param name="x">The X position to teleport the object to</param>
                            /// <param name="y">The Y position to teleport the object to</param>
                            /// <param name="silent">If true, this  teleportation will not trigger the <see cref="onTeleported"/> event on the object</param>
                            /// <remarks>It is an error to detach an object that is not attached. Also, the object must have a compatible strategy.</remarks>
                            public void Teleport(Entities.Objects.ObjectStrategyHolder objectStrategyHolder, ushort x, ushort y, bool silent = false)
                            {
                                Entities.Objects.Strategies.ObjectStrategy objectStrategy = GetMainCompatible(objectStrategyHolder);

                                RequireAttached(objectStrategy);

                                Status status = attachedStrategies[objectStrategy];

                                if (status.X > Map.Width - objectStrategyHolder.Object.Width || y > Map.Height - objectStrategyHolder.Object.Height)
                                {
                                    throw new InvalidPositionException("New object coordinates and dimensions are not valid inside intended map's dimensions", status.X, status.Y);
                                }

                                ClearMovement(objectStrategy, status);
                                DoTeleport(objectStrategy, status, x, y, "Before");
                                status.X = x;
                                status.Y = y;
                                DoTeleport(objectStrategy, status, x, y, "AfterPositionChange");
                                if (!silent) objectStrategy.Object.onTeleported.Invoke(x, y);
                                DoTeleport(objectStrategy, status, x, y, "After");
                            }

                            /**
                             * Iterates all the strategies for the different stages of teleportation.
                             */
                            private void DoTeleport(Entities.Objects.Strategies.ObjectStrategy objectStrategy, Status status, uint x, uint y, string stage)
                            {
                                Traverse(delegate (ObjectsManagementStrategy strategy)
                                {
                                    strategy.DoTeleport(GetCompatible(objectStrategy, strategy), status, x, y, stage);
                                });
                            }

                            /*************************************************************************************************
                             * 
                             * Updates according to particular data change. These fields exist in the strategy. This method
                             *   will get the holder, the strategy being updated (which belongs to the holder), and the
                             *   property with the old/new values.
                             * 
                             * You will never call this method directly.
                             * 
                             * The strategy processing this data change will be picked according to the counterpart setting.
                             * It does not, and will not (most times in combined strategies) match the current map strategy
                             *   but instead map a strategy component in this same object.
                             * 
                             *************************************************************************************************/

                            /// <summary>
                            ///   This method is invoked by an <see cref="Entities.Objects.ObjectStrategyHolder"/>. Object strategy
                            ///     holders will call this method when one of their properties was updated in a meaningful
                            ///     way to the extent that this management strategy holder must be aware of that change.
                            /// </summary>
                            /// <param name="objectStrategyHolder">The object['s strategy holder] having a property that changed</param>
                            /// <param name="objectStrategy">The particular strategy having such property</param>
                            /// <param name="property">The property that changed</param>
                            /// <param name="oldValue">The old value</param>
                            /// <param name="newValue">The new value</param>
                            public void PropertyWasUpdated(Entities.Objects.ObjectStrategyHolder objectStrategyHolder, Entities.Objects.Strategies.ObjectStrategy objectStrategy, string property, object oldValue, object newValue)
                            {
                                Entities.Objects.Strategies.ObjectStrategy mainObjectStrategy = GetMainCompatible(objectStrategyHolder);

                                RequireAttached(mainObjectStrategy);

                                (GetComponent(objectStrategy.CounterpartType) as ObjectsManagementStrategy).DoProcessPropertyUpdate(objectStrategy, attachedStrategies[mainObjectStrategy], property, oldValue, newValue);

                                mainObjectStrategy.Object.onStrategyPropertyUpdated.Invoke(objectStrategy, property, oldValue, newValue);
                            }
                        }

#if UNITY_EDITOR
                        [CustomEditor(typeof(ObjectsManagementStrategyHolder))]
                        [CanEditMultipleObjects]
                        public class ObjectsManagementStrategyHolderEditor : Editor
                        {
                            SerializedProperty strategy;

                            protected virtual void OnEnable()
                            {
                                strategy = serializedObject.FindProperty("strategy");
                            }

                            public override void OnInspectorGUI()
                            {
                                serializedObject.Update();

                                ObjectsManagementStrategyHolder underlyingObject = (serializedObject.targetObject as ObjectsManagementStrategyHolder);
                                ObjectsManagementStrategy[] strategies = underlyingObject.GetComponents<ObjectsManagementStrategy>();
                                GUIContent[] strategyNames = (from strategy in strategies select new GUIContent(strategy.GetType().Name)).ToArray();

                                int index = ArrayUtility.IndexOf(strategies, strategy.objectReferenceValue as ObjectsManagementStrategy);
                                index = EditorGUILayout.Popup(new GUIContent("Main Strategy"), index, strategyNames);
                                strategy.objectReferenceValue = index >= 0 ? strategies[index] : null;

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
#endif
                    }
                }
            }
        }
    }
}
