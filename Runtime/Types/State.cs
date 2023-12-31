using System;
using System.Collections.Generic;

namespace AlephVault.Unity.WindRose
{
    namespace Types
    {
        /// <summary>
        ///   State wrappers, which are also weak-referenced
        ///   to avoid comparing strings.
        /// </summary>
        public class State
        {
            // The list of states to use.
            private static Dictionary<string, WeakReference<State>> states = new Dictionary<string, WeakReference<State>>();

            // The state name for this object.
            private string stateName;

            private State(string state)
            {
                stateName = state;
            }

            public override string ToString()
            {
                return stateName;
            }

            public static State Get(string state)
            {
                if (states.TryGetValue(state, out WeakReference<State> value) && value.TryGetTarget(out State target))
                {
                    return target;
                }
                else
                {
                    State newState = new State(state);
                    states[state] = new WeakReference<State>(newState);
                    return newState;
                }
            }

            ~State()
            {
                states.Remove(stateName);
            }
        }
    }
}