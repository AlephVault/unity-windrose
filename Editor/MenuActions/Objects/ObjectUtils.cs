using System.Collections.Generic;
using AlephVault.Unity.MenuActions.Types;
using UnityEngine;
using UnityEditor;
using AlephVault.Unity.Support.Utils;

namespace AlephVault.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Maps
        {
            using AlephVault.Unity.MenuActions.Utils;

            /// <summary>
            ///   Menu actions to create an object inside an Objects layer.
            /// </summary>
            public static class ObjectUtils
            {
                private class CreateObjectWindow : SmartEditorWindow
                {
                    private static int[] addStrategyOptions = { 0, 1, 2 };
                    private static string[] addStrategyLabels = { "Simple (includes Solidness and Layout)", "Layout", "Nothing (will be added manually later)" };
                    private static int[] addTriggerOptions = { 0, 1, 2 };
                    private static string[] addTriggerLabels = { "No trigger", "Live trigger", "Platform" };

                    public Transform selectedTransform;
                    // Basic properties.
                    private string objectName = "New Object";
                    private Vector2Int objectSize = new (1, 1);
                    // Optional behaviours to send commands.
                    private bool addCommandSender = false;
                    private bool addTalkSender = false; // depends on addCommandSender.
                    // Optional behaviours for trigger type.
                    private int addTrigger = 0;
                    // Optional behaviours for when trigger type = activator.
                    private bool addCommandReceiver = false; // depends on addTrigger == 1.
                    private bool addTalkReceiver = false; // depends on addCommandReceiver.
                    // Object strategy setup.
                    private int addStrategy = 0;

                    protected override float GetSmartWidth()
                    {
                        return 500;
                    }
                    
                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();
                        
                        // General settings start here.

                        titleContent = new GUIContent("Wind Rose - Creating a new object");
                        EditorGUILayout.LabelField("This wizard will create an object in the hierarchy of the current scene, under the selected objects layer in the hierarchy.", longLabelStyle);

                        // Object properties.

                        EditorGUILayout.LabelField("This is the name the game object will have when added to the hierarchy.", longLabelStyle);
                        objectName = MenuActionUtils.EnsureNonEmpty(EditorGUILayout.TextField("Object name", objectName), "New Object");

                        EditorGUILayout.LabelField("These are the object properties in the editor. Can be changed later.", longLabelStyle);
                        objectSize = EditorGUILayout.Vector2IntField("Object width (X) and height (Y) [1 to 32767]", objectSize);
                        objectSize = new Vector2Int(Values.Clamp(1, objectSize.x, 32767), Values.Clamp(1, objectSize.y, 32767));

                        addCommandSender = EditorGUILayout.ToggleLeft("Close Command Sender (Provides feature to send a custom command to close objects)", addCommandSender);
                        if (addCommandSender)
                        {
                            EditorGUILayout.BeginVertical(indentedStyle);
                            addTalkSender = EditorGUILayout.ToggleLeft("Talk Sender (A particular close command sender that dispatches a talk command to NPCs)", addTalkSender);
                            EditorGUILayout.EndVertical();
                        }
                        addTrigger = EditorGUILayout.IntPopup("Trigge Type", addTrigger, addTriggerLabels, addTriggerOptions);
                        if (addTrigger == 1)
                        {
                            EditorGUILayout.BeginVertical(indentedStyle);
                            addCommandReceiver = EditorGUILayout.ToggleLeft("Command Receiver (Provides feature to NPCs to receive a custom command)", addCommandReceiver);
                            if (addCommandReceiver)
                            {
                                EditorGUILayout.BeginVertical(indentedStyle);
                                addTalkReceiver = EditorGUILayout.ToggleLeft("Talk Receiver (A particular command receiver that understands a talk command)", addTalkReceiver);
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        addStrategy = EditorGUILayout.IntPopup("Object Strategy", addStrategy, addStrategyLabels, addStrategyOptions);
                        SmartButton("Create Object", Execute);
                    }

                    private void Execute()
                    {
                        GameObject gameObject = new GameObject(objectName);
                        gameObject.transform.parent = selectedTransform;
                        gameObject.SetActive(false);
                        Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.MapObject>(gameObject, new Dictionary<string, object>() {
                            { "width", (ushort)objectSize.x },
                            { "height", (ushort)objectSize.y }
                        });
                        gameObject.AddComponent<Authoring.Behaviours.Entities.Objects.ObjectStrategyHolder>();
                        if (addCommandSender)
                        {
                            Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.CommandExchange.CloseCommandSender>(gameObject);
                            if (addTalkSender)
                            {
                                Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.CommandExchange.Talk.TalkSender>(gameObject);
                            }
                        }
                        switch (addTrigger)
                        {
                            case 1:
                                Layout.Utils.Behaviours.AddComponent<BoxCollider>(gameObject);
                                Layout.Utils.Behaviours.AddComponent<Rigidbody>(gameObject);
                                Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.TriggerLive>(gameObject);
                                if (addCommandReceiver)
                                {
                                    Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.CommandExchange.CommandReceiver>(gameObject);
                                    if (addTalkReceiver)
                                    {
                                        Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.CommandExchange.Talk.TalkReceiver>(gameObject);
                                    }
                                }
                                break;
                            case 2:
                                Layout.Utils.Behaviours.AddComponent<BoxCollider>(gameObject);
                                Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.TriggerPlatform>(gameObject, new Dictionary<string, object>()
                                {
                                    { "innerMarginFactor", 0.25f }
                                });
                                break;
                        }
                        Authoring.Behaviours.Entities.Objects.Strategies.ObjectStrategy mainStrategy = null;
                        switch(addStrategy)
                        {
                            case 0:
                                Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Strategies.Base.LayoutObjectStrategy>(gameObject);
                                Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Strategies.Solidness.SolidnessObjectStrategy>(gameObject);
                                mainStrategy = Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Strategies.Simple.SimpleObjectStrategy>(gameObject);
                                break;
                            case 1:
                                mainStrategy = Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Entities.Objects.Strategies.Base.LayoutObjectStrategy>(gameObject);
                                break;
                        }
                        Authoring.Behaviours.Entities.Objects.ObjectStrategyHolder currentHolder = gameObject.GetComponent<Authoring.Behaviours.Entities.Objects.ObjectStrategyHolder>();
                        Layout.Utils.Behaviours.SetObjectFieldValues(currentHolder, new Dictionary<string, object>()
                        {
                            { "objectStrategy", mainStrategy }
                        });
                        gameObject.SetActive(true);
                        Undo.RegisterCreatedObjectUndo(gameObject, "Create Object");
                    }
                }

                /// <summary>
                ///   This method is used in the menu action: GameObject > Wind Rose > Objects > Create Object.
                ///   It creates a <see cref="Authoring.Behaviours.Entities.Objects.MapObject"/> under the selected objects
                ///     layer, in the scene editor.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Objects/Create Object", false, 11)]
                public static void CreateObject()
                {
                    CreateObjectWindow window = ScriptableObject.CreateInstance<CreateObjectWindow>();
                    window.position = new Rect(60, 180, 700, 468);
                    window.minSize = new Vector2(700, 215);
                    window.maxSize = window.minSize;
                    window.selectedTransform = Selection.activeTransform;
                    // window.position = new Rect(new Vector2(57, 336), new Vector2(689, 138));
                    window.ShowUtility();
                }

                /// <summary>
                ///   Validates the menu item: GameObject > Wind Rose > Objects > Create Object.
                ///   It enables such menu option when an <see cref="Behaviours.World.Layers.Objects.ObjectsLayer"/>
                ///     is selected in the scene editor.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Objects/Create Object", true)]
                public static bool CanCreateObject()
                {
                    return Selection.activeTransform && Selection.activeTransform.GetComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsLayer>();
                }
            }
        }
    }
}