using System.Collections;
using System.Collections.Generic;
using GameMeanMachine.Unity.WindRose.Types;
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
                        public abstract class StateBundle<StateType> : MonoBehaviour where StateType : class
                        {
                            /// <summary>
                            ///   The state to use - fixed per subclass.
                            /// </summary>
                            /// <returns>The state key to use</returns>
                            protected abstract State GetState();

                            /// <summary>
                            ///   The state value to use.
                            /// </summary>
                            [SerializeField]
                            private StateType value;

                            private void Awake()
                            {
                                GetComponent<MultiState<StateType>>().AddState(GetState(), value);
                            }
                        }
                    }
                }
            }
        }
    }
}