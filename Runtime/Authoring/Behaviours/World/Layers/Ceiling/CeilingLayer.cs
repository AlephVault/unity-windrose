using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                    namespace Ceiling
                    {
                        /// <summary>
                        ///   The topmost layer (if present, and no custom-type
                        ///     layers exist). It will hold <see cref="Ceilings.Ceiling"/>
                        ///     objects inside.
                        /// </summary>
                        [RequireComponent(typeof(Grid))]
                        public class CeilingLayer : MapLayer
                        {
                            protected override int GetSortingOrder()
                            {
                                return 50;
                            }

                            protected override void Awake()
                            {
                                base.Awake();
                                // We sort the layers accordingly - please use different sorting orders explicitly.
                                Grid grid = GetComponent<Grid>();
                                if (grid.cellLayout != GridLayout.CellLayout.Rectangle)
                                {
                                    Destroy(gameObject);
                                    throw new Types.Exception("Ceilings' grids only support Rectangle grids for their tilemap(s)");
                                }
                                grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                            }
                        }
                    }
                }
            }
        }
    }
}
