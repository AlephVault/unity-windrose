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
                    using AlephVault.Unity.Layout.Utils;

                    /// <summary>
                    ///   A map layer sorts itself inside its parent map, in the
                    ///     DEFAULT layer, and in the zero point, identity rotation,
                    ///     and identity scale. Map layers will hold different aspects
                    ///     of the map's content, and while currently only 4 layer
                    ///     types are supported, nothing prevents the development of
                    ///     more layer types (although this is entirely up to the
                    ///     developer).
                    /// </summary>
                    [RequireComponent(typeof(AlephVault.Unity.Support.Authoring.Behaviours.Normalized))]
                    [RequireComponent(typeof(SortingGroup))]
                    [ExecuteInEditMode]
                    public abstract class MapLayer : MonoBehaviour
                    {
                        /// <summary>
                        ///   Tells when the layer is not directly contained inside
                        ///     a map.
                        /// </summary>
                        public class ParentMustBeMapException : Types.Exception
                        {
                            public ParentMustBeMapException() : base() { }
                            public ParentMustBeMapException(string message) : base(message) { }
                        }

                        private SortingGroup sortingGroup;

                        /// <summary>
                        ///   The parent map this layer belongs to. It will be one level up
                        ///     in the hierarchy.
                        /// </summary>
                        public Map Map { get; private set; }

                        protected virtual void Awake()
                        {
                            sortingGroup = GetComponent<SortingGroup>();
                            try
                            {
                                Map = Behaviours.RequireComponentInParent<Map>(this);
                            }
                            catch (AlephVault.Unity.Support.Types.Exception)
                            {
                                Destroy(gameObject);
                                throw new ParentMustBeMapException();
                            }
                        }

                        /// <summary>
                        ///   Appropriately refreshes the sort order.
                        /// </summary>
                        protected virtual void Start()
                        {
                            sortingGroup.sortingLayerID = 0;
                            sortingGroup.sortingOrder = GetSortingOrder();
                        }

                        /// <summary>
                        ///   Executed only in editor mode, this refreshes the sort order as does Start().
                        /// </summary>
                        protected virtual void Update()
                        {
                            sortingGroup.sortingLayerID = 0;
                            sortingGroup.sortingOrder = GetSortingOrder();
                        }

                        /// <summary>
                        ///   This method is implemented in the subclasses, and return a
                        ///     constant number telling the sort order of the layer inside
                        ///     the parent map.
                        /// </summary>
                        /// <returns>The sorting layer to be used</returns>
                        protected abstract int GetSortingOrder();
                    }
                }
            }
        }
    }
}
