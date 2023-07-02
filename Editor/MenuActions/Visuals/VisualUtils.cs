using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEditor;
using AlephVault.Unity.Support.Utils;

namespace AlephVault.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Visuals
        {
            using Authoring.ScriptableObjects.VisualResources;
            using Authoring.Behaviours.Entities.Visuals;
            using Authoring.Behaviours.Entities.Visuals.StateBundles.Moving;
            using Authoring.Behaviours.Entities.Common;
            using AlephVault.Unity.MenuActions.Utils;

            /// <summary>
            ///   Menu actions to create a visual inside an object in the scene.
            /// </summary>
            public static class VisualUtils
            {
                private class CreateVisualWindow : EditorWindow
                {
                    private static int[] visualTypes = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                    private static string[] visualTypeLabels = new string[] {
                        "Static (Visual - Using Sprite)",
                        string.Format("Animated (Visual, Animated - Using {0})", typeof(Animation).FullName),
                        string.Format("Rose-Sprited (Visual, RoseSprited - Using {0})", typeof(SpriteRose).FullName),
                        string.Format("Rose-Animated (Visual, Animated, RoseAnimated - Using {0})", typeof(AnimationRose).FullName),
                        "Multi-State static (Visual, MultiSprited - Using Sprite)",
                        string.Format("Multi-State Animated (Visual, Animated, MultiAnimated - Using {0})", typeof(Animation).FullName),
                        string.Format("Multi-State Rose-Sprited (Visual, RoseSprited, MultiRoseSprited - Using {0})", typeof(SpriteRose).FullName),
                        string.Format("Multi-State Rose-Animated (Visual, Animated, RoseAnimated, MultiRoseAnimated - Using {0})", typeof(AnimationRose).FullName),
                    };

                    private string visualObjectName = "";
                    private int visualType = 0;
                    private ushort visualLevel = 1 << 14;
                    private bool addMovingBundle = false;
                    public Transform selectedTransform;

                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();

                        Rect rect = EditorGUILayout.BeginVertical();
                        // General settings start here.

                        titleContent = new GUIContent("Wind Rose - Creating a new visual");
                        EditorGUILayout.LabelField("This wizard will create a visual object in the hierarchy of the current scene, under the selected object in the hierarchy.", longLabelStyle);

                        // Visual properties.

                        EditorGUILayout.LabelField("This is the name the game object will have when added to the hierarchy.", longLabelStyle);
                        visualObjectName = MenuActionUtils.EnsureNonEmpty(EditorGUILayout.TextField("Object name", visualObjectName), "New Visual");

                        EditorGUILayout.LabelField("Visual type", captionLabelStyle);
                        visualType = EditorGUILayout.IntPopup(visualType, visualTypeLabels, visualTypes);
                        visualLevel = (ushort)Values.Clamp(0, EditorGUILayout.IntField("Level [1 to 32767]", visualLevel), (1 << 15) - 1);

                        if (visualType >= 4)
                        {
                            EditorGUILayout.LabelField("While the Multi-State behaviours already provide a setting for idle state display, more state bundles can be added to support states in standard behaviours:", longLabelStyle);
                            addMovingBundle = EditorGUILayout.ToggleLeft("Moving (e.g. for walking characters)", addMovingBundle);
                        }

                        bool execute = GUILayout.Button("Create Visual");
                        EditorGUILayout.EndVertical();
                        
                        if (execute) Execute();
                    }

                    private void Execute()
                    {
                        GameObject gameObject = new GameObject(visualObjectName);
                        gameObject.transform.parent = selectedTransform;
                        gameObject.SetActive(false);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Pausable>(gameObject);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SpriteRenderer>(gameObject);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Visual>(gameObject, new Dictionary<string, object>() {
                            { "level", visualLevel }
                        });

                        switch (visualType)
                        {
                            case 0:
                                // Nothing to do here.
                                break;
                            case 1:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Animated>(gameObject);
                                break;
                            case 2:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<RoseSprited>(gameObject);
                                break;
                            case 3:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Animated>(gameObject);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<RoseAnimated>(gameObject);
                                break;
                            case 4:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MultiSprited>(gameObject);
                                if (addMovingBundle)
                                {
                                    AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MovingSpriteBundle>(gameObject);
                                }
                                break;
                            case 5:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Animated>(gameObject);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MultiAnimated>(gameObject);
                                if (addMovingBundle)
                                {
                                    AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MovingAnimationBundle>(gameObject);
                                }
                                break;
                            case 6:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<RoseSprited>(gameObject);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MultiRoseSprited>(gameObject);
                                if (addMovingBundle)
                                {
                                    AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MovingSpriteRoseBundle>(gameObject);
                                }
                                break;
                            case 7:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Animated>(gameObject);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<RoseAnimated>(gameObject);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MultiRoseAnimated>(gameObject);
                                if (addMovingBundle)
                                {
                                    AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<MovingAnimationRoseBundle>(gameObject);
                                }
                                break;
                        }
                        gameObject.SetActive(true);
                        Undo.RegisterCreatedObjectUndo(gameObject, "Create Visual");
                        Close();
                    }
                }

                /// <summary>
                ///   This method is used in the menu action: GameObject > Wind Rose > Visuals > Create Visual.
                ///   It creates a <see cref="Behaviours.Entities.Visuals.Visual"/> under the selected transform,
                ///     in the scene editor, that has a <see cref="Behaviours.Entities.Objects.MapObject"/> component.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Visuals/Create Visual", false, 11)]
                public static void CreateVisual()
                {
                    CreateVisualWindow window = ScriptableObject.CreateInstance<CreateVisualWindow>();
                    window.position = new Rect(new Vector2(57, 336), new Vector2(689, 138));
                    window.selectedTransform = Selection.activeTransform;
                    window.ShowUtility();
                }

                /// <summary>
                ///   Validates the menu item: GameObject > Wind Rose > Visuals > Create Visual.
                ///   It enables such menu option when an <see cref="Behaviours.Entities.Objects.MapObject"/>
                ///     is selected in the scene editor.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Visuals/Create Visual", true)]
                public static bool CanCreateVisual()
                {
                    return Selection.activeTransform && Selection.activeTransform.GetComponent<Authoring.Behaviours.Entities.Objects.MapObject>();
                }
            }
        }
    }
}