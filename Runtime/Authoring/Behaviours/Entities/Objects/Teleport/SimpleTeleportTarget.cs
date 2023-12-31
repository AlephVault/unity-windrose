﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Teleport
                {
                    /// <summary>
                    ///   The end side of a <see cref="SimpleTeleporter"/>. See that
                    ///     class for more informaation.
                    /// </summary>
                    [RequireComponent(typeof(TriggerPlatform))]
                    public class SimpleTeleportTarget : MonoBehaviour
                    {
                        /// <summary>
                        ///   Tells whether the just teleported object will look
                        ///     (by setting its <see cref="MapObject.orientation"/>
                        ///     property to a new value) to the orientation specified
                        ///     in <see cref="NewOrientation"/>.
                        /// </summary>
                        public bool ForceOrientation = true;

                        /// <summary>
                        ///   This property is meaningful only if <see cref="ForceOrientation"/>
                        ///     is <c>true</c>.
                        /// </summary>
                        public Types.Direction NewOrientation = Types.Direction.DOWN;
                    }
                }
            }
        }
    }
}
