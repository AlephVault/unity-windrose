using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AlephVault.Unity.Support.Utils;
using AlephVault.Unity.MenuActions.Utils;

namespace GameMeanMachine.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Objects
        {
            /// <summary>
            ///   Menu actions to create a teleporter object inside an Objects layer.
            /// </summary>
            public static class TeleporterUtils
            {
                private class CreateTeleporterWindow : EditorWindow
                {
                    public Transform selectedTransform;
                    private string objectName = "New Teleporter";
                    private Vector2Int objectSize;
                    private bool addTeleportTargetBehaviour;
                    private bool useTaggedSystem;

                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();

                        titleContent = new GUIContent("Wind Rose - Creating a new local teleporter object");
                        EditorGUILayout.LabelField("This wizard will create a local teleporter object object under the currently selected objects layer in the hierarchy editor.", longLabelStyle);

                        EditorGUILayout.LabelField("This is the name the game object will have when added to the hierarchy.", longLabelStyle);
                        objectName = MenuActionUtils.EnsureNonEmpty(EditorGUILayout.TextField("Object name", objectName), "New Teleporter");

                        EditorGUILayout.LabelField("These are the object properties in the editor. Can be changed later.", longLabelStyle);
                        objectSize = EditorGUILayout.Vector2IntField("Object width (X) and height (Y) [1 to 32767]", objectSize);
                        objectSize = new Vector2Int(Values.Clamp(1, objectSize.x, 32767), Values.Clamp(1, objectSize.y, 32767));

                        addTeleportTargetBehaviour = EditorGUILayout.ToggleLeft("This teleport will also be a target for other possible teleports", addTeleportTargetBehaviour);
                        useTaggedSystem = EditorGUILayout.ToggleLeft("Use tagged teleports (useful for dynamic systems, but more prone to errors)", useTaggedSystem);

                        EditorGUILayout.LabelField("A target must be manually set for this teleporter. The target must have dimensions supporting the intended object(s) to be transferred.", captionLabelStyle);

                        if (GUILayout.Button("Create Object")) Execute();
                    }

                    private void Execute()
                    {
                        GameObject gameObject = new GameObject(objectName);
                        gameObject.transform.parent = selectedTransform;
                        gameObject.SetActive(false);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.MapObject>(gameObject, new Dictionary<string, object>() {
                            { "width", (ushort)objectSize.x },
                            { "height", (ushort)objectSize.y }
                        });
                        if (!useTaggedSystem)
                        {
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Teleport.SimpleTeleporter>(gameObject);
                            if (addTeleportTargetBehaviour)
                            {
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Teleport.SimpleTeleportTarget>(gameObject);
                            }
                        }
                        else
                        {
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Teleport.TaggedTeleporter>(gameObject);
                            if (addTeleportTargetBehaviour)
                            {
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Teleport.TaggedTeleportTarget>(gameObject);
                            }
                        }
                        gameObject.SetActive(true);
                        Undo.RegisterCreatedObjectUndo(gameObject, "Create Teleporter");
                        Close();
                    }
                }

                /// <summary>
                ///   This method is used in the menu action: GameObject > Wind Rose > Objects > Create Teleporter.
                ///   It creates a <see cref="Behaviours.Entities.Objects.Teleport.SimpleTeleporter"/> under the
                ///   selected objects layer, in the scene editor. If appropriately flagged, it creates a tagged
                ///   teleporter (instead of a simple one). The difference between both teleporters is that the
                ///   simple teleporter directly references the target, while the tagged teleporter
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Objects/Create Teleporter", false, 12)]
                public static void CreateTeleporter()
                {
                    CreateTeleporterWindow window = ScriptableObject.CreateInstance<CreateTeleporterWindow>();
                    window.position = new Rect(60, 180, 700, 468);
                    window.minSize = new Vector2(700, 196);
                    window.selectedTransform = Selection.activeTransform;
                    window.ShowUtility();
                }

                /// <summary>
                ///   Validates the menu item: GameObject > Wind Rose > Objects > Create Teleporter.
                ///   It enables such menu option when an <see cref="Behaviours.World.Layers.Objects.ObjectsLayer"/>
                ///     is selected in the scene editor.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Objects/Create Teleporter", true)]
                public static bool CanCreateTeleporter()
                {
                    return Selection.activeTransform && Selection.activeTransform.GetComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsLayer>();
                }
            }
        }
    }
}
