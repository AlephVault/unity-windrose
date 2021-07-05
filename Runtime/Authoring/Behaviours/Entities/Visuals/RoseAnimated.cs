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
                    /// <summary>
                    ///   Handles the object's ability to animate, given four sequences of sprites.
                    ///   From those four sequences, it will pick the appropriate for the current
                    ///     direction, which is a behaviour to be implemented. The fetched animation
                    ///     will be given to its related <see cref="Animated"/> component.
                    /// </summary>
                    [RequireComponent(typeof(Animated))]
                    public class RoseAnimated : VisualBehaviour
                    {
                        private Animated animated;

                        /// <summary>
                        ///   The default animation rose, for when no other animation is given.
                        /// </summary>
                        [SerializeField]
                        private ScriptableObjects.Animations.AnimationRose defaultAnimationRose;

                        /// <summary>
                        ///   The current orientation. Different behaviours will set this value
                        ///     in different moments, likely related to the <see cref="MapObject.Oriented"/>
                        ///     behaviour subscription.
                        /// </summary>
                        private Types.Direction orientation = Types.Direction.FRONT;

                        // Track the current state to not update unnecessarily the animation later.
                        private ScriptableObjects.Animations.AnimationRose animationRose;

                        // Refreshes the underlying animation.
                        private void RefreshAnimation()
                        {
                            animated.Animation = animationRose.GetForDirection(orientation);
                        }

                        /// <summary>
                        ///   Gets or sets the current animation rose, and updates the animation (on set).
                        /// </summary>
                        public ScriptableObjects.Animations.AnimationRose AnimationRose
                        {
                            get { return animationRose; }
                            set
                            {
                                if (animationRose != value)
                                {
                                    animationRose = value;
                                    RefreshAnimation();
                                }
                            }
                        }

                        /// <summary>
                        ///   Sets the current animation rose to the default one.
                        /// </summary>
                        public void SetDefaultAnimationRose()
                        {
                            AnimationRose = defaultAnimationRose;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            animated = GetComponent<Animated>();
                        }

                        private Objects.MapObject relatedObject;

                        private void OrientationChanged(Types.Direction newOrientation)
                        {
                            orientation = newOrientation;
                            // Remember that OnEnable* may be triggered before Start(). In such
                            //   scenario, animationRose will not be set. In such case, we
                            //   ignore this call.
                            // (*OnEnable triggers this function)
                            if (animationRose) RefreshAnimation();
                        }

                        private void OnEnable()
                        {
                            relatedObject = visual.RelatedObject;
                            if (relatedObject)
                            {
                                relatedObject.onOrientationChanged.AddListener(OrientationChanged);
                                // After setting the event, force the call.
                                OrientationChanged(relatedObject.Orientation);
                            }
                        }

                        private void OnDisable()
                        {
                            if (relatedObject) relatedObject.onOrientationChanged.RemoveListener(OrientationChanged);
                        }

                        /// <summary>
                        ///   Triggered when the underlying visual is started.
                        ///   Ensures the default animation rose to be selected.
                        /// </summary>
                        public override void DoStart()
                        {
                            SetDefaultAnimationRose();
                        }
                    }
                }
            }
        }
    }
}
