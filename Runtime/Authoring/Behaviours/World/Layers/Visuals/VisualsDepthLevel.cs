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
                    namespace Visuals
                    {
                        /// <summary>
                        ///   <para>
                        ///     Visual depth levels serve as a perspective plane for different
                        ///       levels of visuals. Inside, visuals will sort according to the
                        ///       Y coordinate of their positionables.
                        ///   </para>
                        /// </summary>
                        [RequireComponent(typeof(SortingGroup))]
                        public class VisualsDepthLevel : MonoBehaviour
                        {
                            void Awake()
                            {
                                transform.localPosition = Vector3.zero;
                            }
                        }
                    }
                }
            }
        }
    }
}
