using System.Collections;
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
            /// <summary>
            ///   This boilerplate creates a startup of the project, involving
            ///   mane different brand-new folders.
            /// </summary>
            public static class ProjectStartup
            {
                /// <summary>
                ///   This boilerplate function creates:
                ///   - Graphics/: Decals/, Objects/, Tiles/ and UI/.
                ///   - Objects/Prefabs/: Maps/, Objects/, Scopes/, Visual.
                ///   - Objects/Tiles/: Base/, Bundled/.
                ///   - Scripts/UI/.
                ///   - Scripts/Core/:
                ///     - Types/.
                ///     - Authoring/:
                ///       - Types/.
                ///       - ScriptableObjects/TileStrategies.
                ///       - Behaviours/: MapObjects/ObjectStrategies, Maps/ObjectStrategies.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Boilerplates/Project Startup", false, 11)]
                public static void ExecuteBoilerplate()
                {
                    new Boilerplate()
                        .IntoDirectory("Graphics")
                            .IntoDirectory("Decals")
                            .End()
                            .IntoDirectory("Objects")
                            .End()
                            .IntoDirectory("Tiles")
                            .End()
                            .IntoDirectory("UI")
                            .End()
                        .End()
                        .IntoDirectory("Objects")
                            .IntoDirectory("Prefabs")
                                .IntoDirectory("Maps")
                                .End()
                                .IntoDirectory("Objects")
                                .End()
                                .IntoDirectory("Scopes")
                                .End()
                                .IntoDirectory("Visual")
                                .End()
                            .End()
                            .IntoDirectory("Tiles")
                                .IntoDirectory("Base")
                                .End()
                                .IntoDirectory("Bundled")
                                .End()
                            .End()
                        .End()
                        .IntoDirectory("Scripts")
                            .IntoDirectory("Core")
                                .IntoDirectory("Authoring")
                                    .IntoDirectory("Behaviours")
                                        .IntoDirectory("MapObjects")
                                            .IntoDirectory("ObjectStrategies")
                                            .End()
                                        .End()
                                        .IntoDirectory("Maps")
                                            .IntoDirectory("ObjectManagementStrategies")
                                            .End()
                                        .End()
                                    .End()
                                    .IntoDirectory("ScriptableObjects")
                                        .IntoDirectory("Tiles")
                                            .IntoDirectory("Strategies")
                                            .End()
                                        .End()
                                    .End()
                                    .IntoDirectory("Types")
                                    .End()
                                .End()
                                .IntoDirectory("Types")
                                .End()
                            .End()
                            .IntoDirectory("UI")
                            .End()
                        .End();
                }
            }
        }
    }
}
