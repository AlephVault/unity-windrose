using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
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
                        ///   State bundle for sprite roses.
                        /// </summary>
                        [RequireComponent(typeof(MultiRoseSprited))]
                        public abstract class SpriteRoseBundle : StateBundle<ScriptableObjects.VisualResources.SpriteRose>
                        {
                        }
                    }
                }
            }
        }
    }
}