using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Solidness
                            {
                                /// <summary>
                                ///   Step type tells what kind of occupancy change will occur for holes, solid, or "solid for others"
                                ///     objects when starting, finishing and cancelling a movement. This value will be specified in
                                ///     <see cref="SolidnessObjectsManagementStrategy"/> objects and will not be changed. Options will
                                ///     be "safe" and "optimist". The "safe" option will assume there is high change of cancelling the
                                ///     started movement(s) in the game (i.e. the developer will provide a way to invoke the
                                ///     <see cref="ObjectsManagementStrategyHolder.MovementCancel(Entities.Objects.ObjectStrategyHolder)"/>
                                ///     method in the UI or game logic, and/or there will be a considerable amount of teleports) and will
                                ///     "reserve" the cells both "before" and "after" the movement while the movement is in progress (and
                                ///     clear the cells "before" the movement when the movement ends / clear de cells "after" the
                                ///     movement when the movement is cancelled), while the "optimistic" option will assume there is low
                                ///     change of cancelling the movement or teleporting objects: when starting a movement, it will clear
                                ///     the cells "before" the movement and reserve the cells "after" the movement (finishing a movement
                                ///     will do nothing, while cancelling a movement will revert both the clearing and the reservation).
                                /// </summary>
                                public enum StepType
                                {
                                    Safe, Optimistic
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
