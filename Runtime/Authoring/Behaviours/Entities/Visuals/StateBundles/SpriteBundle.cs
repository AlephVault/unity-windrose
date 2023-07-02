using System.Collections;
using System.Collections.Generic;
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
                        ///   State bundle for sprites.
                        /// </summary>
                        [RequireComponent(typeof(MultiSprited))]
                        public abstract class SpriteBundle : StateBundle<Sprite>
                        {
                        }
                    }
                }
            }
        }
    }
}