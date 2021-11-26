using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    using Types;
    namespace Authoring
    {
        namespace Behaviours
        {
            using Entities.Objects.Strategies;

            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Simple
                            {
                                /// <summary>
                                ///   <para>
                                ///     Combines the power of <see cref="Base.LayoutObjectsManagementStrategy"/>
                                ///       which forbids walking through blocked cells, and <see cref="Solidness.SolidnessObjectsManagementStrategy"/>
                                ///       which forbids solid objects walking through occupied cells.
                                ///   </para>
                                ///   <para>
                                ///     Its counterpart is <see cref="Entities.Objects.Strategies.Simple.SimpleObjectStrategy"/>.
                                ///   </para> 
                                /// </summary>
                                [RequireComponent(typeof(Base.LayoutObjectsManagementStrategy))]
                                [RequireComponent(typeof(Solidness.SolidnessObjectsManagementStrategy))]
                                public class SimpleObjectsManagementStrategy : ObjectsManagementStrategy
                                {
                                    protected override ObjectsManagementStrategy[] GetDependencies()
                                    {
                                        return new ObjectsManagementStrategy[]
                                        {
                                            GetComponent<Base.LayoutObjectsManagementStrategy>(),
                                            GetComponent<Solidness.SolidnessObjectsManagementStrategy>()
                                        };
                                    }

                                    protected override Type GetCounterpartType()
                                    {
                                        return typeof(Entities.Objects.Strategies.Simple.SimpleObjectStrategy);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
