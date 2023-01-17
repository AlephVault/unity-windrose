using System.Collections.Generic;
using AlephVault.Unity.Boilerplates.Utils;
using UnityEditor;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            public static class CreateObjectStrategy
            {
                private static void DumpObjectStrategyTemplates(
                    string basename, bool tileStrategy, bool objectStrategies
                )
                {
                    string directory = "Packages/com.gamemeanmachine.unity.windrose/" +
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
                [MenuItem("Assets/Create/Wind Rose/Boilerplates/Create Object Strategy", false, 11)]
                public static void ExecuteBoilerplate()
                {
                    DumpObjectStrategyTemplates("Glorgax", true, true);
                }
            }
        }
    }
}
