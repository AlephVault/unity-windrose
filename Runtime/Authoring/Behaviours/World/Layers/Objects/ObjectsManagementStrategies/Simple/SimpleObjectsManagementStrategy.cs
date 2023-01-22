using System;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Strategies.Simple;
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
                                    /// <summary>
                                    ///   The related layout strategy.
                                    /// </summary>
                                    public Base.LayoutObjectsManagementStrategy LayoutStrategy { get; private set; }

                                    /// <summary>
                                    ///   The related solidness strategy.
                                    /// </summary>
                                    public Solidness.SolidnessObjectsManagementStrategy SolidnessStrategy { get;
                                        private set;
                                    }
                                    
                                    protected override void Awake()
                                    {
                                        base.Awake();
                                        LayoutStrategy = GetComponent<Base.LayoutObjectsManagementStrategy>();
                                        SolidnessStrategy =
                                            GetComponent<Solidness.SolidnessObjectsManagementStrategy>();
                                    }
                                    
                                    protected override Type GetCounterpartType()
                                    {
                                        return typeof(SimpleObjectStrategy);
                                    }

                                    public override bool CanAllocateMovement(ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction,
                                        bool continued)
                                    {
                                        SimpleObjectStrategy simpleStrategy = (SimpleObjectStrategy)strategy;
                                        return LayoutStrategy.CanAllocateMovement(simpleStrategy.LayoutStrategy, status, direction,
                                                   continued) &&
                                               SolidnessStrategy.CanAllocateMovement(simpleStrategy.SolidnessStrategy, status, direction,
                                                   continued);
                                    }

                                    public override bool CanClearMovement(ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        return true;
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
