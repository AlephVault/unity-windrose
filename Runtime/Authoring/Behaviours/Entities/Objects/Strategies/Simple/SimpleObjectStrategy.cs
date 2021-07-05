using System;
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
                        [RequireComponent(typeof(Base.LayoutObjectStrategy))]
                        [RequireComponent(typeof(Solidness.SolidnessObjectStrategy))]
                        public class SimpleObjectStrategy : ObjectStrategy
                        {
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
