using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace Tiles
            {
                namespace Strategies
                {
                    namespace Base
                    {
                        /**
                         * Layout strategies tell whether the cell is blocking or not.
                         * Blocking cells cannot be walked through (as determined in
                         *   the Layout map strategy).
                         */
                        [CreateAssetMenu(fileName = "NewLayoutTileStrategy", menuName = "Wind Rose/Tile Strategies/Layout", order = 201)]
                        public class LayoutTileStrategy : TileStrategy
                        {
                            [SerializeField]
                            private bool blocks;

                            public bool Blocks
                            {
                                get
                                {
                                    return blocks;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
