using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameMeanMachine.Unity.WindRose
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
                    ///     Receives a command sent from any kind of command sender. Perhaps even no need for
                    ///       a particular sender is needed, but just invoke <see cref="SendCommand(string, object[], GameObject)"/>
                    ///       on this behaviour.
                    ///   </para>
                    ///   <para>
                    ///     Other components depending on this one may be interested in adding listeners
                    ///       by invoking <see cref="ListenCommand(string, Action<string, object[], GameObject>)"/>.
                    ///   </para>
                    /// </summary>
                    [RequireComponent(typeof(TriggerLive))]
                    public class CommandReceiver : MonoBehaviour, Common.Pausable.IPausable
                    {
                        public class UnityCommandReceivedEvent : UnityEvent<string, object[], GameObject, Action> { }

                        ///   This event is triggered when the object receives a command from somewhere.
                        private UnityCommandReceivedEvent onCommandReceiver = new UnityCommandReceivedEvent();

                        /// <summary>
                        ///   Sends a command to this component. The component will determine whether it
                        ///     can attend the command or not. If the component is not enabled, it will
                        ///     not attend any command.
                        /// </summary>
                        /// <param name="commandName">Command name</param>
                        /// <param name="args">Arguments</param>
                        /// <param name="sender">The command sender</param> 
                        /// <returns><c>true</c>, if command was processed, <c>false</c> otherwise.</returns>
                        public bool SendCommand(string commandName, object[] args, GameObject sender)
                        {
                            if (enabled)
                            {
                                bool processed = false;
                                onCommandReceiver.Invoke(commandName, args, sender, () => {
                                    processed = true;
                                });
                                return processed;
                            }
                            else
                            {
                                return false;
                            }
                        }

                        /// <summary>
                        ///   Listens the command using a particular listener receiving the command name and
                        ///     arguments (the same listener may be used to attend many commands).
                        /// </summary>
                        /// <param name="commandName">Command name</param>
                        /// <param name="listener">Listener callback</param>
                        public void ListenCommand(string commandName, Action<string, object[], GameObject> listener)
                        {
                            onCommandReceiver.AddListener((string cmdName, object[] args, GameObject sender, Action reporter) => {
                                if (commandName == cmdName)
                                {
                                    listener(cmdName, args, sender);
                                    reporter();
                                }
                            });
                        }

                        /// <summary>
                        ///   Pauses the object - this means the object will not attend commands.
                        /// </summary>
                        public void Pause(bool fullFreeze)
                        {
                            enabled = false;
                        }

                        /// <summary>
                        ///   Resumes the object - it will attend commands again.
                        /// </summary>
                        public void Resume()
                        {
                            enabled = true;
                        }
                    }
                }
            }
        }
    }
}
