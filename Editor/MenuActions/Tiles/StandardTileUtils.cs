using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace AlephVault.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Tiles
        {
            using AlephVault.Unity.Layout.Utils;

            /// <summary>
            ///   Menu actions to create standard tiles from sprites.
            /// </summary>
            public static class StandardTileUtils
            {
                /// <summary>
                ///   This method is used in the menu action: Assets > Create > Wind Rose > Tiles > Tiles (From 1+ selected sprites).
                ///   For each sprite being selected, it creates a tile under the new /Tiles subdirectory, in the same directory,
                ///     for each individual sprite and keeping the sprite name.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Tiles (From 1+ selected sprites)", false, priority = 101)]
                public static void CreateTiles()
                {
                    foreach(Sprite sprite in TileUtils.GetSelectedAssets<Sprite>().ToArray())
                    {
                        string path = AssetDatabase.GetAssetPath(sprite);
                        string parentPath = Path.GetDirectoryName(path);
                        string fileName = sprite.name + ".asset";
                        string bundledPath = Path.Combine(parentPath, "Tiles");
                        if (!AssetDatabase.IsValidFolder(bundledPath))
                        {
                            AssetDatabase.CreateFolder(parentPath, "Tiles");
                        }
                        Tile tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = sprite;
                        Undo.RegisterCreatedObjectUndo(tile, "Create Tile");
                        AssetDatabase.CreateAsset(tile, Path.Combine(bundledPath, fileName));
                    }
                }

                /// <summary>
                ///   Validates the menu item: Assets > Create > Wind Rose > Tiles > Tiles (From 1+ selected sprites).
                ///   It enables such menu option when 1 or more <see cref="Sprite"/> objects are selected in
                ///     the project explorer.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Tiles (From 1+ selected sprites)", true)]
                public static bool CanCreateTiles()
                {
                    return TileUtils.GetSelectedAssets<Sprite>().Count() >= 1;
                }

                /***************** Move these functions to another utility depending on Unity2DExtras

                /// <summary>
                ///   This method is used in the menu action: Assets > Create > Wind Rose > Tiles > Random Tile (From 2+ selected sprites).
                ///   Taking all the selected sprites, creates a new random tile storing it in the /Tiles subdirectory in
                ///     the same folder of (and also using the filename taken from) the first sprite selected.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Random Tile (From 2+ selected sprites)", false, priority = 102)]
                public static void CreateRandomTile()
                {
                    Sprite[] sprites = TileUtils.GetSelectedAssets<Sprite>().ToArray();
                    string path = AssetDatabase.GetAssetPath(sprites[0].GetInstanceID());
                    string parentPath = Path.GetDirectoryName(path);
                    string fileName = sprites[0].name + ".asset";
                    string bundledPath = Path.Combine(parentPath, "Tiles");
                    if (!AssetDatabase.IsValidFolder(bundledPath))
                    {
                        AssetDatabase.CreateFolder(parentPath, "Tiles");
                    }
                    RandomTile randomTile = ScriptableObject.CreateInstance<RandomTile>();
                    Behaviours.SetObjectFieldValues(randomTile, new Dictionary<string, object>() {
                        { "m_Sprites", sprites }
                    });
                    Undo.RegisterCreatedObjectUndo(randomTile, "Create Random Tile");
                    AssetDatabase.CreateAsset(randomTile, Path.Combine(bundledPath, fileName));
                }

                /// <summary>
                ///   Validates the menu item: Assets > Create > Wind Rose > Tiles > Random Tile (From 2+ selected sprites).
                ///   It enables such menu option when 2 or more <see cref="Sprite"/> objects are selected in
                ///     the project explorer.
                /// </summary>
                [MenuItem("Assets/Create/Wind Rose/Tiles/Random Tile (From 2+ selected sprites)", true)]
                public static bool CanCreateRandomTile()
                {
                    return TileUtils.GetSelectedAssets<Sprite>().Count() >= 2;
                }

                ****************************************************************/
            }
        }
    }
}
