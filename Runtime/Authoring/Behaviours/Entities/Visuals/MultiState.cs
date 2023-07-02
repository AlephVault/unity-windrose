using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlephVault.Unity.WindRose.Types;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    /// <summary>
                    ///   <para>
                    ///     Holds several states under a dictionary. States are intended to be visual, and
                    ///       subclasses will implement them (usually as animations, animation roses, or
                    ///       static images). Since they are only intended to be visual, only one instance
                    ///       of MultiState should be used and their appropriate context should be taken
                    ///       into account (e.g. cannot add a static multi while Animated is present, or
                    ///       cannot add rose-animated while animated multi is present,...).
                    ///   </para>
                    /// </summary>
                    public abstract class MultiState<StateType> : VisualBehaviour
                    {
                        /***************** Core data elements *****************/

                        // The default (idle) state.
                        private static readonly State IDLE_STATE = State.Get("");

                        /// <summary>
                        ///   Tells when an error is raised inside multi-state component methods.
                        /// </summary>
                        public class Exception : Types.Exception
                        {
                            public Exception() { }
                            public Exception(string message) : base(message) { }
                            public Exception(string message, System.Exception inner) : base(message, inner) { }
                        }

                        // All the registered states
                        private Dictionary<State, StateType> states = new Dictionary<State, StateType>();
                        
                        /***************** Identity *****************/

                        // The map object it is tied to
                        private Objects.MapObject mapObject;

                        /***************** Data elements *****************/

                        /// <summary>
                        ///   The default state (used for idle state, and usually meaning
                        ///     a resting or stand-up position).
                        /// </summary>
                        [SerializeField]
                        private StateType idleState;

                        /// <summary>
                        ///   The state being rendered. It will be grabbed from
                        ///     the <see cref="Objects.MapObject"/> component.
                        /// </summary>
                        private State selectedState = IDLE_STATE;

                        // Uses the appropriate state being selected. If something goes wrong,
                        //   the exception will be absorbed, a warning will be issued, and
                        //   the idle state will be set.
                        private void RefreshState()
                        {
                            try
                            {
                                StateType state;
                                if (states.TryGetValue(selectedState, out state))
                                {
                                    UseState(state);
                                }
                            }
                            catch (KeyNotFoundException)
                            {
                                // Key IDLE will always be available
                                selectedState = IDLE_STATE;
                            }
                        }

                        protected abstract void UseState(StateType state);

                        /// <summary>
                        ///   Tells whether this object defines a particular state.
                        /// </summary>
                        /// <param name="state">The state to check</param>
                        /// <returns>Whether it is defined or not in this object</returns>
                        public bool HasState(State state)
                        {
                            return states.ContainsKey(state);
                        }

                        /// <summary>
                        ///   Registers an state under a key. This is usually done when initializing
                        ///     other components (e.g. moving components).
                        /// </summary>
                        /// <param name="state">The state to use</param>
                        /// <param name="stateValue">The value to register</param>
                        public void AddState(State state, StateType stateValue)
                        {
                            if (states.ContainsKey(state))
                            {
                                throw new Exception("State already in use: " + state);
                            }
                            else
                            {
                                states.Add(state, stateValue);
                            }
                        }

                        /// <summary>
                        ///   Replaces an existing state with a new one. This is run at run-time
                        ///     and will require the state being replaced to exist, or fail otherwise.
                        /// </summary>
                        /// <param name="state">The state being replaced</param>
                        /// <param name="stateValue">The new state to use</param>
                        public void ReplaceState(State state, StateType stateValue)
                        {
                            if (!states.ContainsKey(state))
                            {
                                throw new Exception("State key does not exist: " + state);
                            }
                            else
                            {
                                states[state] = stateValue;
                                RefreshState();
                            }
                        }
                        
                        // Updates the currently selected key
                        private void OnSelectedKeyChanged(State newKey)
                        {
                            if (newKey != selectedState)
                            {
                                selectedState = newKey;
                                RefreshState();
                            }
                        }

                        // On enabled, takes the related object and binds the event
                        private void OnEnable()
                        {
                            mapObject = visual.RelatedObject;
                            if (mapObject)
                            {
                                mapObject.OnStateChanged.AddListener(OnSelectedKeyChanged);
                            }
                        }

                        // On disabled, releases the event and the related object
                        private void OnDisable()
                        {
                            if (mapObject)
                            {
                                mapObject.OnStateChanged.RemoveListener(OnSelectedKeyChanged);
                            }
                            mapObject = null;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            // Ensures at least the idle state exists
                            AddState(IDLE_STATE, idleState);
                        }
                    }
                }
            }
        }
    }
}
 
 