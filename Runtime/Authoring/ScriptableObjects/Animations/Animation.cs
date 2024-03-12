using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace VisualResources
            {
                /// <summary>
                ///   This class defines a WindRose animation. An animation contains the frames to render
                ///     and the amount of intended frames per second. Usually, this class is used inside an animation
                ///     rose (for certain direction in such rose) or as default animation for objects that are not
                ///     orientable (i.e. they have no direction to look/move to).
                /// </summary>
                [CreateAssetMenu(fileName = "NewAnimation", menuName = "AlephVault/Wind Rose/Visual Resources/Animation", order = 202)]
                public class Animation : ScriptableObject
                {
                    /// <summary>
                    ///   The frames to render.
                    /// </summary>
                    [SerializeField]
                    private Sprite[] sprites;

                    /// <summary>
                    ///   The frames per second.
                    /// </summary>
                    [SerializeField]
                    private uint fps = 16;

                    /// <summary>
                    ///   See <see cref="fps"/>.
                    /// </summary>
                    public uint FPS { get { return fps; } }

                    /// <summary>
                    ///   See <see cref="sprites"/> .
                    /// </summary>
                    public Sprite[] Sprites { get { return sprites; } }
                }
            }
        }
    }
}