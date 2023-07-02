using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    namespace StateBundles
                    {
                        /// <summary>
                        ///   State bundle for animations.
                        /// </summary>
                        [RequireComponent(typeof(MultiAnimated))]
                        public abstract class AnimationBundle : StateBundle<ScriptableObjects.VisualResources.Animation>
                        {
                        }
                    }
                }
            }
        }
    }
}
