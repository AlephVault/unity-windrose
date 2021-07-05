using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace GameMeanMachine.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Tiles
        {
            using AlephVault.Unity.Layout.Utils;
            using Authoring.ScriptableObjects.Tiles;

            /// <summary>
            ///   Menu actions to create bundled tiles from non-bundled ones.
            /// </summary>
            public static class BundledTileUtils
            {
                /// <summary>
                ///   This method is used in the menu action: Assets > Create > Wind Rose > Tiles > Bundled Tiles (From 1+ selected non-bundled tiles).
                ///   It creates one <see cref="BundledTile"/> for each selected <see cref="Tile"/> or <see cref="TileBase"/> not already being an
                ///     instance of <see cref="BundledTile"/>, under the new /Bundled subdirectory, in the same directory, for each individual
                ///     file and keeping the file name.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Bundled Tiles (From 1+ selected non-bundled tiles)", false, priority = 202)]
                public static void WrapIntoBundledTiles()
                {
                    foreach(TileBase tileBase in GetSelectedNonBundledTiles())
                    {
                        string path = AssetDatabase.GetAssetPath(tileBase);
                        string parentPath = Path.GetDirectoryName(path);
                        string fileName = Path.GetFileName(path);
                        string bundledPath = Path.Combine(parentPath, "Bundled");
                        if (!AssetDatabase.IsValidFolder(bundledPath))
                        {
                            AssetDatabase.CreateFolder(parentPath, "Bundled");
                        }
                        BundledTile bundledTile = ScriptableObject.CreateInstance<BundledTile>();
                        Undo.RegisterCreatedObjectUndo(bundledTile, "Create Bundled Tile");
                        Behaviours.SetObjectFieldValues(bundledTile, new Dictionary<string, object>() {
                            { "sourceTile", tileBase }
                        });
                        AssetDatabase.CreateAsset(bundledTile, Path.Combine(bundledPath, fileName));
                    }
                }

                /// <summary>
                ///   Validates the menu item: Assets > Create > Wind Rose > Tiles > Bundled Tiles (From 1+ selected non-bundled tiles).
                ///   It enables such menu option when 1 or more <see cref="Tile"/> or <see cref="TileBase"/> objects are selected in
                ///     the project explorer.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Bundled Tiles (From 1+ selected non-bundled tiles)", true)]
                public static bool CanWrapIntoBundledTiles()
                {
                    return GetSelectedNonBundledTiles().Count() > 0;
                }

                private static IEnumerable<TileBase> GetSelectedNonBundledTiles()
                {
                    foreach(TileBase tileBase in TileUtils.GetSelectedAssets<TileBase>())
                    {
                        if (!(tileBase is BundledTile)) yield return tileBase;
                    }
                }
            }
        }
    }
}
