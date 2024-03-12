using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.Boilerplates.Utils;
using AlephVault.Unity.MenuActions.Types;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            /// <summary>
            ///   This boilerplate creates a strategy in the WindRose project,
            ///   which must already have the startup layout defined by prior
            ///   calling to <see cref="ProjectStartup"/>.
            /// </summary>
            public static class CreateObjectStrategy
            {
                /// <summary>
                ///   Utility window used to create strategy files. It takes
                ///   a name and whether to put both the ObjectStrategy and
                ///   ObjectManagementStrategy files and the TileStrategy
                ///   file, or just the TileStrategy file, or just the other
                ///   two files.
                /// </summary>
                public class CreateObjectStrategyWindow : SmartEditorWindow
                {
                    // The base name to use.
                    private Regex baseNameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private string baseName = "MyCustom";
                    // The flags.
                    private bool objectStrategies = true;
                    private bool tileStrategy = true;
                    
                    protected override float GetSmartWidth()
                    {
                        return 750;
                    }
                    
                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();

                        EditorGUILayout.LabelField("Strategies generation", captionLabelStyle);
                        EditorGUILayout.LabelField(@"
This utility generates up to three strategy files, with boilerplate code and instructions on how to understand that code.

First, the base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

Then, two flags may be selected (they are selected by default), and they have the following behaviour:
- Object Strategy files: will generate two scripts named '{base name}ObjectsManagementStrategy' and '{base name}ObjectStrategy'.
- Tile Strategy file: will generate one script named '{base name}TileStrategy'.

WARNING: THIS MIGHT OVERRIDE EXISTING CODE. Always use proper source code management & versioning.
".Trim(), longLabelStyle);
                        baseName = EditorGUILayout.TextField("Base name", baseName).Trim();
                        bool validBaseName = baseNameCriterion.IsMatch(baseName);
                        if (!validBaseName)
                        {
                            EditorGUILayout.LabelField("The base name is invalid!", indentedStyle);
                        }

                        objectStrategies = EditorGUILayout.ToggleLeft("Object Strategy files", objectStrategies);
                        tileStrategy = EditorGUILayout.ToggleLeft("Tile Strategy files", tileStrategy);

                        if (validBaseName && (objectStrategies || tileStrategy))
                            SmartButton("Generate", Execute);
                    }

                    private void Execute()
                    {
                        DumpObjectStrategyTemplates(baseName, tileStrategy, objectStrategies);
                    }
                }
                
                // Performs the full dump of the code.
                private static void DumpObjectStrategyTemplates(
                    string basename, bool tileStrategy, bool objectStrategies
                ) {
                    string directory = "Packages/com.alephvault.unity.windrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";
                    TextAsset osText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/ObjectStrategy.cs.txt"
                    );
                    TextAsset omsText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/ObjectsManagementStrategy.cs.txt"
                    );
                    TextAsset tsText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/TileStrategy.cs.txt"
                    );
                    string objectStrategyName = basename + "ObjectStrategy";
                    string objectsManagementStrategyName = basename + "ObjectsManagementStrategy";
                    string tileStrategyName = basename + "TileStrategy";
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"OBJECTSTRATEGY", objectStrategyName},
                        {"OBJECTSMANAGEMENTSTRATEGY", objectsManagementStrategyName}
                    };

                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Core", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("MapObjects", false)
                                            .IntoDirectory("ObjectStrategies", false)
                                                .Do(delegate(Boilerplate boilerplate, string s)
                                                {
                                                    if (objectStrategies)
                                                    {
                                                        Boilerplate.InstantiateScriptCodeTemplate(
                                                            osText, objectStrategyName, replacements
                                                        )(boilerplate, s);
                                                    }
                                                })
                                            .End()
                                        .End()
                                        .IntoDirectory("Maps", false)
                                            .IntoDirectory("ObjectManagementStrategies", false)
                                                .Do(delegate(Boilerplate boilerplate, string s)
                                                {
                                                    if (objectStrategies)
                                                    {
                                                        Boilerplate.InstantiateScriptCodeTemplate(
                                                            omsText, objectsManagementStrategyName, replacements
                                                        )(boilerplate, s);
                                                    }
                                                })
                                            .End()
                                        .End()
                                    .End()
                                    .IntoDirectory("ScriptableObjects", false)
                                        .IntoDirectory("Tiles", false)
                                            .IntoDirectory("Strategies", false)
                                                .Do(delegate(Boilerplate boilerplate, string s)
                                                {
                                                    if (tileStrategy)
                                                    {
                                                        Boilerplate.InstantiateScriptCodeTemplate(
                                                            tsText, tileStrategyName, replacements
                                                        )(boilerplate, s);
                                                    }
                                                })
                                            .End()
                                        .End()
                                    .End()
                                .End()
                            .End()
                        .End();
                }
                
                /// <summary>
                ///   Opens a dialog to execute the strategy creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Aleph Vault/WindRose/Boilerplates/Create Object Strategy", false, 12)]
                public static void ExecuteBoilerplate()
                {
                    CreateObjectStrategyWindow window = ScriptableObject.CreateInstance<CreateObjectStrategyWindow>();
                    Vector2 size = new Vector2(750, 290);
                    window.position = new Rect(new Vector2(110, 250), size);
                    window.minSize = size;
                    window.maxSize = size;
                    window.titleContent = new GUIContent("WindRose Strategies generation");
                    window.ShowUtility();
                }
            }
        }
    }
}
