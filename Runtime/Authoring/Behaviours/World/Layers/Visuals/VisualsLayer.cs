using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Visuals
                    {
                        /// <summary>
                        ///   This layer holds the visuals to be rendered. For more information,
                        ///     see <see cref="Behaviours.Objects.Visuals.Visual"/>. Since visuals
                        ///     have a level property, this property tells the the depth perspective
                        ///     to use (intended to distinguish priority on rendering like holes,
                        ///     foot-level auras, characters, top-level auras, ...). This component
                        ///     is the one that will handle such depth perspective.
                        /// </summary>
                        public class VisualsLayer : MapLayer
                        {
                            protected override int GetSortingOrder()
                            {
                                return 40;
                            }

                            private Dictionary<ushort, VisualsDepthLevel> levels = new Dictionary<ushort, VisualsDepthLevel>();

                            /// <summary>
                            ///   Instantiates a new level for the visuals. Hopefully, the amount
                            ///     of levels to be used will be kept controlled to few levels in
                            ///     the overall game. These levels are never released until the
                            ///     destruction of the layer, so care must be taken when choosing
                            ///     the levels to use.
                            /// </summary>
                            /// <param name="level">The level to ask for - between 0 and 32767</param>
                            /// <returns>The corresponding level object</returns>
                            public VisualsDepthLevel this[ushort level]
                            {
                                get
                                {
                                    if (level >= 32768) throw new ArgumentException("Level must not be greater than 32768");
                                    VisualsDepthLevel levelObj;
                                    if (!levels.TryGetValue(level, out levelObj))
                                    {
                                        GameObject gameObj = new GameObject("Depth Level " + level);
                                        gameObj.transform.SetParent(transform);
                                        gameObj.transform.localRotation = Quaternion.identity;
                                        gameObj.transform.localScale = Vector3.one;
                                        gameObj.transform.localPosition = Vector3.zero;
                                        SortingGroup sortingGroup = gameObj.AddComponent<SortingGroup>();
                                        sortingGroup.sortingOrder = level;
                                        levelObj = gameObj.AddComponent<VisualsDepthLevel>();
                                        levels[level] = levelObj;
                                    }
                                    return levelObj;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
