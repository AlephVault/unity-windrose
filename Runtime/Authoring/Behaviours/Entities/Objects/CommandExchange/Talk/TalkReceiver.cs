using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using AlephVault.Unity.Support.Utils;

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
                    namespace Talk
                    {
                        /// <summary>
                        ///   This class is usually attached to NPCs. It will receive the
                        ///     <see cref="TalkSender.COMMAND"/> sent by an <see cref="TalkSender"/>
                        ///     and trigger the <see cref="onTalkReceived"/> event.
                        /// </summary>
                        /// <remarks>
                        ///   When receiving the command, this object will look towards the opposite
                        ///     direction the sender object is looking to, and then trigger the
                        ///     <see cref="onTalkReceived"/> event.
                        /// </remarks>
                        [RequireComponent(typeof(CommandReceiver))]
                        public class TalkReceiver : MonoBehaviour
                        {
                            MapObject mapObject;

                            [Serializable]
                            public class UnityTalkReceivedEvent : UnityEvent<GameObject> { }

                            /// <summary>
                            ///   This event triggers when a <see cref="TalkSender.COMMAND"/> is received.
                            /// </summary>
                            /// <remarks>
                            ///   Only ONE UI-interaction-triggering handler should be added to this event.
                            /// </remarks>
                            public readonly UnityTalkReceivedEvent onTalkReceived = new UnityTalkReceivedEvent();

                            private void Start()
                            {
                                mapObject = GetComponent<MapObject>();
                                GetComponent<CommandReceiver>().ListenCommand(TalkSender.COMMAND, (string commandName, object[] arguments, GameObject sender) => {
                                    StartTalk(sender);
                                });
                            }

                            private async void StartTalk(GameObject sender)
                            {
                                MapObject senderMapObject = sender.GetComponent<MapObject>();
                                if (senderMapObject)
                                {
                                    switch (senderMapObject.Orientation)
                                    {
                                        case Types.Direction.DOWN:
                                            mapObject.Orientation = Types.Direction.UP;
                                            break;
                                        case Types.Direction.UP:
                                            mapObject.Orientation = Types.Direction.DOWN;
                                            break;
                                        case Types.Direction.LEFT:
                                            mapObject.Orientation = Types.Direction.RIGHT;
                                            break;
                                        case Types.Direction.RIGHT:
                                            mapObject.Orientation = Types.Direction.LEFT;
                                            break;
                                    }
                                    await Tasks.Blink();
                                }
                                onTalkReceived.Invoke(sender);
                            }
                        }
                    }
                }
            }
        }
    }
}