using UnityEngine;

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
                    namespace Talk
                    {
                        /// <summary>
                        ///   This behaviour provides features to send a <see cref="Talk"/> command
                        ///     to another object. If the object is a <see cref="TalkReceiver"/>
                        ///     then it will know how to handle such command.
                        /// </summary>
                        [RequireComponent(typeof(CloseCommandSender))]
                        public class TalkSender : MonoBehaviour
                        {
                            /// <summary>
                            ///   The "talk" command.
                            /// </summary>
                            public const string COMMAND = "WR:Talk";

                            private CloseCommandSender sender;
                            private void Start()
                            {
                                sender = GetComponent<CloseCommandSender>();
                            }

                            /// <summary>
                            ///   Sends a talk command. If an object is adjacent and has
                            ///     <see cref="TalkReceiver"/>, then it will be able to
                            ///     handle such command.
                            /// </summary>
                            public void Talk()
                            {
                                sender.Cast("WR:Talk");
                            }
                        }
                    }
                }
            }
        }
    }
}