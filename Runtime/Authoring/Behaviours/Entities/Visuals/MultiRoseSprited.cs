using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        using ScriptableObjects.VisualResources;

        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    /// <summary>
                    ///   MultiRoseSprited state managers involve a rose sprited behavior and
                    ///     will give them the state in form of an <see cref="SpriteRose"/>.
                    /// </summary>
                    [RequireComponent(typeof(RoseSprited))]
                    public class MultiRoseSprited : MultiState<SpriteRose>
                    {
                        private RoseSprited roseSprited;

                        protected override void UseState(SpriteRose state)
                        {
                            roseSprited.SpriteRose = state;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            roseSprited = GetComponent<RoseSprited>();
                        }
                    }
                }
            }
        }

    }
}

