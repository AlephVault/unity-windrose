using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace VisualResources
            {
                using Types;
                using AlephVault.Unity.Layout.Utils;

                /// <summary>
                ///   An animation rose consist of 4 animations: one for each direction.
                ///   Intended for animations in orientable or moving objects.
                /// </summary>
                [CreateAssetMenu(fileName = "NewAnimationRose", menuName = "Wind Rose/Visual Resources/Animation Rose", order = 201)]
                public class AnimationRose : ScriptableObject
                {
                    private delegate void AnimationRoseInitializationCallback(string path, AnimationRose rose, Animation down, Animation left, Animation right, Animation up);

                    /// <summary>
                    ///   Animation for the UP direction.
                    /// </summary>
                    [SerializeField]
                    private Animation up;

                    /// <summary>
                    ///   Animation for the DOWN direction.
                    /// </summary>
                    [SerializeField]
                    private Animation down;

                    /// <summary>
                    ///   Animation for the LEFT direction.
                    /// </summary>
                    [SerializeField]
                    private Animation left;

                    /// <summary>
                    ///   Animation for the RIGHT direction.
                    /// </summary>
                    [SerializeField]
                    private Animation right;

#if UNITY_EDITOR
                    [MenuItem("Assets/Create/Wind Rose/Visual Resources/Animation Rose (with Moving animations)")]
                    public static void CreateMovingInstanceWithChildSpecs()
                    {
                        CreateInstanceWithChildSpecs(delegate (string path, AnimationRose rose, Animation down, Animation left, Animation right, Animation up)
                        {
                            uint fps = 4;
                            Behaviours.SetObjectFieldValues(down, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[4] },
                            });
                            Behaviours.SetObjectFieldValues(left, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[4] },
                            });
                            Behaviours.SetObjectFieldValues(right, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[4] },
                            });
                            Behaviours.SetObjectFieldValues(up, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[4] },
                            });

                            string title = "Do you want to fill the animations' sprites?";
                            string message = "You can select a character spritesheet of 12 sprites to fill this animation rose. If you don't, you will have to fill the animation manually later.\n\n" +
                                             "It is required that the selected spritesheet image is imported as a Sprite texture in a Sprite Mode setting of multiple sub-images, and they have the name " +
                                             "of the main asset and an _index suffix.";
                            if (EditorUtility.DisplayDialog(title, message, "Pick spritesheet", "I'll manually fill them later"))
                            {
                                while (true)
                                {
                                    string sourceImagePath = EditorUtility.OpenFilePanelWithFilters("Please select a spritesheet", path, new string[] { "PNG images", "png" });
                                    if (sourceImagePath == "")
                                    {
                                        message = "You did not select any image. Do you want to try again or fill the animations later?";
                                        if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                        {
                                            break;
                                        }
                                    }
                                    else if (sourceImagePath.StartsWith(Application.dataPath))
                                    {
                                        sourceImagePath = sourceImagePath.Substring(Path.GetDirectoryName(Application.dataPath).Length + 1);
                                        Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(sourceImagePath).OfType<Sprite>().ToArray();
                                        if (sprites.Length != 12)
                                        {
                                            message = "The selected image is not a registered texture with 12 sprites. Do you want to try again or fill the animations later?";
                                            if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // Sorting the sprites by index
                                            string name = AssetDatabase.LoadMainAssetAtPath(sourceImagePath).name;
                                            int nameLength = name.Length + 1;
                                            bool parseError = false;
                                            Sprite[] sortedSprites = new Sprite[12];
                                            foreach (Sprite sprite in sprites)
                                            {
                                                int parsedInt;
                                                if (!int.TryParse(sprite.name.Substring(nameLength), out parsedInt) || parsedInt < 0 || parsedInt > 11 || sortedSprites[parsedInt] != null)
                                                {
                                                    parseError = true;
                                                    break;
                                                }
                                                sortedSprites[parsedInt] = sprite;
                                            }

                                            if (parseError)
                                            {
                                                message = "The selected image sprites have not an _index-suffixed name ranging from 0 to 11. Do you want to try again or fill the animations later?";
                                                if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                                {
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                // The time of the truth: filling the images
                                                Behaviours.SetObjectFieldValues(down, new Dictionary<string, object>() {
                                                { "sprites", new Sprite[] { sortedSprites[0], sortedSprites[1], sortedSprites[2], sortedSprites[1] } },
                                            });
                                                Behaviours.SetObjectFieldValues(left, new Dictionary<string, object>() {
                                                { "sprites", new Sprite[] { sortedSprites[3], sortedSprites[4], sortedSprites[5], sortedSprites[4] } },
                                            });
                                                Behaviours.SetObjectFieldValues(right, new Dictionary<string, object>() {
                                                { "sprites", new Sprite[] { sortedSprites[6], sortedSprites[7], sortedSprites[8], sortedSprites[7] } },
                                            });
                                                Behaviours.SetObjectFieldValues(up, new Dictionary<string, object>() {
                                                { "sprites", new Sprite[] { sortedSprites[9], sortedSprites[10], sortedSprites[11], sortedSprites[10] } },
                                            });
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Invalid path", "The chosen path is not inside project's data.", "OK");
                                    }
                                }
                            }
                        });
                    }

                    [MenuItem("Assets/Create/Wind Rose/Visual Resources/Animation Rose (with Staying animations)")]
                    public static void CreateStayingInstanceWithChildSpecs()
                    {
                        CreateInstanceWithChildSpecs(delegate (string path, AnimationRose rose, Animation down, Animation left, Animation right, Animation up)
                        {
                            uint fps = 1;
                            // The time of the truth: filling the images
                            Behaviours.SetObjectFieldValues(down, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[1] },
                            });
                            Behaviours.SetObjectFieldValues(left, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[1] },
                            });
                            Behaviours.SetObjectFieldValues(right, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[1] },
                            });
                            Behaviours.SetObjectFieldValues(up, new Dictionary<string, object>() {
                                { "fps", fps },
                                { "sprites", new Sprite[1] },
                            });

                            string title = "Do you want to fill the animations' sprites?";
                            string message = "You can select a character spritesheet of 12 sprites to fill this animation rose. If you don't, you will have to fill the animation manually later.\n\n" +
                                             "It is required that the selected spritesheet image is imported as a Sprite texture in a Sprite Mode setting of multiple sub-images, and they have the name " +
                                             "of the main asset and an _index suffix.";
                            if (EditorUtility.DisplayDialog(title, message, "Pick spritesheet", "I'll manually fill them later"))
                            {
                                while (true)
                                {
                                    string sourceImagePath = EditorUtility.OpenFilePanelWithFilters("Please select a spritesheet", path, new string[] { "PNG images", "png" });
                                    if (sourceImagePath == "")
                                    {
                                        message = "You did not select any image. Do you want to try again or fill the animations later?";
                                        if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                        {
                                            break;
                                        }
                                    }
                                    else if (sourceImagePath.StartsWith(Application.dataPath))
                                    {
                                        sourceImagePath = sourceImagePath.Substring(Path.GetDirectoryName(Application.dataPath).Length + 1);
                                        Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(sourceImagePath).OfType<Sprite>().ToArray();
                                        if (sprites.Length != 12)
                                        {
                                            message = "The selected image is not a registered texture with 12 sprites. Do you want to try again or fill the animations later?";
                                            if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // Sorting the sprites by index
                                            string name = AssetDatabase.LoadMainAssetAtPath(sourceImagePath).name;
                                            int nameLength = name.Length + 1;
                                            bool parseError = false;
                                            Sprite[] sortedSprites = new Sprite[12];
                                            foreach (Sprite sprite in sprites)
                                            {
                                                int parsedInt;
                                                if (!int.TryParse(sprite.name.Substring(nameLength), out parsedInt) || parsedInt < 0 || parsedInt > 11 || sortedSprites[parsedInt] != null)
                                                {
                                                    parseError = true;
                                                    break;
                                                }
                                                sortedSprites[parsedInt] = sprite;
                                            }

                                            if (parseError)
                                            {
                                                message = "The selected image sprites have not an _index-suffixed name ranging from 0 to 11. Do you want to try again or fill the animations later?";
                                                if (!EditorUtility.DisplayDialog(title, message, "Try again", "I'll manually fill them later"))
                                                {
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                // The time of the truth: filling the images
                                                Behaviours.SetObjectFieldValues(down, new Dictionary<string, object>() {
                                                    { "sprites", new Sprite[] { sortedSprites[1] } },
                                                });
                                                Behaviours.SetObjectFieldValues(left, new Dictionary<string, object>() {
                                                    { "sprites", new Sprite[] { sortedSprites[4] } },
                                                });
                                                Behaviours.SetObjectFieldValues(right, new Dictionary<string, object>() {
                                                    { "sprites", new Sprite[] { sortedSprites[7] } },
                                                });
                                                Behaviours.SetObjectFieldValues(up, new Dictionary<string, object>() {
                                                    { "sprites", new Sprite[] { sortedSprites[10] } },
                                                });
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Invalid path", "The chosen path is not inside project's data.", "OK");
                                    }
                                }
                            }
                        });
                    }

                    [MenuItem("Assets/Create/Wind Rose/Visual Resources/Animation Rose (with empty animations)")]
                    public static void CreateInstanceWithNoChildSpecs()
                    {
                        CreateInstanceWithChildSpecs(delegate (string path, AnimationRose rose, Animation down, Animation left, Animation right, Animation up)
                        {
                        });
                    }

                    private static void CreateInstanceWithChildSpecs(AnimationRoseInitializationCallback callback)
                    {
                        AnimationRose instance = CreateInstance<AnimationRose>();
                        Animation instanceUp = CreateInstance<Animation>();
                        Animation instanceDown = CreateInstance<Animation>();
                        Animation instanceLeft = CreateInstance<Animation>();
                        Animation instanceRight = CreateInstance<Animation>();
                        Behaviours.SetObjectFieldValues(instance, new Dictionary<string, object>() {
                            { "up", instanceUp },
                            { "down", instanceDown },
                            { "left", instanceLeft },
                            { "right", instanceRight },
                        });

                        string newAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                        if (newAssetPath == "")
                        {
                            newAssetPath = "Assets";
                        }
                        if (!Directory.Exists(newAssetPath))
                        {
                            newAssetPath = Path.GetDirectoryName(newAssetPath);
                        }

                        callback(newAssetPath, instance, instanceDown, instanceLeft, instanceRight, instanceUp);

                        AssetDatabase.CreateAsset(instanceUp, Path.Combine(newAssetPath, "AnimationUp.asset"));
                        AssetDatabase.CreateAsset(instanceDown, Path.Combine(newAssetPath, "AnimationDown.asset"));
                        AssetDatabase.CreateAsset(instanceLeft, Path.Combine(newAssetPath, "AnimationLeft.asset"));
                        AssetDatabase.CreateAsset(instanceRight, Path.Combine(newAssetPath, "AnimationRight.asset"));
                        AssetDatabase.CreateAsset(instance, Path.Combine(newAssetPath, "AnimationRose.asset"));
                    }
#endif

                    /// <summary>
                    ///   Gets the animation for a given direction. This method is used internally from
                    ///     other classes (e.g. orientable).
                    /// </summary>
                    /// <param name="direction">The desired direction</param>
                    /// <returns>The animation to render</returns>
                    public Animation GetForDirection(Direction direction)
                    {
                        switch (direction)
                        {
                            case Direction.UP:
                                return up;
                            case Direction.DOWN:
                                return down;
                            case Direction.LEFT:
                                return left;
                            case Direction.RIGHT:
                                return right;
                            default:
                                // No default will run here,
                                //   but just for code completeness
                                return down;
                        }
                    }
                }
            }
        }
    }
}