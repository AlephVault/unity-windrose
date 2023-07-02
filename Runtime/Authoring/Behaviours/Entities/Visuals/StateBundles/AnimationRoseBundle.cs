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
                        ///   State bundle for animation roses.
                        /// </summary>
                        [RequireComponent(typeof(MultiRoseAnimated))]
                        public abstract class AnimationRoseBundle : StateBundle<ScriptableObjects.VisualResources.AnimationRose>
                        {
                        }
                    }
                }
            }
        }
    }
}