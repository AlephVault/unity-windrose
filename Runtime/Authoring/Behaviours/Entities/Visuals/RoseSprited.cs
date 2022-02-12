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
                    ///   Handles the object's sprite rendering, given four sprites. From those four
                    ///     sprites, it will pick the appropriate for the current direction. The
                    ///     fetched sprite will be given to its related <see cref="SpriteRenderer"/>
                    ///     component.
                    /// </summary>
                    public class RoseSprited : VisualBehaviour
                    {
                        private SpriteRenderer spriteRenderer;
                        
                        /// <summary>
                        ///   The default sprite rose, for when no other sprite is given.
                        /// </summary>
                        [SerializeField]
                        private ScriptableObjects.VisualResources.SpriteRose defaultSpriteRose;

                        /// <summary>
                        ///   The current orientation. Different behaviours will set this value
                        ///     in different moments, likely related to the <see cref="MapObject.Oriented"/>
                        ///     behaviour subscription.
                        /// </summary>
                        private Types.Direction orientation = Types.Direction.FRONT;

                        // Track the current state to not update unnecessarily the sprite later.
                        private ScriptableObjects.VisualResources.SpriteRose spriteRose;

                        // Refreshes the underlying sprite.
                        private void RefreshSprite()
                        {
                            spriteRenderer.sprite = spriteRose.GetForDirection(orientation);
                        }

                        /// <summary>
                        ///   Gets or sets the current sprite rose, and updates the sprite (on set).
                        /// </summary>
                        public ScriptableObjects.VisualResources.SpriteRose SpriteRose
                        {
                            get { return spriteRose; }
                            set
                            {
                                if (spriteRose != value)
                                {
                                    spriteRose = value;
                                    RefreshSprite();
                                }
                            }
                        }

                        /// <summary>
                        ///   Sets the current sprite rose to the default one.
                        /// </summary>
                        public void SetDefaultSpriteRose()
                        {
                            SpriteRose = defaultSpriteRose;
                        }

                        protected override void Awake()
                        {
                            base.Awake();
                            spriteRenderer = GetComponent<SpriteRenderer>();
                        }

                        private Objects.MapObject relatedObject;

                        private void OrientationChanged(Types.Direction newOrientation)
                        {
                            orientation = newOrientation;
                            // Remember that OnEnable* may be triggered before Start(). In such
                            //   scenario, spriteRose will not be set. In such case, we ignore
                            //   this call.
                            // (*OnEnable triggers this function)
                            if (spriteRose) RefreshSprite();
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
                            SetDefaultSpriteRose();
                        }
                    }
                }
            }
        }
    }
}
