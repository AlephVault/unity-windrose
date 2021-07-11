using UnityEngine;
using UnityEngine.Rendering;

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
                        using Entities.Objects;

                        /// <summary>
                        ///   <para>
                        ///     The layer that will contain the map objects. This layer is mandatory.
                        ///     It will also pair with its strategy holder, which must be appropriately
                        ///       setup (i.e. strategies, and main strategy).
                        ///   </para>
                        ///   <para>
                        ///     This component depends on <see cref="Grid"/> and will provide the
                        ///       values of the cell size. It will match the same values in the
                        ///       <see cref="Floor.FloorLayer"/>'s <see cref="Grid" /> component,
                        ///       although it is not strictly required or enforced.
                        ///   </para>
                        /// </summary>
                        [RequireComponent(typeof(Grid))]
                        public class ObjectsLayer : MapLayer
                        {
                            private Grid grid;

                            protected override void Awake()
                            {
                                base.Awake();
                                grid = GetComponent<Grid>();
                                if (grid.cellLayout != GridLayout.CellLayout.Rectangle)
                                {
                                    Destroy(gameObject);
                                    throw new Types.Exception("Object layers' grids only support Rectangle grids for their tilemap(s)");
                                }
                                StrategyHolder = GetComponent<ObjectsManagementStrategyHolder>();
                            }

                            protected override void Start()
                            {
                                base.Start();
#if UNITY_EDITOR
                                if (!Application.isPlaying) return;
#endif
                                // Initializing strategy
                                if (StrategyHolder == null)
                                {
                                    throw new Types.Exception("An objects management strategy holder is required when the map initializes.");
                                }
                                StrategyHolder.Initialize();
                                // We consider this map as initialized after its strategy started.
                                Initialized = true;
                                // Now, it is turn of the already-in-place map objects to initialize.
                                foreach (MapObject mapObject in GetComponentsInChildren<MapObject>())
                                {
                                    mapObject.Initialize();
                                }
                            }

                            protected override int GetSortingOrder()
                            {
                                return 30;
                            }

                            /// <summary>
                            ///   Gets the cell width.
                            /// </summary>
                            /// <returns>The cell width, in game units</returns>
                            public float GetCellWidth()
                            {
                                return grid.cellSize.x;
                            }

                            /// <summary>
                            ///   Gets the cell height.
                            /// </summary>
                            /// <returns>The cell height, in game units</returns>
                            public float GetCellHeight()
                            {
                                return grid.cellSize.y;
                            }

                            /// <summary>
                            ///   The objects strategy holder. It manages the rules under which the
                            ///     objcts inside can perform movements.
                            /// </summary>
                            public ObjectsManagementStrategyHolder StrategyHolder { get; private set; }

                            /// <summary>
                            ///   Tells whether the map is initialized. No need to make use of
                            ///     this property, but <see cref="MapObject"/> objects will.
                            /// </summary>
                            public bool Initialized { get; private set; }

                            /// <summary>
                            ///   Attaches an object to this layer.
                            /// </summary>
                            /// <param name="mapObject">The object to attach</param>
                            /// <param name="x">The new X position</param>
                            /// <param name="y">The new Y position</param>
                            public void Attach(MapObject mapObject, uint x, uint y)
                            {
                                if (Initialized) StrategyHolder.Attach(mapObject.StrategyHolder, x, y);
                            }
                        }
                    }
                }
            }
        }
    }
}
