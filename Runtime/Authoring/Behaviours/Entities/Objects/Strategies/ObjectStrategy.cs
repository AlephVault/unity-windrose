using System;
using System.Linq;
using UnityEngine;
using AlephVault.Unity.Support.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                namespace Strategies
                {
                    /// <summary>
                    ///   <para>
                    ///     Object strategies are the counterpart of <see cref="World.Layers.Objects.ObjectsManagementStrategies.ObjectsManagementStrategy"/>,
                    ///       and will reside in the same object holding an <see cref="ObjectStrategyHolder"/>.
                    ///   </para>
                    ///   <para>
                    ///     Quite often they hold state instead of logic, and notify the counterpart management strategy in the map by invoking
                    ///       <see cref="PropertyWasUpdated(string, object, object)"/>.
                    ///   </para>
                    /// </summary>
                    [RequireComponent(typeof(MapObject))]
                    public abstract class ObjectStrategy : MonoBehaviour
                    {
                        /// <summary>
                        ///   Builds a FQDN property name for a given type and property.
                        /// </summary>
                        /// <typeparam name="T">The ObjectStrategy type for which the property has to be got</typeparam>
                        /// <param name="property">The name of the property</param>
                        /// <returns>The fully qualified property name</returns>
                        public static string FullyQualifiedProperty<T>(string property) where T : ObjectStrategy
                        {
                            return Classes.FullyQualifiedProperty<T>(property);
                        }

                        private static Type baseCounterpartStrategyType = typeof(World.Layers.Objects.ObjectsManagementStrategies.ObjectsManagementStrategy);

                        /// <summary>
                        ///   Tells when the given counterpart type is not valid (i.e. subclass of
                        ///     <see cref="World.Layers.Objects.ObjectsManagementStrategies.ObjectsManagementStrategy"/>).
                        /// </summary>
                        public class UnsupportedTypeException : Types.Exception
                        {
                            public UnsupportedTypeException() { }
                            public UnsupportedTypeException(string message) : base(message) { }
                            public UnsupportedTypeException(string message, Exception inner) : base(message, inner) { }
                        }

                        /// <summary>
                        ///   The related strategy holder, which will be in the same object.
                        /// </summary>
                        public ObjectStrategyHolder StrategyHolder { get; private set; }

                        /// <summary>
                        ///   <para>
                        ///     The counterpart type, which is particular to each subtype of object
                        ///       strategy.
                        ///   </para>
                        ///   <para>
                        ///     It will be a subclass of <see cref="World.ObjectsManagementStrategies.ObjectsManagementStrategy"/>.
                        ///   </para>
                        /// </summary>
                        public Type CounterpartType { get; private set; }

                        /// <summary>
                        ///   The related object.
                        /// </summary>
                        public MapObject Object { get; private set; }

                        /**
                         * Initializes the related data and the counterpart type.
                         */
                        protected virtual void Awake()
                        {
                            StrategyHolder = GetComponent<ObjectStrategyHolder>();
                            CounterpartType = GetCounterpartType();
                            if (CounterpartType == null || !AlephVault.Unity.Support.Utils.Classes.IsSameOrSubclassOf(CounterpartType, baseCounterpartStrategyType))
                            {
                                Destroy(gameObject);
                                throw new UnsupportedTypeException(string.Format("The type returned by CounterpartType must be a subclass of {0}", baseCounterpartStrategyType.FullName));
                            }
                            Object = GetComponent<MapObject>();
                        }

                        /// <summary>
                        ///   Returns the counterpart type, which is per-strategy defined and
                        ///     is subclass of <see cref="World.ObjectsManagementStrategies.ObjectsManagementStrategy"/>.
                        /// </summary>
                        /// <returns>The counterpart type</returns>
                        protected abstract Type GetCounterpartType();

                        /// <summary>
                        ///   <para>
                        ///     This method is run by the <see cref="ObjectStrategyHolder"/> and there
                        ///       is no need or use to invoke it by hand.
                        ///   </para>
                        ///   <para>
                        ///     Initializes the strategy, after the map object is initialized.
                        ///   </para>
                        /// </summary>
                        public virtual void Initialize()
                        {
                        }

                        /// <summary>
                        ///   Use this method inside properties' logic to notify the value change.
                        /// </summary>
                        /// <param name="property">The property being changed</param>
                        /// <param name="oldValue">The old value</param>
                        /// <param name="newValue">The new value</param>
                        protected void PropertyWasUpdated(string property, object oldValue, object newValue)
                        {
                            StrategyHolder?.Object?.ParentMap?.ObjectsLayer?.StrategyHolder?.PropertyWasUpdated(StrategyHolder, this, property, oldValue, newValue);
                        }
                    }

#if UNITY_EDITOR
                    [CustomEditor(typeof(ObjectStrategyHolder))]
                    [CanEditMultipleObjects]
                    public class ObjectStrategyHolderEditor : Editor
                    {
                        SerializedProperty strategy;

                        protected virtual void OnEnable()
                        {
                            strategy = serializedObject.FindProperty("objectStrategy");
                        }

                        public override void OnInspectorGUI()
                        {
                            serializedObject.Update();

                            ObjectStrategyHolder underlyingObject = (serializedObject.targetObject as ObjectStrategyHolder);
                            ObjectStrategy[] strategies = underlyingObject.GetComponents<ObjectStrategy>();
                            GUIContent[] strategyNames = (from strategy in strategies select new GUIContent(strategy.GetType().Name)).ToArray();

                            int index = ArrayUtility.IndexOf(strategies, strategy.objectReferenceValue as ObjectStrategy);
                            index = EditorGUILayout.Popup(new GUIContent("Main Strategy"), index, strategyNames);
                            strategy.objectReferenceValue = index >= 0 ? strategies[index] : null;

                            serializedObject.ApplyModifiedProperties();
                        }
                    }
#endif
                }
            }
        }
    }
}
