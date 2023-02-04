using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World.Layers.Darkness;
using UnityEngine;
using UnityEngine.Rendering;

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
                    namespace Darkness
                    {
                        public class LightMask : VisualBehaviour
                        {
                            /// <summary>
                            ///   The offset of the mask.
                            /// </summary>
                            [SerializeField]
                            private Vector3 offset = new Vector3(0.5f, 0.5f, 0);
                            
                            /// <summary>
                            ///   The related sprite mask.
                            /// </summary>
                            [SerializeField]
                            private SpriteMask mask;

                            /// <summary>
                            ///   The mask size.
                            /// </summary>
                            private Vector3 maskScale;

                            /// <summary>
                            ///   The related sprite shape.
                            /// </summary>
                            [SerializeField]
                            private Sprite maskShape;

                            protected override void Awake()
                            {
                                base.Awake();
                                GameObject maskObj = new GameObject("Mask");
                                if (!mask)
                                {
                                    mask = maskObj.AddComponent<SpriteMask>();
                                    mask.sprite = maskShape;
                                    mask.transform.localScale = new Vector3(maskScale.x, maskScale.y, 1);
                                    mask.backSortingOrder = 1;
                                    mask.frontSortingOrder = 1;
                                    mask.backSortingLayerID = 0;
                                }
                            }

                            protected void OnEnable()
                            {
                                DarknessLayer darkness = visual.RelatedObject.ParentMap.DarknessLayer;
                                SortingGroup masksGroup = darkness ? darkness.MasksGroup : null;
                                Transform masksTransform = masksGroup ? masksGroup.transform : null;
                                if (masksTransform)
                                {
                                    mask.gameObject.SetActive(true);
                                    mask.transform.SetParent(masksTransform);                                    
                                }
                                else
                                {
                                    mask.gameObject.SetActive(false);
                                }
                            }

                            protected void OnDisable()
                            {
                                mask.gameObject.SetActive(false);
                            }

                            public override void DoUpdate()
                            {
                                if (enabled)
                                {
                                    mask.transform.localPosition = transform.localPosition + offset;
                                }
                            }

                            /// <summary>
                            ///   Tells whether the light is turned on or not.
                            /// </summary>
                            public bool IsTurnedOn
                            {
                                get => mask && mask.enabled;
                                set
                                {
                                    if (mask) mask.enabled = value;
                                }
                            }

                            private void OnDestroy()
                            {
                                Destroy(mask);
                            }
                        }
                    }
                }
            }
        }
    }
}
