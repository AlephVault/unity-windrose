using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Strategies
                {
                    namespace Solidness
                    {
                        using World.Layers.Objects.ObjectsManagementStrategies.Solidness;

                        /// <summary>
                        ///   Solidness strategy keeps the solidness state of this
                        ///     object. By default, it will be solid. See the
                        ///     <see cref="Solidness"/> property and
                        ///     <see cref="solidness"/> field for more details.
                        /// </summary>
                        public class SolidnessObjectStrategy : ObjectStrategy
                        {
                            /// <summary>
                            ///   A constant for the <see cref="solidness"/> property,
                            ///   to be mapped for strategies.
                            /// </summary>
                            public static readonly string SolidnessProperty = FullyQualifiedProperty<SolidnessObjectStrategy>("solidness");

                            /// <summary>
                            ///   A constant for the <see cref="mask"/> property,
                            ///   to be mapped for strategies.
                            /// </summary>
                            public static readonly string MaskProperty = FullyQualifiedProperty<SolidnessObjectStrategy>("mask");

                            /// <summary>
                            ///   A constant for the <see cref="TraversesOtherSolidsProperty"/> property,
                            ///   to be mapped for strategies.
                            /// </summary>
                            public static readonly string TraversesOtherSolidsProperty = FullyQualifiedProperty<SolidnessObjectStrategy>("traversesOtherSolids");

                            /// <summary>
                            ///   The object's solidness status.
                            /// </summary>
                            [SerializeField]
                            private SolidnessStatus solidness = SolidnessStatus.Solid;

                            /// <summary>
                            ///   Tells whether the object, being solid, can traverse
                            ///     other solids. This property is only meaningful when
                            ///     the current <see cref="solidness"/> is
                            ///     <see cref="SolidnessStatus.Solid"/>.
                            /// </summary>
                            [SerializeField]
                            private bool traversesOtherSolids = false;

                            /// <summary>
                            ///   The solidness mask for this object. Only meaningful
                            ///     when using <see cref="SolidnessStatus.Mask"/> status. 
                            /// </summary>
                            [SolidObjectMask.AutoClamped]
                            [SerializeField]
                            private SolidObjectMask mask;

                            // Tells whether this object is also a TriggerPlatform.
                            private bool isPlatform;

                            private void ClampSolidness()
                            {
                                if (isPlatform && solidness != SolidnessStatus.Ghost && solidness != SolidnessStatus.Hole)
                                {
                                    solidness = SolidnessStatus.Ghost;
                                }
                            }

                            protected override void Awake()
                            {
                                base.Awake();

                                // Init & Clamp the just-deserialized mask.
                                if (mask == null)
                                {
                                    mask = new SolidObjectMask();
                                }
                                if (mask.Width != Object.Width || mask.Height != Object.Height)
                                {
                                    mask = mask.Resized(Object.Width, Object.Height);
                                }
                            }

                            /// <summary>
                            ///   Upon initialization, if this object is also a <see cref="TriggerPlatform"/>
                            ///     then the solidness will be changed to <see cref="SolidnessStatus.Ghost"/> unless
                            ///     it is <see cref="SolidnessStatus.Hole"/>.
                            /// </summary>
                            public override void Initialize()
                            {
                                TriggerPlatform triggerPlatform = StrategyHolder.GetComponent<TriggerPlatform>();
                                isPlatform = triggerPlatform != null;
                                ClampSolidness();
                            }

                            /// <summary>
                            ///   Its counterpart strategy is <see cref="SolidnessObjectsManagementStrategy"/>.
                            /// </summary>
                            /// <returns>The counterpart type</returns>
                            protected override Type GetCounterpartType()
                            {
                                return typeof(SolidnessObjectsManagementStrategy);
                            }

                            /// <summary>
                            ///   <para>
                            ///     The object's solidness status property. It will notify
                            ///       the counterpart strategy upon value change.
                            ///   </para>
                            ///   <para>
                            ///     If the object is a platform, the solidness will only be allowed
                            ///       to be <see cref="SolidnessStatus.Ghost"/> or
                            ///       <see cref="SolidnessStatus.Hole"/>.
                            ///   </para>
                            /// </summary>
                            public SolidnessStatus Solidness
                            {
                                get { return solidness; }
                                set
                                {
                                    var oldValue = solidness;
                                    solidness = value;
                                    ClampSolidness();
                                    if (oldValue != solidness) PropertyWasUpdated(SolidnessProperty, oldValue, solidness);
                                }
                            }

                            /// <summary>
                            ///   Gets (a clone of) the current mask / changes the current mask.
                            ///   The new mask will be clamped and filled (with "ghost" values)
                            ///     appropriately.
                            /// </summary>
                            public SolidObjectMask Mask
                            {
                                get { return mask; }
                                set
                                {
                                    if (mask == value) return;
                                    var oldValue = mask;
                                    if (value.Width != Object.Width || value.Height != Object.Height)
                                    {
                                        mask = value.Resized(Object.Width, Object.Height);
                                    }
                                    else
                                    {
                                        mask = value;
                                    }
                                    PropertyWasUpdated(MaskProperty, oldValue, mask);
                                }
                            }

                            /// <summary>
                            ///   The object's traversal flag for solid/mask state. See <see cref="traversesOtherSolids"/>.
                            ///   It will notify the counterpart strategy upon value change.
                            /// </summary>
                            public bool TraversesOtherSolids
                            {
                                get { return traversesOtherSolids; }
                                set
                                {
                                    var oldValue = traversesOtherSolids;
                                    traversesOtherSolids = value;
                                    if (oldValue != traversesOtherSolids) PropertyWasUpdated(TraversesOtherSolidsProperty, oldValue, traversesOtherSolids);
                                }
                            }
                        }

#if UNITY_EDITOR
                        [CustomEditor(typeof(SolidnessObjectStrategy))]
                        [CanEditMultipleObjects]
                        class SolidnessObjectStrategyEditor : Editor
                        {
                            SerializedProperty solidness;
                            SerializedProperty traversesOtherSolids;
                            SerializedProperty mask;

                            private void OnEnable()
                            {
                                solidness = serializedObject.FindProperty("solidness");
                                traversesOtherSolids = serializedObject.FindProperty("traversesOtherSolids");
                                mask = serializedObject.FindProperty("mask");
                            }

                            public override void OnInspectorGUI()
                            {
                                serializedObject.Update();

                                EditorGUILayout.PropertyField(solidness);
                                SolidnessStatus solidnessValue = (SolidnessStatus)Enum.GetValues(typeof(SolidnessStatus)).GetValue(solidness.enumValueIndex);
                                if (solidnessValue == SolidnessStatus.Solid) EditorGUILayout.PropertyField(traversesOtherSolids);
                                if (solidnessValue == SolidnessStatus.Mask) EditorGUILayout.PropertyField(mask);

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
#endif
                    }
                }
            }
        }
    }
}
