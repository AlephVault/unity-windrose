using System;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies.Base;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies.Solidness;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Strategies
                {
                    namespace Simple
                    {
                        /// <summary>
                        ///   Simple object strategy is just a combination of
                        ///   <see cref="Base.LayoutObjectStrategy"/> and
                        ///   <see cref="Solidness.SolidnessObjectStrategy"/>.
                        ///   Its counterpart type is
                        ///   <see cref="World.Layers.Objects.ObjectsManagementStrategies.Simple.SimpleObjectsManagementStrategy"/>.
                        /// </summary>
                        [RequireComponent(typeof(LayoutObjectStrategy))]
                        [RequireComponent(typeof(SolidnessObjectStrategy))]
                        public class SimpleObjectStrategy : ObjectStrategy
                        {
                            /// <summary>
                            ///   The related layout strategy.
                            /// </summary>
                            public LayoutObjectStrategy LayoutStrategy { get; private set; }

                            /// <summary>
                            ///   The related solidness strategy.
                            /// </summary>
                            public SolidnessObjectStrategy SolidnessStrategy { get; private set; }

                            protected override void Awake()
                            {
                                base.Awake();
                                LayoutStrategy = GetComponent<LayoutObjectStrategy>();
                                SolidnessStrategy = GetComponent<SolidnessObjectStrategy>();
                            }

                            /// <summary>
                            ///   Its counterpart type is
                            ///   <see cref="World.Layers.Objects.ObjectsManagementStrategies.Simple.SimpleObjectsManagementStrategy"/>.
                            /// </summary>
                            protected override Type GetCounterpartType()
                            {
                                return typeof(World.Layers.Objects.ObjectsManagementStrategies.Simple.SimpleObjectsManagementStrategy);
                            }
                        }
                    }
                }
            }
        }
    }
}
