using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using AlephVault.Unity.Support.Utils;

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
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Solidness
                            {
                                /// <summary>
                                ///   SolidObjectMask is an abstraction of an object's solidness mask,
                                ///     which has [Width]x[Height] cells, and each cell may onle be one
                                ///     out of three values: Solid, Ghost, Hole. They alter the objects
                                ///     layer's solidness strategy's overall mask in the same way the
                                ///     individual statuses do, but this time per individual cell. This
                                ///     type has only meaning when the owner object uses a Mask solidness
                                ///     type. Otherwise this type has no use.
                                /// </summary>
                                [Serializable]
                                public class SolidObjectMask
                                {
                                    /// <summary>
                                    ///   A marker attribute telling that this particular property should be
                                    ///     automatically clamped by the drawer.
                                    /// </summary>
                                    public class AutoClampedAttribute : Attribute
                                    {
                                    }

                                    /// <summary>
                                    ///   The actual underlying array of statuses.
                                    /// </summary>
                                    [SerializeField]
                                    private SolidnessStatus[] cells;

                                    /// <summary>
                                    ///   The mask width.
                                    /// </summary>
                                    [Delayed]
                                    [SerializeField]
                                    private uint width;

                                    /// <summary>
                                    ///   The mask height.
                                    /// </summary>
                                    [Delayed]
                                    [SerializeField]
                                    private uint height;

                                    /// <summary>
                                    ///   The mask width.
                                    /// </summary>
                                    public uint Width { get { return width; } }

                                    /// <summary>
                                    ///   The mask height.
                                    /// </summary>
                                    public uint Height { get { return height; } }

                                    /// <summary>
                                    ///   Creates a zero-sized mask. This one exists as an empty value for serialization.
                                    /// </summary>
                                    public SolidObjectMask() : this(0, 0, null) { }

                                    /// <summary>
                                    ///   Creates a new solid mask with the given data.
                                    /// </summary>
                                    /// <param name="width">The mask's width</param>
                                    /// <param name="height">The mask's height</param>
                                    /// <param name="cells">The mask's cells. Cells being Mask are an error: they will be converted to Ghost</param>
                                    public SolidObjectMask(uint width, uint height, SolidnessStatus[] cells)
                                    {
                                        if (width == 0 || height == 0)
                                        {
                                            this.width = 0;
                                            this.height = 0;
                                            this.cells = null;
                                            return;
                                        }

                                        if (cells == null)
                                        {
                                            throw new ArgumentNullException("cells");
                                        }

                                        uint length = width * height;
                                        if (length != cells.Length)
                                        {
                                            throw new ArgumentException("Width and height must multiply to the given array's length");
                                        }

                                        this.width = width;
                                        this.height = height;
                                        this.cells = new SolidnessStatus[length];
                                        for (int index = 0; index < length; index++)
                                        {
                                            SolidnessStatus status = cells[index];
                                            this.cells[index] = status == SolidnessStatus.Mask ? SolidnessStatus.Ghost : status;
                                        }
                                    }

                                    /// <summary>
                                    ///   Gets a single mask position in terms of inner (x, y) coordinates.
                                    ///   The (0, 0) point refers the bottom-left corner of the object.
                                    /// </summary>
                                    /// <param name="x">The given x position to query</param>
                                    /// <param name="y">The given y position to query</param>
                                    /// <returns>The status at the given position</returns>
                                    public SolidnessStatus this[uint x, uint y]
                                    {
                                        get
                                        {
                                            if (x >= width) throw new ArgumentOutOfRangeException("x");
                                            if (y >= height) throw new ArgumentOutOfRangeException("y");
                                            return cells[y * width + x];
                                        }
                                    }

                                    /// <summary>
                                    ///   Dumps the content of the array into a new array. This method should be used only
                                    ///     on edition or under highly controlled scenarios which do not occur frequently
                                    ///     because this will essentially be a performance killer if overused.
                                    /// </summary>
                                    /// <returns>A copy of the inner cells' statuses array</returns>
                                    public SolidnessStatus[] Dump()
                                    {
                                        if (this.cells == null) return null;
                                        int length = this.cells.Length;
                                        SolidnessStatus[] cells = new SolidnessStatus[length];
                                        for (int index = 0; index < length; index++)
                                        {
                                            SolidnessStatus status = this.cells[index];
                                            cells[index] = status == SolidnessStatus.Mask ? SolidnessStatus.Ghost : status;
                                        }
                                        return cells;
                                    }

                                    /// <summary>
                                    ///   Copies the current mask into a new size. If one of the dimensions grows and new
                                    ///     cells appear, they will be filled by default with the Ghost status, and more
                                    ///     precisely with the chosen type, if another one. A new mask will be returned,
                                    ///     and the current one will be unaffected.
                                    /// </summary>
                                    /// <param name="width">The new width</param>
                                    /// <param name="height">The new height</param>
                                    /// <param name="fill">The value to use when filling new cells</param>
                                    /// <returns>A new mask with the modified content</returns>
                                    public SolidObjectMask Resized(uint width, uint height, SolidnessStatus fill = SolidnessStatus.Ghost)
                                    {
                                        if (width == 0 || height == 0)
                                        {
                                            return new SolidObjectMask();
                                        }

                                        return new SolidObjectMask(width, height, ResizeAndFill(this.cells, this.width, this.height, width, height, fill));
                                    }

                                    // Resizes the given source mask contents, given their dimensions, new dimensions and fill.
                                    // A new mask contents array is returned. The original is unaffected.
                                    private static SolidnessStatus[] ResizeAndFill(SolidnessStatus[] source, uint sourceWidth, uint sourceHeight, uint width, uint height, SolidnessStatus fill)
                                    {
                                        SolidnessStatus[] newCells = new SolidnessStatus[width * height];
                                        uint targetIndex = 0;
                                        uint sourceVOffset = 0;
                                        uint minWidth = Values.Min(width, sourceWidth);
                                        uint minHeight = Values.Min(height, sourceHeight);
                                        for (int y = 0; y < minHeight; y++)
                                        {
                                            for (uint x = 0; x < minWidth; x++)
                                            {
                                                SolidnessStatus status = source[sourceVOffset + x];
                                                newCells[targetIndex++] = status == SolidnessStatus.Mask ? SolidnessStatus.Ghost : status;
                                            }
                                            for (uint x = minWidth; x < width; x++)
                                            {
                                                newCells[targetIndex++] = fill;
                                            }
                                            sourceVOffset += sourceWidth;
                                        }
                                        for (uint y = minHeight; y < height; y++)
                                        {
                                            for (uint x = 0; x < width; x++)
                                            {
                                                newCells[targetIndex++] = fill;
                                            }
                                            sourceVOffset += sourceHeight;
                                        }
                                        return newCells;
                                    }

                                    /// <summary>
                                    ///   Performs a resize of a given mask contents given its size, new size, and fill options. While the mask is 1-dimensional,
                                    ///   its source width and height must also be specified to compute it appropriately.
                                    /// </summary>
                                    /// <param name="source">The mask contents to resize.</param>
                                    /// <param name="sourceWidth">The width of the content.</param>
                                    /// <param name="sourceHeight">The height of the content.</param>
                                    /// <param name="width">The new width.</param>
                                    /// <param name="height">The new height.</param>
                                    /// <param name="fill">The fill for the new cells.</param>
                                    /// <returns></returns>
                                    public static SolidnessStatus[] Resized(SolidnessStatus[] source, uint sourceWidth, uint sourceHeight, uint width, uint height, SolidnessStatus fill)
                                    {
                                        if (width == 0 || height == 0)
                                        {
                                            return null;
                                        }

                                        if (sourceWidth * sourceHeight != source.Length)
                                        {
                                            throw new ArgumentException("Source dimensions do not match the source array");
                                        }

                                        return ResizeAndFill(source, sourceWidth, sourceHeight, width, height, fill);
                                    }

                                    /// <summary>
                                    ///   Clones the mask into a given one.
                                    /// </summary>
                                    /// <returns>The cloned mask</returns>
                                    public SolidObjectMask Clone()
                                    {
                                        return new SolidObjectMask(width, height, Dump());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
