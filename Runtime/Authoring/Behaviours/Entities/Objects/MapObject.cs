using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AlephVault.Unity.Support.Utils;
using GameMeanMachine.Unity.WindRose.Vendor.RBush;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                using Types;
                using World;
                using World.Layers.Objects;
                using Entities.Common;
                using AlephVault.Unity.Layout.Utils;

                /// <summary>
                ///   <para>
                ///     Aside of the map itself, map objects are the spirit of the party.
                ///   </para>
                ///   <para>
                ///     Map objects are the middle step between the user interface (or
                ///       artificial intelligence) and the underlying map and object
                ///       strategies: They will provide the behaviour to teleport,
                ///       attach to -and detach from- maps, look in different directions,
                ///       and a mean to notify when a property was changed.
                ///   </para>
                ///   <para>
                ///     Aside from that, map objects know how to perform actual movement,
                ///       when it finishes, how to notify underlying strategies, and also
                ///       when and if the object is looking somewhere (i.e. changing its
                ///       orientation), which may differ of the object's movement (e.g.
                ///       side-movement).
                ///   </para>
                /// </summary>
                [RequireComponent(typeof(Pausable))]
                public class MapObject : MonoBehaviour, Pausable.IPausable, ISpatialData
                {
                    #region Lifecycle

                    // Whether it is initialized.
                    private bool initialized = false;

                    // The current ongoing movement count, involving
                    // number of movement starts, ends, cancels, and
                    // also attachments and detachments.
                    private uint movementCount = 0;
                    
                    // Whether it is told to be destroyed or not.
                    private bool destroyed = false;
                    
                    private void Awake()
                    {
                        // Cleans the initial value of mainVisual
                        if (!new HashSet<Visuals.Visual>(GetChildVisuals()).Contains(mainVisual))
                        {
                            mainVisual = null;
                        }

                        StrategyHolder = GetComponent<ObjectStrategyHolder>();
                        onAttached.AddListener(delegate (Map newParentMap)
                        {
                            /*
                             * Attaching to a map involves:
                             * 1. The actual "parent" of the object will be a child of the RelatedMap being an ObjectsLayer.
                             * 2. We set the parent transform of the object to such ObjectsLayer's transform.
                             * 3. Finally we must ensure the transform.localPosition be updated accordingly (i.e. forcing a snap).
                             */
                            IncrementMovementCounter();
                            transform.localRotation = Quaternion.identity;
                            parentMap = newParentMap;
                            ObjectsLayer ObjectsLayer = parentMap.ObjectsLayer;
                            transform.SetParent(ObjectsLayer.transform);
                            transform.localPosition = new Vector3(
                                X * ObjectsLayer.GetCellWidth(),
                                Y * ObjectsLayer.GetCellHeight(),
                                0
                            );
                            origin = transform.localPosition;
                            Snap();
                        });
                        onTeleported.AddListener(delegate (ushort x, ushort y)
                        {
                            IncrementMovementCounter();
                            Snap();
                        });
                        onMovementStarted.AddListener((d) =>
                        {
                            IncrementMovementCounter();
                        });
                        onMovementCancelled.AddListener(delegate (Direction? formerMovement)
                        {
                            queueRemainingDuration = 0;
                            IncrementMovementCounter();
                            Snap();
                        });
                        onMovementFinished.AddListener((d) =>
                        {
                            queueRemainingDuration = 0;
                            IncrementMovementCounter();
                        });
                        onDetached.AddListener(delegate ()
                        {
                            IncrementMovementCounter();
                            if (parentMap == null || parentMap.transform == null || parentMap.transform.parent == null)
                            {
                                if (!destroyed) transform.SetParent(null);
                            }
                            else
                            {
                                Scope scope = parentMap.transform.parent.GetComponentInParent<Scope>();
                                if (scope == null)
                                {
                                    if (!destroyed) transform.SetParent(null);
                                }
                                else
                                {
                                    if (!destroyed) transform.SetParent(scope.transform);
                                }
                            }
                            parentMap = null;                            
                        });
                    }

                    private void Start()
                    {
                        Initialize();
                        // THEN instantiate all the overlays.
                        if (Application.isPlaying)
                        {
                            InitVisuals();
                            OnStateChanged.Invoke(currentState);
                        }
                    }

                    private void Update()
                    {
                        MovementTick();
                        foreach (Visuals.Visual visual in visuals) visual.DoUpdate();
                    }

                    private void OnDestroy()
                    {
                        destroyed = true;
                        Detach();
                        onAttached.RemoveAllListeners();
                        onDetached.RemoveAllListeners();
                        onMovementStarted.RemoveAllListeners();
                        onMovementCancelled.RemoveAllListeners();
                        onMovementFinished.RemoveAllListeners();
                        onStrategyPropertyUpdated.RemoveAllListeners();
                        onTeleported.RemoveAllListeners();
                    }

                    // Increments the internal movement counter.
                    private void IncrementMovementCounter()
                    {
                        if (movementCount == uint.MaxValue)
                        {
                            movementCount = 0;
                        }
                        else
                        {
                            movementCount++;
                        }
                    }

                    /// <summary>
                    ///   <para>
                    ///     This method is called when the map is initialized (first) and when this
                    ///       object starts its execution in the scene. Both conditions have to be
                    ///       fulfilled for the logic to initialize. When calling this function
                    ///       externally, guarantee that the initialization is done in the appropriate
                    ///       order for this to work.
                    ///   </para>
                    ///   <para>
                    ///     For this method to succeed, this object must be a child object of one
                    ///       holding a <see cref="ObjectsLayer"/> which in turn must be inside a
                    ///       <see cref="Map"/>, and the map must have dimensions that allow this
                    ///       object considering its size and initial position. If it is not attached
                    ///       to a map, it will work anyway (but the object will not be able to walk
                    ///       or perform useful interactions).
                    ///   </para>
                    /// </summary>
                    public void Initialize()
                    {
                        if (!Application.isPlaying) return;

                        if (initialized)
                        {
                            return;
                        }

                        // We will make use of strategy
                        if (StrategyHolder == null)
                        {
                            throw new Exception("An object strategy holder is required when the map object initializes.");
                        }
                        else
                        {
                            StrategyHolder.Initialize();
                        }

                        try
                        {
                            ObjectsLayer parentLayer;
                            // We find the parent map like this: (current) -> ObjectsLayer -> map
                            if (transform.parent != null && transform.parent.parent != null)
                            {
                                parentLayer = transform.parent.GetComponent<ObjectsLayer>();
                            }
                            else
                            {
                                parentLayer = null;
                            }
                            // It is OK to have no map! However, the object will be detached and
                            //   almost nothing useful will be able to be done to the object until
                            //   it is attached.
                            if (parentLayer != null)
                            {
                                // Here we are with an object that was instantiated inside a map's
                                //   hierarchy. We will not proceed and mark as initialized if
                                //   the underlying map is not initialized beforehand: otherwise
                                //   we would not necessarily know the appropriate dimensions.
                                if (!parentLayer.Initialized) return;
                                // And we also keep its objects layer
                                Behaviours.RequireComponentInParent<ObjectsLayer>(gameObject);
                                // Then we calculate the cell position from the grid in the layer.
                                Grid grid = Behaviours.RequireComponentInParent<Grid>(gameObject);
                                Vector3Int cellPosition = grid.WorldToCell(transform.position);
                                // Then we initialize, and perhaps it may explode due to exception.
                                Attach(parentLayer.Map, (ushort)cellPosition.x, (ushort)cellPosition.y, true);
                            }
                            // After success of a standalone map object being initialized, either
                            //   by itself or by the parent map invoking the initialization.
                            initialized = true;
                        }
                        catch (Behaviours.MissingComponentInParentException)
                        {
                            // nothing - diaper
                        }
                    }
                    #endregion

                    #region Miscelaneous
                    /// <summary>
                    ///   This event class notifies a property change.
                    /// </summary>
                    [Serializable]
                    public class UnityPropertyUpdateEvent : UnityEvent<Strategies.ObjectStrategy, string, object, object> { }

                    /// <summary>
                    ///   Event that triggers when the object changes a property in one of its strategies.
                    ///   This event is triggered explicitly via capabilities inside <see cref="Strategies.ObjectStrategy.PropertyWasUpdated(string, object, object)"/>.
                    /// </summary>
                    public readonly UnityPropertyUpdateEvent onStrategyPropertyUpdated = new UnityPropertyUpdateEvent();
                    #endregion

                    #region Sized
                    /// <summary>
                    ///   The width of this object, in map cells.
                    /// </summary>
                    [Delayed]
                    [SerializeField]
                    private ushort width = 1;

                    /// <summary>
                    ///   The height of this object, in map cells.
                    /// </summary>
                    [Delayed]
                    [SerializeField]
                    private ushort height = 1;

                    /// <summary>
                    ///   An extra height only considered for spatial indices.
                    ///   Some sort of perspective faking to visually locate
                    ///   this object in a spatial index, related to some sort
                    ///   of conceptual height (in virtual z-axis). E.g. Games
                    ///   like Pokemon use zHeight == 0, and games like Tibia
                    ///   or Argentum Online use zHeight == 1, typically.
                    /// </summary>
                    [Delayed]
                    [SerializeField]
                    private ushort zHeight = 0;

                    /// <summary>
                    ///   See <see cref="width"/>.
                    /// </summary>
                    public ushort Width => width; // Referencing directly allows us to query the width without a map assigned yet.

                    /// <summary>
                    ///   See <see cref="height"/>.
                    /// </summary>
                    public ushort Height => height; // Referencing directly allows us to query the height without a map assigned yet.

                    /// <summary>
                    ///   See <see cref="zHeight"/>.
                    /// </summary>
                    public ushort ZHeight => zHeight; // Referencing directly allows us to query the height without a map assigned yet.
                    
                    /// <summary>
                    ///   Returns the spatial data (useful for spatial indices).
                    /// </summary>
                    public Envelope Envelope => new Envelope { MinX = X, MinY = Y, MaxX = X + width, MaxY =Y + height + zHeight };
                    #endregion

                    #region Moving
                    /// <summary>
                    ///   This event class is a multi-purpose direction-related event for movements.
                    /// </summary>
                    [Serializable]
                    public class UnityMovementEvent : UnityEvent<Direction> { }

                    /// <summary>
                    ///   This event class is a multi-purpose nullable-direction-related event for movements.
                    /// </summary>
                    [Serializable]
                    public class UnityOptionalMovementEvent : UnityEvent<Direction?> { }

                    /// <summary>
                    ///   This event class notifies a speed change.
                    /// </summary>
                    [Serializable]
                    public class UnitySpeedEvent : UnityEvent<uint> { }

                    // Gets an offset vector for the movement given a direction and the map's cell size.
                    private Vector2 VectorForCurrentDirection()
                    {
                        switch (Movement)
                        {
                            case Direction.UP:
                                return Vector2.up * GetCellHeight();
                            case Direction.DOWN:
                                return Vector2.down * GetCellHeight();
                            case Direction.LEFT:
                                return Vector2.left * GetCellWidth();
                            case Direction.RIGHT:
                                return Vector2.right * GetCellWidth();
                        }
                        // This one is never reached!
                        return Vector2.zero;
                    }

                    /// <summary>
                    ///   The current movement of the object inside the attached map.
                    ///   It will be <c>null</c> if the object is not moving.
                    /// </summary>
                    public Direction? Movement { get { return (parentMap != null) ? parentMap.ObjectsLayer.StrategyHolder.StatusFor(StrategyHolder).Movement : null; } }

                    /// <summary>
                    ///   The default state provided for movement state.
                    /// </summary>
                    public static readonly State MOVING_STATE = State.Get("moving");

                    /// <summary>
                    ///   The default state provided for idle state.
                    /// </summary>
                    public static readonly State IDLE_STATE = State.Get("");

                    // Origin and target of movement. This has to do with the min/max values
                    //   of Snapped, but specified for the intended movement.
                    private Vector2 origin = Vector2.zero, target = Vector2.zero;

                    // The remaining duration for a queued movement in the queue.
                    private float queueRemainingDuration = 0f;

                    /// <summary>
                    ///   The movement speed, in game units per second.
                    /// </summary>
                    [SerializeField]
                    private uint speed = 4;

                    public uint Speed
                    {
                        get { return speed; }
                        set
                        {
                            speed = value;
                            onSpeedChanged.Invoke(value);
                        }
                    }

                    // This member hold the last movement being commanded to this object
                    private Direction? CommandedMovement = null;

                    /// <summary>
                    ///   Tells whether the object is moving. It knows that by reading the
                    ///   current movement in the underlying map object.
                    /// </summary>
                    public bool IsMoving { get { return Movement != null; } }
                    
                    /// <summary>
                    ///   Event that triggers when the object starts moving.
                    /// </summary>
                    public readonly UnityMovementEvent onMovementStarted = new UnityMovementEvent();

                    /// <summary>
                    ///   Event that triggers when the object cannot start moving.
                    /// </summary>
                    public readonly UnityMovementEvent onMovementRejected = new UnityMovementEvent();

                    /// <summary>
                    ///   Event that triggers when the object cancels its movement.
                    /// </summary>
                    public readonly UnityOptionalMovementEvent onMovementCancelled = new UnityOptionalMovementEvent();

                    /// <summary>
                    ///   Event that triggers when the object completes its movement into a cell.
                    /// </summary>
                    public readonly UnityMovementEvent onMovementFinished = new UnityMovementEvent();

                    /// <summary>
                    ///   Event that triggers when the object changes speed.
                    /// </summary>
                    public readonly UnitySpeedEvent onSpeedChanged = new UnitySpeedEvent();

                    /// <summary>
                    ///   Starts a movement in certain direction. It also allocates the internal
                    ///   movement of the object in the management strategy.
                    /// </summary>
                    /// <param name="movement">The direction of the new movement</param>
                    /// <param name="continued">Whether the movement is continued from another former movement</param>
                    /// <param name="queueIfMoving">
                    ///   If <c>true</c>, this movement is "stored" and will execute automatically
                    ///     after the current movement ends.
                    /// </param>
                    /// <returns>Whether the movement could be started</returns>
                    public bool StartMovement(Direction movement, bool continued = false, bool queueIfMoving = true)
                    {
                        if (ParentMap == null || Paused) return false;

                        if (IsMoving)
                        {
                            // The movement will not be performed now since there
                            //   is a movement in progess
                            if (queueIfMoving)
                            {
                                // Set the time lapse for the queue.
                                queueRemainingDuration = speed == 0 ? float.MaxValue : 1 / (4f * speed);
                                CommandedMovement = movement;
                            }
                            return false;
                        }
                        else if (parentMap.ObjectsLayer.StrategyHolder.MovementStart(StrategyHolder, movement, continued))
                        {
                            origin = new Vector3(X * GetCellWidth(), Y * GetCellHeight(), transform.localPosition.z);
                            target = origin + VectorForCurrentDirection();
                            CurrentState = MOVING_STATE;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    /// <summary>
                    ///   Cancels an already started movement.
                    /// </summary>
                    /// <returns>Whether the movement could be cancelled</returns>
                    public bool CancelMovement()
                    {
                        return parentMap != null && !Paused && parentMap.ObjectsLayer.StrategyHolder.MovementCancel(StrategyHolder);
                    }

                    // Performs the whole movement logic, even including movement
                    // queuing and calculating movement vectors, when a movement
                    // is allocated.
                    private void MovementTick()
                    {
                        if (ParentMap == null || Paused) return;

                        if (IsMoving)
                        {
                            Vector2 targetOffset = VectorForCurrentDirection();

                            // We calculate the movement offset
                            float movementNorm = speed * Time.deltaTime;

                            /**
                             * Now the logic must go like this:
                             * - If no next movement is commanded, or a different-than-current movement is commanded,
                             *   we must move 'till target, as now we do, and at the given speed.
                             * - Otherwise (same movement being commanded) our logic will be extended:
                             *   *** see the inners in the ELSE branch for details ***
                             */
                            if (CommandedMovement != Movement)
                            {
                                Vector2 movement = Vector2.MoveTowards(transform.localPosition, target, movementNorm);
                                if ((Vector2)transform.localPosition == movement)
                                {
                                    // If the movement and the localPosition (converted to 2D vector) are the same,
                                    //   we mark the movement as finished.
                                    parentMap.ObjectsLayer.StrategyHolder.MovementFinish(StrategyHolder);
                                }
                                else
                                {
                                    // Otherwise we adjust the localPosition to the intermediate step.
                                    transform.localPosition = new Vector3(movement.x, movement.y, transform.localPosition.z);
                                }
                            }
                            else
                            {
                                /**
                                 * Inners will be more elaborated here:
                                 * 1. We calculate our movement with a target position of (target) + (a vector with norm of [movement offset by current speed and timedelta] + 1).
                                 *    This is intended to avoid movement clamping against the target due to high speeds.
                                 * 2a. If the distance between this target position and the origin is less than the distance between the target and the origin,
                                 *     we just increment the position.
                                 * 2c. Otherwise (LOOPING) we ALSO mark the movement as completed, and start a new one (adapting origin and target)
                                 */
                                Vector2 movementDestination = target + targetOffset * (1 + movementNorm);
                                Vector2 movement = Vector2.MoveTowards(transform.localPosition, movementDestination, movementNorm);
                                // Adjusting the position as usual
                                transform.localPosition = new Vector3(movement.x, movement.y, transform.localPosition.z);
                                while (true)
                                {
                                    float traversedDistanceSinceOrigin = (movement - origin).magnitude;

                                    // We break this loop if the delta is lower than cell dimension because we
                                    //   do not need to mark new movements anymore.
                                    if (traversedDistanceSinceOrigin < targetOffset.magnitude) break;

                                    // We intend to at least finish this movement and perhaps continue with a new one
                                    Direction currentMovement = Movement.Value;
                                    uint currentMovementCount = movementCount;
                                    parentMap.ObjectsLayer.StrategyHolder.MovementFinish(StrategyHolder);
                                    // We break this loop if something else is happening (i.e.
                                    // due to a strategy intercepting the current movement or
                                    // the finish of the last movement and doing something else).
                                    // Usually, we would expect that, after finishing a movement,
                                    // the count is {currentCount} + 1. If something else happens,
                                    // that will not be true: it will be bigger. In that case, we
                                    // break the "one-shot movement continuation".
                                    if (movementCount != currentMovementCount + 1) break;
                                    currentMovementCount = movementCount;
                                    
                                    if (traversedDistanceSinceOrigin > targetOffset.magnitude)
                                    {
                                        origin = target;
                                        target = target + targetOffset;
                                        // If the movement cannot be performed, we break this loop
                                        //   and also clamp the movement to the actual box, so we
                                        //   avoid "bounces".
                                        if (!StartMovement(currentMovement))
                                        {
                                            transform.localPosition = new Vector3(origin.x, origin.y, transform.localPosition.z);
                                            break;
                                        }
                                        
                                        // AND the same check must be done here. In normal conditions,
                                        // the count would be {currentCount} + 1 unless something else
                                        // happens, since already a StartMovement happened. If that is
                                        // not the case, something else happened and we must break.
                                        if (movementCount != currentMovementCount + 1) break;
                                    }
                                }
                            }
                            Snap();
                        }
                        else if (CommandedMovement != null)
                        {
                            StartMovement(CommandedMovement.Value);
                            queueRemainingDuration = 0;
                            origin = transform.localPosition;
                            target = origin + VectorForCurrentDirection();
                            CurrentState = MOVING_STATE;
                        }
                        else
                        {
                            // In this case, the object is NOT moving.
                            // So, if the state key is MOVING_STATE then
                            // now the state key must be IDLE instead.
                            if (currentState == MOVING_STATE) CurrentState = IDLE_STATE;
                        }

                        queueRemainingDuration -= Time.deltaTime;
                        if (queueRemainingDuration <= 0)
                        {
                            queueRemainingDuration = 0;
                            // We clean up the last commanded movement, so future frames
                            //   do not interpret this command as a must, since it expired.
                            CommandedMovement = null;
                        }
                    }

                    /// <summary>
                    ///   Forces an active movement to finish: it moves straight to the target.
                    ///     WARNING: THIS METHOD MAY EVEN CAUSE INSTANTANEOUS MOVES IF USED TOO
                    ///     QUICKLY. While this may be a desired effect, it will NOT trigger
                    ///     teleports appropriately. This may be an issue to be faced in the
                    ///     future but, as of today, avoid abusing this on pure-client-side
                    ///     games.
                    /// </summary>
                    public void FinishMovement()
                    {
                        if (IsMoving)
                        {
                            parentMap.ObjectsLayer.StrategyHolder.MovementFinish(StrategyHolder);
                            Snap();
                        }
                    }
                    #endregion

                    #region Strategiful
                    /// <summary>
                    ///   The strategy holder of this object.
                    /// </summary>
                    public ObjectStrategyHolder StrategyHolder { get; private set; }
                    #endregion

                    #region Stateful
                    /// <summary>
                    ///   This event notifies state property changes.
                    /// </summary>
                    [Serializable]
                    public class StateEvent : UnityEvent<State> { }

                    /// <summary>
                    ///   Notifies when the state property changes.
                    /// </summary>
                    public readonly StateEvent OnStateChanged = new StateEvent();

                    // Keeps the current selected state, if any.
                    private State currentState = IDLE_STATE;

                    /// <summary>
                    ///   Gets or sets the selected state key. Notifies the interested
                    ///     behaviours of the key change. By default, the state is an
                    ///     empty string (which means: not moving).
                    /// </summary>
                    public State CurrentState
                    {
                        get
                        {
                            return currentState;
                        }
                        set
                        {
                            if (Paused) return;
                            currentState = value;
                            OnStateChanged.Invoke(currentState);
                        }
                    }
                    #endregion

                    #region Oriented
                    /// <summary>
                    ///   This event notifies orientation changes. Syntactically,
                    ///     it looks like a movement event.
                    /// </summary>
                    [Serializable]
                    public class OrientationEvent : UnityEvent<Direction> { }

                    /// <summary>
                    ///   Notofies when the direction property changes.
                    /// </summary>
                    public readonly OrientationEvent onOrientationChanged = new OrientationEvent();

                    /// <summary>
                    ///   The current objec's orientation. Set it in editor to tell an
                    ///     initial (and perhaps permanent, depending on your game)
                    ///     orientation for the object.
                    /// </summary>
                    [SerializeField]
                    private Direction orientation = Direction.FRONT;

                    /// <summary>
                    ///   Gets or sets the current orientation. Notifies the interested
                    ///     behaviours of the orientation change.
                    /// </summary>
                    public Direction Orientation
                    {
                        get
                        {
                            return orientation;
                        }
                        set
                        {
                            if (Paused) return;
                            orientation = value;
                            onOrientationChanged.Invoke(orientation);
                        }
                    }
                    #endregion

                    #region MapAware
                    /// <summary>
                    ///   This event notifies when the object is attached to a map.
                    /// </summary>
                    [Serializable]
                    public class UnityAttachedEvent : UnityEvent<Map> { }

                    /// <summary>
                    ///   Event that triggers when this object is attached to a map.
                    /// </summary>
                    public readonly UnityAttachedEvent onAttached = new UnityAttachedEvent();

                    /// <summary>
                    ///   Event that triggers when this object is detached from its map.
                    /// </summary>
                    public readonly UnityEvent onDetached = new UnityEvent();

                    /// <summary>
                    ///   This event notifies when the object is teleported in the
                    ///     same map.
                    /// </summary>
                    [Serializable]
                    public class UnityTeleportedEvent : UnityEvent<ushort, ushort> { }

                    /// <summary>
                    ///   Event that triggers after the object is teleported to a certain position inside the map.
                    /// </summary>
                    public readonly UnityTeleportedEvent onTeleported = new UnityTeleportedEvent();

                    /// <summary>
                    ///   The map this object is currently attached to.
                    /// </summary>
                    private Map parentMap;

                    /// <summary>
                    ///   Gets the parent map this object is attached to. See <see cref="parentMap"/>.
                    /// </summary>
                    public Map ParentMap => parentMap;

                    /// <summary>
                    ///   The current X position of the object inside the attached map.
                    /// </summary>
                    public ushort X => parentMap.ObjectsLayer.StrategyHolder.StatusFor(StrategyHolder).X;

                    /// <summary>
                    ///   The current Y position of the object inside the attached map.
                    /// </summary>
                    public ushort Y => parentMap.ObjectsLayer.StrategyHolder.StatusFor(StrategyHolder).Y;

                    /// <summary>
                    ///   The opposite X position of this object inside the attached map, with
                    ///     respect of its <see cref="width"/> value.
                    /// </summary>
                    /// <remarks>(Xf, Yf) point is the opposite corner of (X, Y).</remarks>
                    public ushort Xf { get { return (ushort)(parentMap.ObjectsLayer.StrategyHolder.StatusFor(StrategyHolder).X + Width - 1); } }

                    /// <summary>
                    ///   The opposite Y position of this object inside the attached map, with
                    ///     respect of its <see cref="height"/> value.
                    /// </summary>
                    /// <remarks>(Xf, Yf) point is the opposite corner of (X, Y).</remarks>
                    public ushort Yf { get { return (ushort)(parentMap.ObjectsLayer.StrategyHolder.StatusFor(StrategyHolder).Y + Height - 1); } }

                    /// <summary>
                    ///   See <see cref="ObjectsLayer.GetCellWidth"/>.
                    /// </summary>
                    /// <returns>The width of the cells of its parent Objects Layer</returns>
                    public float GetCellWidth()
                    {
                        return GetComponentInParent<ObjectsLayer>().GetCellWidth();
                    }

                    /// <summary>
                    ///   See <see cref="ObjectsLayer.GetCellHeight"/>.
                    /// </summary>
                    /// <returns>The height of the cells of its parent Objects Layers</returns>
                    public float GetCellHeight()
                    {
                        return GetComponentInParent<ObjectsLayer>().GetCellHeight();
                    }

                    /// <summary>
                    ///   Detaches the object from its map. See <see cref="ObjectsManagementStrategyHolder.Detach(ObjectStrategyHolder)"/>
                    ///     for more details.
                    /// </summary>
                    /// <remarks>It does nothing if the object is not attached to a map.</remarks>
                    public void Detach()
                    {
                        // There are some times at startup when the MapState object may be null.
                        // That's why we run the conditional.
                        //
                        // For the general cases, Detach will find a mapObjectState attached.
                        if (parentMap != null) parentMap.ObjectsLayer.StrategyHolder.Detach(StrategyHolder);
                    }

                    /// <summary>
                    ///   Attaches the object to a map.
                    /// </summary>
                    /// <param name="map">The map to attach the object to</param>
                    /// <param name="x">The new x position of the object</param>
                    /// <param name="y">The new y position of the object</param>
                    /// <param name="force">
                    ///   If true, the object will be detached from its previous map, and attached to this one.
                    ///   If false and the object is already attached to a map, an error will raise.
                    /// </param>
                    public void Attach(Map map, ushort x, ushort y, bool force = false)
                    {
                        if (force) Detach();
                        map.Attach(this, Values.Clamp(0, x, (ushort)(map.Width - 1)), Values.Clamp(0, y, (ushort)(map.Height - 1)));
                    }

                    /// <summary>
                    ///   Teleports the object to another position in the same map. It also triggers the <see cref="onTeleported"/>
                    ///     event unless <paramref name="silent"/> argument is set to true.
                    /// </summary>
                    /// <param name="x">The new x position of the object</param>
                    /// <param name="y">The new y position of the object</param>
                    /// <param name="silent">If true, this  teleportation will not trigger the <see cref="onTeleported"/> event</param>
                    /// <remarks>Does nothing if the object is paused.</remarks>
                    public void Teleport(ushort x, ushort y, bool silent = false)
                    {
                        if (parentMap != null && !Paused) parentMap.ObjectsLayer.StrategyHolder.Teleport(StrategyHolder, x, y, silent);
                    }
                    #endregion

                    #region Pausable
                    /// <summary>
                    ///   Flags the object, and its animations, as unpaused. This also invokes <see cref="Common.Pausable.Pause(bool)"/>
                    ///     on the pausable components of each attached visual.
                    /// </summary>
                    /// <param name="fullFreeze">If <c>true</c>, also flags the object animations as paused</param>
                    public void Pause(bool fullFreeze)
                    {
                        Paused = true;
                        AnimationsPaused = fullFreeze;
                        foreach (Visuals.Visual visual in visuals)
                        {
                            visual.GetComponent<Common.Pausable>().Pause(fullFreeze);
                        }
                    }

                    /// <summary>
                    ///   Flags the object, and its animations, as unpaused. This also invokes <see cref="Common.Pausable.Resume"/>
                    ///     on the pausable components of each attached visual.
                    /// </summary>
                    public void Resume()
                    {
                        Paused = false;
                        AnimationsPaused = false;
                        foreach (Visuals.Visual visual in visuals)
                        {
                            visual.GetComponent<Common.Pausable>().Resume();
                        }
                    }

                    /// <summary>
                    ///   Tells whether this object is paused.
                    /// </summary>
                    public bool Paused { get; private set; }

                    /// <summary>
                    ///   Tells whether the animations of this object are paused.
                    ///   For certain game configuration, you may have this in <c>false</c>
                    ///     even while having <see cref="Paused"/> in true.
                    /// </summary>
                    public bool AnimationsPaused { get; private set; }
                    #endregion

                    #region VisualHolder
                    // The visual objects that are attached to this object.
                    private HashSet<Visuals.Visual> visuals = new HashSet<Visuals.Visual>();

                    /// <summary>
                    ///   Returns the visual objects currently attached to this object.
                    /// </summary>
                    public IEnumerator<Visuals.Visual> Visuals
                    {
                        get
                        {
                            return visuals.GetEnumerator();
                        }
                    }

                    /// <summary>
                    ///   Map objects MAY have a visual considered the MAIN one. This
                    ///     is not mandatory but, if done, it will ensure the main visual
                    ///     is forever tied to this object.
                    /// </summary>
                    [SerializeField]
                    private Visuals.Visual mainVisual;

                    /// <summary>
                    ///   See <see cref="mainVisual"/>.
                    /// </summary>
                    public Visuals.Visual MainVisual { get { return mainVisual; } }

                    // Gets all the children visual objects.
                    private IEnumerable<Visuals.Visual> GetChildVisuals()
                    {
                        return from component in (
                          from index in Enumerable.Range(0, transform.childCount)
                          select transform.GetChild(index).GetComponent<Visuals.Visual>()
                        )
                               where component != null
                               select component;
                    }

                    // Attaches all the visuals that are direct children.
                    private void InitVisuals()
                    {
                        foreach (Visuals.Visual visual in GetChildVisuals().ToArray())
                        {
                            AddVisual(visual);
                            visual.DoStart();
                        }
                    }

                    /// <summary>
                    ///   Attaches the visual to this object, if it is not
                    ///     attached. Raises an exception if the visual is
                    ///     the main visual in another object, and fails
                    ///     silently if the visual is null.
                    /// </summary>
                    /// <param name="visual">The visual to add</param>
                    /// <returns>Whether the visual was just added</returns>
                    public bool AddVisual(Visuals.Visual visual)
                    {
                        if (!visual || visuals.Contains(visual)) return false;
                        if (visual.IsMain && visual.RelatedObject != this)
                        {
                            throw new Exception("The visual object trying to add is the main visual in another object");
                        }
                        visual.Detach();
                        visuals.Add(visual);
                        visual.OnAttached(this);
                        return true;
                    }

                    /// <summary>
                    ///   Detaches the visual from this object, if it is
                    ///     attached.
                    /// </summary>
                    /// <param name="visual">The visual to remove</param>
                    /// <returns>Whether the visual was just removed</returns>
                    public bool PopVisual(Visuals.Visual visual)
                    {
                        if (!visuals.Contains(visual)) return false;
                        if (visual.IsMain)
                        {
                            throw new Exception("The visual object trying to remove is the main visual in this object");
                        }
                        visuals.Remove(visual);
                        visual.OnDetached(this);
                        return true;
                    }
                    #endregion

                    #region Snapped
                    // Snaps the object to its appropriate local position given its
                    //   current coordinates.
                    private void Snap()
                    {
                        // Run this code only if this object is attached to a map, and not destroyed.
                        if (destroyed || !ParentMap) return;

                        bool snapInX = false;
                        bool snapInY = false;
                        bool clampInX = false;
                        bool clampInY = false;
                        float initialX = transform.localPosition.x;
                        // We invert the Y coordinate because States usually go up->down, and we expect it to be negative beforehand
                        float initialY = transform.localPosition.y;
                        float innerX = 0;
                        float innerY = 0;
                        float? minX = 0;
                        float? maxX = 0;
                        float? minY = 0;
                        float? maxY = 0;
                        float finalX = 0;
                        float finalY = 0;
                        float cellWidth = GetCellWidth();
                        float cellHeight = GetCellHeight();

                        // In this context, we can ALWAYS check for its current movement or position.

                        switch (Movement)
                        {
                            case Direction.LEFT:
                                snapInY = true;
                                clampInX = true;
                                minX = null;
                                maxX = X * cellWidth;
                                break;
                            case Direction.RIGHT:
                                snapInY = true;
                                clampInX = true;
                                minX = X * cellWidth;
                                maxX = null;
                                break;
                            case Direction.UP:
                                snapInX = true;
                                clampInY = true;
                                minY = Y * cellHeight;
                                maxY = null;
                                break;
                            case Direction.DOWN:
                                snapInX = true;
                                clampInY = true;
                                minY = null;
                                maxY = Y * cellHeight;
                                break;
                            default:
                                snapInX = true;
                                snapInY = true;
                                break;
                        }

                        innerX = snapInX ? X * cellWidth : initialX;
                        innerY = snapInY ? Y * cellHeight : initialY;

                        finalX = clampInX ? Values.Clamp(minX, innerX, maxX) : innerX;
                        finalY = clampInY ? Values.Clamp(minY, innerY, maxY) : innerY;

                        // We make the Y coordinate negative, as it was (or should be) in the beginning.
                        transform.localPosition = new Vector3(finalX, finalY, transform.localPosition.z);
                    }
                    #endregion
                }
            }
        }
    }
}