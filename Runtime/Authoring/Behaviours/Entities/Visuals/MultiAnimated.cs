using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        using ScriptableObjects.Animations;

        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    /// <summary>
                    ///   MultiAnimated state managers involve an animated behavior and will give
                    ///     them the state in form of an <see cref="Animation"/>. This behaviour
                    ///     is incompatible with <see cref="RoseAnimated"/>.
                    /// </summary>
                    [RequireComponent(typeof(Animated))]
                    public class MultiAnimated : MultiState<ScriptableObjects.Animations.Animation>
                    {
                        private Animated animated;

                        protected override void UseState(ScriptableObjects.Animations.Animation state)
                        {
                            animated.Animation = state;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            if (GetComponent<RoseAnimated>())
                            {
                                Destroy(gameObject);
                                throw new Types.Exception(string.Format("{0} components are incompatible with {1}", typeof(MultiAnimated).FullName, typeof(RoseAnimated).FullName));
                            }
                            animated = GetComponent<Animated>();
                        }
                    }
                }
            }
        }
    }
}
