using System.Collections;
using System.Collections.Generic;
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
                    ///   Visual behaviours have a special contract that is related
                    ///     to its visual management only. The contract involves
                    ///     one method that are documented: <see cref="DoUpdate"/>.
                    /// </summary>
                    [RequireComponent(typeof(Visual))]
                    public abstract class VisualBehaviour : MonoBehaviour
                    {
                        /// <summary>
                        ///   The visual this component is attached to.
                        /// </summary>
                        protected Visual visual;

                        /// <summary>
                        ///   Triggered when the underlying visual is updated.
                        /// </summary>v
                        public virtual void DoUpdate() { }

                        /// <summary>
                        ///   Triggered when the underlying visual is started.
                        ///   This method should update data not related to what occurs in OnEnabled.
                        /// </summary>
                        public virtual void DoStart() { }

                        protected virtual void Awake()
                        {
                            visual = GetComponent<Visual>();
                            enabled = false;
                        }
                    }
                }
            }
        }
    }
}
