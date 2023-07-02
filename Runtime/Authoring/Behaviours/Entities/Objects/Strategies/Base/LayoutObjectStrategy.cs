using System;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Strategies
                {
                    namespace Base
                    {
                        /// <summary>
                        ///   This strategy is just the counterpart of <see cref="World.Layers.Objects.ObjectsManagementStrategies.Base.LayoutObjectsManagementStrategy"/>.
                        /// </summary>
                        public class LayoutObjectStrategy : ObjectStrategy
                        {
                            /// <summary>
                            ///   The counterpart type is <see cref="World.Layers.Objects.ObjectsManagementStrategies.Base.LayoutObjectsManagementStrategy"/>.
                            /// </summary>
                            protected override Type GetCounterpartType()
                            {
                                return typeof(World.Layers.Objects.ObjectsManagementStrategies.Base.LayoutObjectsManagementStrategy);
                            }
                        }
                    }
                }
            }
        }
    }
}
