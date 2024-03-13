using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace Tiles
            {
                /// <summary>
                ///   An editor for the Bundled Tile. Its only aim
                ///   is to provide a custom icon.
                /// </summary>
                [InitializeOnLoad]
                [CustomEditor(typeof(BundledTile))]
                public class BundledTileEditor : Editor
                {
                    private static MethodInfo rspMethod;
                    private static bool warningOnMissingMethodSent = false;

                    static BundledTileEditor()
                    {
                        rspMethod = GetType("UnityEditor.SpriteUtility")?.GetMethod(
                            "RenderStaticPreview",
                            new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) }
                        );
                    }
                    
                    public override Texture2D RenderStaticPreview(string assetPath,Object[] subAssets,int width,int height)
                    {
                        // First, check the method is available.
                        if (rspMethod == null)
                        {
                            if (!warningOnMissingMethodSent)
                            {
                                warningOnMissingMethodSent = true;
                                Debug.LogWarning(
                                    "It seems that UnityEditor.SpriteUtility.RenderStaticPreview<Sprite, Color, int, int> " +
                                    "method is not available in your Unity version"
                                );
                            }

                            return null;
                        }

                        // Then, attempt the simple approach: Getting the underlying Tile's sprite.
                        // Only BundledTiles containing a Tile (or derived) class will be rendered
                        // with this behaviours. Future versions might have enhancements to allow
                        // retrieving sprites from other TileBase subtypes.
                        Sprite simpleSprite = GetSprite(target as BundledTile);
                        if (simpleSprite != null)
                        {
                            if (rspMethod != null)
                            {
                                object ret = rspMethod.Invoke("RenderStaticPreview",new object[] { simpleSprite, Color.white, width, height });
                                if (ret is Texture2D texture2D) return texture2D;
                            }
                        }
                        
                        return base.RenderStaticPreview(assetPath,subAssets,width,height);
                    }

                    private static Type GetType(string typeName)
                    {
                        var type = Type.GetType(typeName);
                        if (type != null) return type;

                        if (typeName.Contains("."))
                        {
                            var assemblyName = typeName.Substring(0, typeName.IndexOf('.'));
                            var assembly = Assembly.Load(assemblyName);
                            if (assembly == null)
                                return null;
                            type = assembly.GetType(typeName);
                            if (type != null)
                                return type;
                        }

                        var currentAssembly = Assembly.GetExecutingAssembly();
                        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                        foreach(var assemblyName in referencedAssemblies)
                        {
                            var assembly = Assembly.Load(assemblyName);
                            if (assembly != null)
                            {
                                type=assembly.GetType(typeName);
                                if (type != null)
                                    return type;
                            }
                        }
                        return null;
                    }

                    private Sprite GetSprite(BundledTile tile)
                    {
                        TileBase source = tile.SourceTile;
                        if (source != null && source is Tile sourceTile)
                        {
                            return sourceTile.sprite;
                        }

                        return null;
                    }
                }
            }
        }
    }
}
