﻿using System;
using System.Collections.Generic;
using System.Linq;
using AlephVault.Unity.Support.Utils;
using UnityEngine;
using UnityEngine.Events;

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
                    ///   A visual is the basic visual component to be rendered
                    ///     that is related to in-map objects. Visuals are intended
                    ///     to be awaken from inside <see cref="Objects.MapObject"/>
                    ///     in-map objects, but they are unparented from them and
                    ///     added into the <see cref="World.Layers.Visuals.VisualsLayer"/>
                    ///     when the object is attached to a <see cref="World.Map"/>. More
                    ///     behaviours (involved with sprites and animations) will be
                    ///     added on top of this one.
                    /// </summary>
                    [RequireComponent(typeof(Common.Pausable))]
                    [RequireComponent(typeof(Renderer))]
                    public class Visual : MonoBehaviour
                    {
                        private Renderer renderer;

                        /// <summary>
                        ///   Tells whether this visual is the main visual of an
                        ///     in-map object. This one will be set only once.
                        /// </summary>
                        public bool IsMain { get; private set; }

                        /// <summary>
                        ///   The depth level of this visual. This will rarely change.
                        /// </summary>
                        [SerializeField]
                        private ushort level = 1 << 14;

                        /// <summary>
                        ///   The priority (in-same-depth rendering preference) of the visual.
                        /// </summary>
                        [SerializeField]
                        private byte priority = 0;

                        /// <summary>
                        ///   See <see cref="level"/>.
                        /// </summary>
                        public ushort Level
                        {
                            get
                            {
                                return level;
                            }
                            set
                            {
                                level = value;
                                Resort();
                            }
                        }

                        public byte Priority
                        {
                            get
                            {
                                return priority;
                            }
                            set
                            {
                                priority = Values.Min<byte>(7, priority);
                                Resort();
                            }
                        }

                        /// <summary>
                        ///   The object this visual is attached to. This object is set on
                        ///     runtime, either on awake or when explicitly attached to an
                        ///     object.
                        /// </summary>
                        private Objects.MapObject relatedObject;

                        /// <summary>
                        ///   See <see cref="relatedObject"/>.
                        /// </summary>
                        public Objects.MapObject RelatedObject { get { return relatedObject; } }

                        /// <summary>
                        ///   An optional offset to its related object.
                        /// </summary>
                        public Vector3 RelatedObjectOffset;

                        /// <summary>
                        ///   Tracks the current status of the visibility.
                        /// </summary>
                        private bool visibilityEnabled = false;

                        private void OnEnable()
                        {
                            UpdateVisibilityStatus();
                        }

                        private void OnDisable()
                        {
                            UpdateVisibilityStatus();
                        }

                        // These are handlers for the owner object's events - they will perform simple updates
                        private void OwnerOnAttached(World.Map map) { UpdateVisibilityStatus(); }
                        private void OwnerOnDetached() { UpdateVisibilityStatus(); }
                        private void OwnerOnTeleported(ushort x, ushort y) { Resort(); }
                        private void OwnerMandatoryDirectionEvent(Types.Direction direction) { Resort(); }
                        private void OwnerOptionalDirectionEvent(Types.Direction? direction) { Resort(); }
                        private void OwnerPropertyUpdated(Objects.Strategies.ObjectStrategy strategy, string p, object o, object n) { Resort(); }

                        /// <summary>
                        ///   Invoked by new owner <see cref="Objects.MapObject"/> when attaching
                        ///     this visual. Attaches handlers to all the events.
                        /// </summary>
                        /// <param name="owner">The new owner</param>
                        public void OnAttached(Objects.MapObject owner)
                        {
                            relatedObject = owner;
                            owner.onAttached.AddListener(OwnerOnAttached);
                            owner.onDetached.AddListener(OwnerOnDetached);
                            owner.onTeleported.AddListener(OwnerOnTeleported);
                            owner.onMovementStarted.AddListener(OwnerMandatoryDirectionEvent);
                            owner.onMovementFinished.AddListener(OwnerMandatoryDirectionEvent);
                            owner.onMovementCancelled.AddListener(OwnerOptionalDirectionEvent);
                            owner.onStrategyPropertyUpdated.AddListener(OwnerPropertyUpdated);
                            UpdateVisibilityStatus();
                            Resort();
                        }

                        /// <summary>
                        ///   Invoked by former owner <see cref="Objects.MapObject"/> when detaching
                        ///     this visual.
                        /// </summary>
                        /// <param name="owner">The former owner</param>
                        public void OnDetached(Objects.MapObject formerOwner)
                        {
                            relatedObject = null;
                            formerOwner.onAttached.RemoveListener(OwnerOnAttached);
                            formerOwner.onDetached.RemoveListener(OwnerOnDetached);
                            formerOwner.onTeleported.RemoveListener(OwnerOnTeleported);
                            formerOwner.onMovementStarted.RemoveListener(OwnerMandatoryDirectionEvent);
                            formerOwner.onMovementFinished.RemoveListener(OwnerMandatoryDirectionEvent);
                            formerOwner.onMovementCancelled.RemoveListener(OwnerOptionalDirectionEvent);
                            formerOwner.onStrategyPropertyUpdated.RemoveListener(OwnerPropertyUpdated);
                            UpdateVisibilityStatus();
                        }

                        private void UpdateVisibilityStatus()
                        {
                            bool newVisibilityStatus = gameObject.activeInHierarchy && enabled && relatedObject && relatedObject.ParentMap;
                            if (newVisibilityStatus != visibilityEnabled)
                            {
                                visibilityEnabled = newVisibilityStatus;
                                if (visibilityEnabled)
                                {
                                    renderer.enabled = true;
                                    Resort();
                                    foreach (VisualBehaviour behaviour in GetComponents<VisualBehaviour>())
                                    {
                                        behaviour.enabled = true;
                                    }
                                }
                                else
                                {
                                    renderer.enabled = false;
                                    foreach (VisualBehaviour behaviour in GetComponents<VisualBehaviour>())
                                    {
                                        behaviour.enabled = false;
                                    }
                                }
                            }
                        }

                        private void Resort()
                        {
                            if (level > 32767) level = 32767;
                            if (priority > 7) priority = 7;
                            if (visibilityEnabled)
                            {
                                // give sorting order just by Y position, and give perspective layer according to level
                                Transform transform1 = transform;
                                transform1.SetParent(relatedObject.ParentMap.VisualsLayer[level].transform);
                                transform1.localRotation = Quaternion.identity;
                                renderer.sortingOrder = (relatedObject.ParentMap.Height - relatedObject.Y - 1) * 8 + priority;
                            }
                        }

                        private List<VisualBehaviour> visualBehaviours = new List<VisualBehaviour>();

                        public void DoUpdate()
                        {
                            if (visibilityEnabled)
                            {
                                transform.localPosition = relatedObject.transform.localPosition + RelatedObjectOffset;
                                foreach (VisualBehaviour behaviour in visualBehaviours)
                                {
                                    behaviour.DoUpdate();
                                }
                            }
                        }

                        public void DoStart()
                        {
                            foreach (VisualBehaviour behaviour in visualBehaviours)
                            {
                                behaviour.DoStart();
                            }
                        }

                        private void Awake()
                        {
                            relatedObject = transform.parent ? transform.parent.GetComponent<Objects.MapObject>() : null;
                            if (relatedObject.MainVisual == this) IsMain = true;
                            renderer = GetComponent<Renderer>();
                            UpdateVisibilityStatus();
                            foreach (VisualBehaviour behaviour in GetComponents<VisualBehaviour>())
                            {
                                visualBehaviours.Add(behaviour);
                            }
                        }

                        /// <summary>
                        ///   Detaches this visual from its current owner.
                        /// </summary>
                        public void Detach()
                        {
                            if (relatedObject) relatedObject.PopVisual(this);
                        }
                    }
                }
            }
        }
    }
}
