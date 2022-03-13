using System;
using System.Collections;
using System.Collections.Generic;
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
                    /// <summary>
                    ///   MultiSprited state managers involve a sprite renderer and will
                    ///     give them the state in form of assigned sprite. This behaviour
                    ///     is incompatible with <see cref="Animated"/> or <see cref="RoseAnimated"/>.
                    /// </summary>
                    public class MultiSprited : MultiState<Sprite>
                    {
                        private SpriteRenderer renderer;

                        protected override void UseState(Sprite state)
                        {
                            renderer.sprite = state;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            if (GetComponent<Animated>() || GetComponent<RoseAnimated>())
                            {
                                Destroy(gameObject);
                                throw new Types.Exception(string.Format("{0} components are incompatible with {1} or {2}", typeof(MultiSprited).FullName, typeof(Animated).FullName, typeof(RoseAnimated).FullName));
                            }
                            renderer = GetComponent<SpriteRenderer>();
                        }
                    }
                }
            }
        }
    }
}