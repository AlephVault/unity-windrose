using System.Collections.Generic;
using GameMeanMachine.Unity.WindRose.Types;
using UnityEngine;


namespace GameMeanMachine.Unity.WindRose
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
                    ///   The end side of a <see cref="TaggedTeleporter"/>. See that
                    ///     class for more informaation.
                    /// </summary>
                    [RequireComponent(typeof(TriggerPlatform))]
                    public class TaggedTeleportTarget : MonoBehaviour
                    {
                        private static Dictionary<string, TaggedTeleportTarget> targets =
                            new Dictionary<string, TaggedTeleportTarget>();
                        
                        [SerializeField]
                        private string teleportKey;
                        
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
                        public Direction NewOrientation = Direction.DOWN;

                        private void Awake()
                        {
                            if (targets.ContainsKey(teleportKey))
                            {
                                Destroy(gameObject);
                                return;
                            }

                            targets[teleportKey] = this;
                        }

                        private void OnDestroy()
                        {
                            targets.Remove(teleportKey);
                        }
                        
                        public static bool TryGetTarget(string key, out TaggedTeleportTarget target)
                        {
                            return targets.TryGetValue(key, out target);
                        }
                    }
                }
            }
        }
    }
}
