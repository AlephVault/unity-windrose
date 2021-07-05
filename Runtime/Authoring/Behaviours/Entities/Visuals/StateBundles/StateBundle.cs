using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    namespace StateBundles
                    {
                        /// <summary>
                        ///   Provides, in edit time, a way to add stuff to the underlying
                        ///     <see cref="MultiState{StateType}"/> component. 
                        /// </summary>
                        public abstract class StateBundle<StateType> : MonoBehaviour
                        {
                            /// <summary>
                            ///   The state key to use - fixed per subclass.
                            /// </summary>
                            /// <returns>The state key to use</returns>
                            protected abstract string GetStateKey();

                            /// <summary>
                            ///   The state value to use.
                            /// </summary>
                            [SerializeField]
                            private StateType value;

                            /// <summary>
                            ///   The fallback to use, if value is null.
                            /// </summary>
                            [SerializeField]
                            private string fallback;

                            private void Awake()
                            {
                                if (value != null)
                                {
                                    GetComponent<MultiState<StateType>>().AddState(GetStateKey(), value);
                                }
                                else
                                {
                                    GetComponent<MultiState<StateType>>().AddFallback(GetStateKey(), fallback);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}