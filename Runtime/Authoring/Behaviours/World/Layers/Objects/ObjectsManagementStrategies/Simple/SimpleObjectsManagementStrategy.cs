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
                                    /// <summary>
                                    ///   Combines the result of <see cref="Base.LayoutObjectsManagementStrategy.CanAllocateMovement(Dictionary{Type, bool}, ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool)"/>
                                    ///     and <see cref="Solidness.SolidnessObjectsManagementStrategy.CanAllocateMovement(Dictionary{Type, bool}, ObjectStrategy, ObjectsManagementStrategyHolder.Status, Direction, bool)"/>
                                    ///     with an AND operation.
                                    /// </summary>
                                    public override bool CanAllocateMovement(Dictionary<Type, bool> otherComponentsResults, ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status, Direction direction, bool continued)
                                    {
                                        bool layoutAllowsAllocation = otherComponentsResults[typeof(Base.LayoutObjectsManagementStrategy)];
                                        bool solidnessAllowsAllocaction = otherComponentsResults[typeof(Solidness.SolidnessObjectsManagementStrategy)];
                                        return layoutAllowsAllocation && solidnessAllowsAllocaction;
                                    }

                                    /// <summary>
                                    ///   Combines the result of <see cref="Base.LayoutObjectsManagementStrategy.CanClearMovement(Dictionary{Type, bool}, ObjectStrategy, ObjectsManagementStrategyHolder.Status)"/>
                                    ///     and <see cref="Solidness.SolidnessObjectsManagementStrategy.CanClearMovement(Dictionary{Type, bool}, ObjectStrategy, ObjectsManagementStrategyHolder.Status)"/>
                                    ///     with an AND operation.
                                    /// </summary>
                                    public override bool CanClearMovement(Dictionary<Type, bool> otherComponentsResults, ObjectStrategy strategy, ObjectsManagementStrategyHolder.Status status)
                                    {
                                        bool layoutAllowsClearing = otherComponentsResults[typeof(Base.LayoutObjectsManagementStrategy)];
                                        bool solidnessAllowsClearing = otherComponentsResults[typeof(Solidness.SolidnessObjectsManagementStrategy)];
                                        return layoutAllowsClearing && solidnessAllowsClearing;
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
