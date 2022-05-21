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
                    ///   Handles the object's ability to animate, given a sequence of sprites.
                    /// </summary>
                    [RequireComponent(typeof(SpriteRenderer))]
                    public class Animated : VisualBehaviour, Common.Pausable.IPausable
                    {
                        private SpriteRenderer spriteRenderer;

                        /// <summary>
                        ///   The default animation, for when no other animation is given.
                        /// </summary>
                        [SerializeField]
                        private ScriptableObjects.VisualResources.Animation defaultAnimation;

                        /**
                         * Stuff to handle and render the current animation.
                         */

                        private new ScriptableObjects.VisualResources.Animation animation;
                        private float currentTime;
                        private float frameInterval;
                        private int currentAnimationIndex;
                        private bool paused;

                        /// <summary>
                        ///   Gets or sets the current animation, and resets it (on set).
                        /// </summary>
                        public ScriptableObjects.VisualResources.Animation Animation
                        {
                            get { return animation; }
                            set
                            {
                                if (animation != value)
                                {
                                    animation = value;
                                    if (animation) Reset();
                                }
                            }
                        }

                        /// <summary>
                        ///   Sets the current animation to the default one.
                        /// </summary>
                        public void SetDefaultAnimation()
                        {
                            Animation = defaultAnimation;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            spriteRenderer = GetComponent<SpriteRenderer>();
                            spriteRenderer.enabled = false;
                            paused = false;
                        }

                        private void Reset()
                        {
                            currentTime = 0;
                            currentAnimationIndex = 0;
                            frameInterval = animation != null ? (animation.FPS != 0) ? 1.0f / animation.FPS : float.MaxValue : 0;
                        }

                        /// <summary>
                        ///   Triggered when the underlying visual is started.
                        ///   Ensures the default animation to be selected.
                        /// </summary>
                        public override void DoStart()
                        {
                            SetDefaultAnimation();
                        }

                        /// <summary>
                        ///   Updates the current image. To be invoked, in different moments, by
                        ///     the different subclasses.
                        /// </summary>
                        private void Frame()
                        {
                            // If no animation is set (i.e. null) or the animation
                            // is empty, then do nothing.
                            if (animation == null || animation.Sprites.Length == 0)
                            {
                                if (spriteRenderer.enabled) spriteRenderer.enabled = false;
                                return;
                            }
                            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;
                            
                            currentTime += Time.deltaTime;
                            if (currentTime > frameInterval)
                            {
                                currentTime -= frameInterval;
                                currentAnimationIndex = ((currentAnimationIndex + 1) % Animation.Sprites.Length);
                            }
                            spriteRenderer.sprite = Animation.Sprites[currentAnimationIndex];
                        }

                        /// <summary>
                        ///   Triggered when the underlying visual is updated. Updating this behaviour involves
                        ///     ensuring appropriate frame in the animation.
                        /// </summary>
                        public override void DoUpdate()
                        {
                            if (!paused) Frame();
                        }

                        public void Pause(bool fullFreeze)
                        {
                            paused = fullFreeze;
                        }

                        public void Resume()
                        {
                            paused = false;
                        }
                    }
                }
            }
        }
    }
}