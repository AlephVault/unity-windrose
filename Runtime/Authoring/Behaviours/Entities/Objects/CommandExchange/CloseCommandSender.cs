using System;
using System.Collections;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace CommandExchange
                {
                    /// <summary>
                    ///   <para>
                    ///     Sends a command to an adjacent object. If the object contains a
                    ///       <see cref="CommandReceiver"/> component, it could process the
                    ///       command being sent.
                    ///   </para>
                    ///   <para>
                    ///     The object will throw command through <see cref="Cast(string, bool, object[])"/>,
                    ///       which may be instantaneous (i.e. just one frame) or stay until
                    ///       the command is released with <see cref="Release"/>. Only one command
                    ///       may exist at a time: succesive calls will release former still-alive
                    ///       commands.
                    ///   </para>
                    /// </summary>
                    /// <remarks>
                    ///   If the object being hit by a non-instantaneous command moves away, then
                    ///     it will count as if the command was released. Also, when it moves again
                    ///     into the place where the command was hit, it will count as the command
                    ///     being cast again. A good practice is to drop instantaneous commands
                    ///     to avoid this case.
                    /// </remarks>
                    [RequireComponent(typeof(MapObject))]
                    public class CloseCommandSender : MonoBehaviour, Common.Pausable.IPausable
                    {
                        private MapObject mapObject;
                        private bool paused = false;
                        private static Collider[] targets = new Collider[ushort.MaxValue];

                        private void Start()
                        {
                            mapObject = GetComponent<MapObject>();
                        }

                        private Vector3 ComputeCommandPosition()
                        {
                            float x = 0, y = 0;
                            switch (mapObject.Orientation)
                            {
                                case Types.Direction.DOWN:
                                    x = mapObject.transform.position.x + (mapObject.Width / 2f) * mapObject.GetCellWidth();
                                    y = mapObject.transform.position.y - 0.5f * mapObject.GetCellHeight();
                                    break;
                                case Types.Direction.UP:
                                    x = mapObject.transform.position.x + (mapObject.Width / 2f) * mapObject.GetCellWidth();
                                    y = mapObject.transform.position.y + (mapObject.Height + 0.5f) * mapObject.GetCellHeight();
                                    break;
                                case Types.Direction.LEFT:
                                    y = mapObject.transform.position.y + (mapObject.Height / 2f) * mapObject.GetCellHeight();
                                    x = mapObject.transform.position.x - 0.5f * mapObject.GetCellWidth();
                                    break;
                                case Types.Direction.RIGHT:
                                    y = mapObject.transform.position.y + (mapObject.Height / 2f) * mapObject.GetCellHeight();
                                    x = mapObject.transform.position.x + (mapObject.Width + 0.5f) * mapObject.GetCellWidth();
                                    break;
                                default:
                                    x = mapObject.transform.position.x;
                                    y = mapObject.transform.position.y;
                                    break;
                            }
                            return new Vector3(x, y, 0);
                        }

                        /// <summary>
                        ///   Casts a command in the direction it is looking to.
                        /// </summary>
                        /// <param name="commandName">The command name</param>
                        /// <param name="maxDeliver">
                        ///   If positive, it will be the maximum number of objects that
                        ///     can successfully attend the command being sent.
                        /// </param>
                        /// <param name="arguments">The command arguments</param>
                        public void Cast(string commandName, int maxDeliver = 1, object[] arguments = null)
                        {
                            if (paused) return;
                            Vector3 commandPosition = ComputeCommandPosition();
                            int targetsCount = Physics.OverlapSphereNonAlloc(commandPosition, 0.1f, targets);
                            int delivers = 0;
                            for (int index = 0; index < targetsCount; index++)
                            {
                                if (delivers >= maxDeliver && maxDeliver > 0)
                                {
                                    break;
                                }
                                Collider target = targets[index];
                                CommandReceiver receiver = target.gameObject.GetComponent<CommandReceiver>();
                                if (receiver)
                                {
                                    if (receiver.SendCommand(commandName, arguments, this.gameObject))
                                    {
                                        delivers++;
                                    }
                                }
                            }
                        }

                        /// <summary>
                        ///   Pauses this behaviour - it will not cast anything.
                        /// </summary>
                        public void Pause(bool fullFreeze)
                        {
                            paused = true;
                        }

                        /// <summary>
                        ///   Resumes the behaviour - it will cast again if told to.
                        /// </summary>
                        public void Resume()
                        {
                            paused = false;
                        }
                    }
                }
            }
        }
    }
}
