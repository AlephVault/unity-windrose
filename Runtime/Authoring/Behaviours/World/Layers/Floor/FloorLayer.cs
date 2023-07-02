using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

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
                    namespace Floor
                    {
                        /// <summary>
                        ///   <para>
                        ///     The floor layer. This layer is mandatory and will hold the
                        ///       floors, which in turn hold the tilemaps to be rendered.
                        ///   </para>
                        ///   <para>
                        ///     This component depends on a <see cref="Grid"/> component,
                        ///       and its cell size must be set for the children tilemaps.
                        ///   </para>
                        /// </summary>
                        [RequireComponent(typeof(Grid))]
                        public class FloorLayer : MapLayer
                        {
                            /**
                             * A floor layer will have several floors, which are in turn tilemaps.
                             * It will scrap its tilemaps, however.
                             */

                            private Tilemap[] tilemaps;

                            private class TempListElement
                            {
                                public readonly int SortingOrder;
                                public readonly TilemapRenderer Renderer;
                                public readonly Tilemap Tilemap;

                                public TempListElement(int sortingOrder, TilemapRenderer renderer, Tilemap tilemap)
                                {
                                    SortingOrder = sortingOrder;
                                    Renderer = renderer;
                                    Tilemap = tilemap;
                                }
                            }

                            private void EnsureTilemaps()
                            {
                                if (tilemaps != null) return;
                                List<TempListElement> elements = new List<TempListElement>();
                                foreach (Floors.Floor floor in GetComponentsInChildren<Floors.Floor>())
                                {
                                    TilemapRenderer renderer = floor.GetComponent<TilemapRenderer>();
                                    elements.Add(new TempListElement(renderer.sortingOrder, renderer, floor.GetComponent<Tilemap>()));
                                }
                                tilemaps = (from element in elements
                                            orderby element.SortingOrder
                                            select element.Tilemap).ToArray();
                            }

                            protected override void Awake()
                            {
                                base.Awake();
                                // We sort the layers accordingly - please use different sorting orders explicitly.
                                Grid grid = GetComponent<Grid>();
                                if (grid.cellLayout != GridLayout.CellLayout.Rectangle)
                                {
                                    Destroy(gameObject);
                                    throw new Types.Exception("Floors' grids only support Rectangle grids for their tilemap(s)");
                                }
                                grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                            }

                            protected override int GetSortingOrder()
                            {
                                return 10;
                            }

                            /**
                             * When starting, it will reset the transform of all its children tilemaps.
                             */
                            protected override void Start()
                            {
                                base.Start();
                                int index = 0;
                                foreach (Tilemap tilemap in Tilemaps)
                                {
                                    TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
                                    renderer.sortingLayerID = 0;
                                    renderer.sortingOrder = index++;
                                };
                            }

                            /// <summary>
                            ///   List all the tilemaps. They will be extracted from its children
                            ///     <see cref="Floors.Floor"/> objects.
                            /// </summary>
                            public IEnumerable<Tilemap> Tilemaps
                            {
                                get
                                {
                                    EnsureTilemaps();
                                    return tilemaps.AsEnumerable();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
