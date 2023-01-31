using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using AlephVault.Unity.Support.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Darkness
                    {
                        /// <summary>
                        ///   The topmost layer (if present, and no custom-type
                        ///     layers exist). It will hold <see cref="Ceilings.Ceiling"/>
                        ///     objects inside.
                        /// </summary>
                        [RequireComponent(typeof(Grid))]
                        public class DarknessLayer : MapLayer
                        {
                            /// <summary>
                            ///   The transient darkness object. Typically,
                            ///   this one is set at editor time.
                            /// </summary>
                            [SerializeField]
                            private SpriteRenderer transientDarkness;

                            /// <summary>
                            ///   The fixed darkness object. Typically,
                            ///   this one is set at editor time.
                            /// </summary>
                            [SerializeField]
                            private SpriteRenderer fixedDarkness;

                            /// <summary>
                            ///   The group holding the transient darkness object
                            ///   and the masks.
                            /// </summary>
                            [SerializeField]
                            private SortingGroup masksGroup;

                            /// <summary>
                            ///   The sprite to use for the darkness.
                            ///   Typically, this one is set on editor time.
                            /// </summary>
                            [SerializeField]
                            private Sprite darknessSprite;

                            /// <summary>
                            ///   A value between 0 and 1 (both inclusive)
                            ///   telling how efficient are the lights in
                            ///   this darkness. Essentially, it will tell
                            ///   how dark is the fixed darkness. This one
                            ///   will typically go around 1.
                            /// </summary>
                            [SerializeField]
                            private float lightEfficiency = 1f;

                            /// <summary>
                            ///   A value telling the added darkness opacity
                            ///   for when no lights are turned on in given
                            ///   points of this layer.
                            /// </summary>
                            [SerializeField]
                            private float unlitOpacity = 0.75f;

                            /// <summary>
                            ///   See <see cref="masksGroup" />.
                            /// </summary>
                            public SortingGroup MasksGroup => masksGroup;

                            /// <summary>
                            ///    See <see cref="lightEfficiency" />.
                            /// </summary>
                            public float LightEfficiency
                            {
                                get => lightEfficiency;
                                set
                                {
                                    lightEfficiency = Values.Clamp(0, value, 1);
                                    if (fixedDarkness)
                                    {
                                        fixedDarkness.color = new Color(0, 0, 0, 1 - lightEfficiency);
                                    }
                                }
                            }

                            /// <summary>
                            ///   See <see cref="unlitOpacity" />.
                            /// </summary>
                            public float UnlitOpacity
                            {
                                get => unlitOpacity;
                                set
                                {
                                    unlitOpacity = Values.Clamp(0, value, 1);
                                    if (transientDarkness)
                                    {
                                        transientDarkness.color = new Color(0, 0, 0, unlitOpacity);
                                    }
                                }
                            }

                            private SpriteRenderer MakeDarkness(string objName)
                            {
                                GameObject darknessObj = new GameObject(objName);
                                darknessObj.transform.SetParent(transform);
                                SpriteRenderer darkness = darknessObj.AddComponent<SpriteRenderer>();
                                darkness.sprite = darknessSprite;
                                darkness.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                                darknessObj.transform.localPosition = Vector3.zero;
                                darknessObj.transform.localScale = new Vector3(
                                    Map.Width * Map.CellSize.x, 
                                    Map.Height * Map.CellSize.y, 
                                    1
                                );
                                return darkness;
                            }
                            
                            protected override int GetSortingOrder()
                            {
                                return 60;
                            }

                            protected override void Awake()
                            {
                                base.Awake();
                                lightEfficiency = Values.Clamp(0, lightEfficiency, 1);
                                unlitOpacity = Values.Clamp(0, unlitOpacity, 1);
                                masksGroup ??= (
                                    from comp in GetComponentsInChildren<SortingGroup>()
                                    where comp.gameObject != gameObject
                                    select comp
                                ).FirstOrDefault();
                                if (!masksGroup) {
                                    GameObject groupObj = new GameObject("Group");
                                    masksGroup = groupObj.AddComponent<SortingGroup>();
                                    groupObj.transform.SetParent(transform);
                                    groupObj.transform.localPosition = Vector3.zero;
                                    groupObj.transform.localScale = Vector3.one;
                                }

                                if (!transientDarkness)
                                {
                                    transientDarkness = MakeDarkness("TransientDarkness");
                                }
                                transientDarkness.transform.SetParent(masksGroup.transform);
                                transientDarkness.color = new Color(0, 0, 0, unlitOpacity);
                                if (!fixedDarkness)
                                {
                                    fixedDarkness = MakeDarkness("FixedDarkness");
                                }
                                fixedDarkness.transform.SetParent(transform);
                                fixedDarkness.color = new Color(0, 0, 0, 1 - lightEfficiency);

                                transientDarkness.sortingOrder = 0;
                                transientDarkness.sortingLayerID = 0;
                                fixedDarkness.sortingOrder = 0;
                                fixedDarkness.sortingLayerID = 0;
                            }

                            protected void OnEnable()
                            {
                                transientDarkness.enabled = true;
                                fixedDarkness.enabled = true;
                            }

                            protected void OnDisable()
                            {
                                transientDarkness.enabled = false;
                                fixedDarkness.enabled = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
