using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                using Strategies;

                /// <summary>
                ///   <para>
                ///     Object strategy holders are the complement of Map's object management strategy
                ///       holders, as objects are the complement of maps, and as object strategies are
                ///       the complement of object management strategies. This class is the other side
                ///       of the relationship between an object and the map it belongs to (the other
                ///       side of the relationship is <see cref="World.ObjectsManagementStrategyHolder"/>).
                ///   </para>
                ///   <para>
                ///     Their main purpose is to be in the same place as their related instances of
                ///       <see cref="Strategies.ObjectStrategy"/>, and pick one of them as the default.
                ///     The default strategy will determine the main state to be checked and also
                ///       will involve dependencies to ther strategies (and their states).
                ///   </para>
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                public class ObjectStrategyHolder : MonoBehaviour
                {
                    /// <summary>
                    ///   Tells when the instance being picked into <see cref="objectStrategy"/> is
                    ///     null or does not belong to this behaviour's underlying Game Object.
                    /// </summary>
                    public class InvalidStrategyComponentException : Types.Exception
                    {
                        public InvalidStrategyComponentException() { }
                        public InvalidStrategyComponentException(string message) : base(message) { }
                        public InvalidStrategyComponentException(string message, Exception inner) : base(message, inner) { }
                    }

                    /// <summary>
                    ///   The related <see cref="Objects.MapObject"/> (the in-map object).
                    /// </summary>
                    public MapObject Object { get; private set; }

                    /// <summary>
                    ///   The main strategy of this object.
                    /// </summary>
                    /// <remarks>
                    ///   This strategy will have to be compatible with the
                    ///   <see cref="World.ObjectsManagementStrategies.ObjectsManagementStrategy"/>
                    ///   picked in the <see cref="World.ObjectsManagementStrategyHolder"/>'s main
                    ///   strategy component in the map this object will intend to be attached into.
                    /// </remarks>
                    [SerializeField]
                    private ObjectStrategy objectStrategy;

                    /// <summary>
                    ///   see <see cref="objectStrategy"/>.
                    /// </summary>
                    public ObjectStrategy ObjectStrategy { get { return objectStrategy; } }

                    /// <summary>
                    ///   <para>
                    ///     This method will be invoked on initialization of the related
                    ///       <see cref="Object"/> object. It must not be invoked
                    ///       directly.
                    ///   </para>
                    ///   <para>
                    ///     Initializes the underlying main strategy (the one picked into
                    ///       the <see cref="objectStrategy"/> property).
                    ///   </para>
                    /// </summary>
                    public void Initialize()
                    {
                        ObjectStrategy.Initialize();
                    }

                    /**
                        * On initialization, the strategy will fetch its map object to, actually, know it.
                        */
                    protected virtual void Awake()
                    {
                        Object = GetComponent<MapObject>();
                        if (objectStrategy == null || !(new HashSet<ObjectStrategy>(GetComponents<ObjectStrategy>()).Contains(objectStrategy)))
                        {
                            Destroy(gameObject);
                            throw new InvalidStrategyComponentException("The selected strategy component must be non-null and present among the current object's components");
                        }
                    }
                }
            }
        }
    }
}
