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
                
                /// <summary>
                ///   An sprite rose consist of 4 sprites: one for each direction.
                ///   Intended for non-animated orientable objects.
                /// </summary>
                [CreateAssetMenu(fileName = "NewSpriteRose", menuName = "Wind Rose/Visual Resources/Sprite Rose", order = 201)]
                public class SpriteRose : ScriptableObject
                {
                    /// <summary>
                    ///   Sprite for the UP direction.
                    /// </summary>
                    [SerializeField]
                    private Sprite up;

                    /// <summary>
                    ///   Sprite for the DOWN direction.
                    /// </summary>
                    [SerializeField]
                    private Sprite down;

                    /// <summary>
                    ///   Sprite for the LEFT direction.
                    /// </summary>
                    [SerializeField]
                    private Sprite left;

                    /// <summary>
                    ///   Sprite for the RIGHT direction.
                    /// </summary>
                    [SerializeField]
                    private Sprite right;

                    /// <summary>
                    ///   Gets the sprite for a given direction. This method is used internally from
                    ///     other classes (e.g. orientable).
                    /// </summary>
                    /// <param name="direction">The desired direction</param>
                    /// <returns>The sprite to render</returns>
                    public Sprite GetForDirection(Direction direction)
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