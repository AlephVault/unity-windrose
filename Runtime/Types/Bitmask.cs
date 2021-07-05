using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AlephVault.Unity.Support.Utils;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Types
    {
        /// <summary>
        ///   Bitmasks (or bitmaps) are bidimensional array of bits. They are usually intended for per-map logic involving
        ///     one boolean value on each (x, y) coordinate.
        /// </summary>
        public class Bitmask
        {
            /// <summary>
            ///   Check types are used when checking several cells together. They return the kind of aggregate value to return.
            ///   To this extent, we consider that "blocked" is true and "free" is false, for historical reasons.
            /// </summary>
            /// <remarks>
            ///   Consider the four aristotelic quantifiers for predicates. They are, actually, those operations.
            /// </remarks>
            public enum CheckType { ANY_BLOCKED, ANY_FREE, ALL_BLOCKED, ALL_FREE }
            private ulong[] bits;
            /// <summary>
            ///   Width of this bitmask.
            /// </summary>
            public readonly uint Width;
            /// <summary>
            ///   Height of this bitmask.
            /// </summary>
            public readonly uint Height;

            /// <summary>
            ///   Copy-constructs a bitmask.
            /// </summary>
            /// <param name="source">The source bitmask to copy.</param>
            public Bitmask(Bitmask source)
            {
                if (source == null) throw new ArgumentNullException("source");
                Width = source.Width;
                Height = source.Height;
                bits = source.Dump();
            }

            /// <summary>
            ///   Constructs a bitmask by specifying its data.
            ///   The data array must be of the appropriate size,
            ///     usually from parsing this from a resoruce file,
            ///     or an error will occur.
            /// </summary>
            /// <param name="width">Width of the bitmask.</param>
            /// <param name="height">Height of the bitmask.</param>
            /// <param name="initial">The data to initialize the bitmask with.</param>
            public Bitmask(uint width, uint height, ulong[] data)
            {
                uint size = width * height;
                if (size == 0) throw new ArgumentException("Both width and height must be > 0");
                Width = width;
                Height = height;
                if (data == null) throw new ArgumentNullException("data");
                if (data.Length != (size + 63) / 64) throw new ArgumentException(string.Format("The input data has an invalid length. For {0}x{1} the data size must be {2}",
                    width, height, (size + 63) / 64
                ));
                bits = (ulong[])data.Clone();
            }

            /// <summary>
            ///   Constructs a bitmask by filling with value.
            /// </summary>
            /// <param name="width">Width of the bitmask.</param>
            /// <param name="height">Height of the bitmask.</param>
            /// <param name="initial">Initial fill of each bitmask cell.</param>
            public Bitmask(uint width, uint height, bool initial = false)
            {
                uint size = width * height;
                if (size == 0) throw new ArgumentException("Both width and height must be > 0");
                bits = new ulong[(size + 63) / 64];
                Width = width;
                Height = height;
                Fill(initial);
            }

            /// <summary>
            ///   Gets or sets an individual bit at certain (x, y) position.
            /// </summary>
            /// <param name="x">The X coordinate.</param>
            /// <param name="y">The Y coordinate.</param>
            /// <returns>Boolean value of the bit.</returns>
            public bool this[uint x, uint y]
            {
                get
                {
                    uint flat_index = y * Width + x;
                    return (bits[flat_index / 64] & (1ul << (int)(flat_index % 64))) != 0;
                }
                set
                {
                    uint flat_index = y * Width + x;
                    if (value)
                    {
                        bits[flat_index / 64] |= (1ul << (int)(flat_index % 64));
                    }
                    else
                    {
                        bits[flat_index / 64] &= ~(1ul << (int)(flat_index % 64));
                    }
                }
            }

            /// <summary>
            ///   Fills the bitmask with certain value on each cell.
            /// </summary>
            /// <param name="value">The value to fill.</param>
            public void Fill(bool value)
            {
                ulong filler = value ? ulong.MaxValue : 0;
                for (int x = 0; x < bits.Length; x++) bits[x] = filler;
            }

            /// <summary>
            ///   Creates a new bitmask based on this one, but modified in size and/or translated in offset.
            /// </summary>
            /// <remarks>
            ///   <para>Think as if you do it manually using old versions of MS Paint for a monochrome picture.</para>
            ///   <para>First, you will create your new picture, filling with full black or full white.</para>
            ///   <para>Then you go to your former picture and select-all and copy.</para>
            ///   <para>Then you go back to this picture, select the rectangle-selector, and place somewhere a tiny 1x1 selection.</para>
            ///   <para>Then you paste the content, choosing to not modify the dimensions of the drawing if it doesn't fit but instead clip the pasted content.</para>
            /// </remarks>
            /// <param name="newWidth">The width of the new bitmask.</param>
            /// <param name="newHeight">The height of the new bitmask.</param>
            /// <param name="offsetX">The X offset on which the current content will be pasted.</param>
            /// <param name="offsetY">The Y offset on which the current content will be pasted.</param>
            /// <param name="newFillingValue">The initial value for the destination bitmask.</param>
            /// <returns>The new bitmask.</returns>
            public Bitmask Translated(uint newWidth, uint newHeight, int offsetX, int offsetY, bool newFillingValue = false)
            {
                Bitmask result = new Bitmask(newWidth, newHeight, newFillingValue);
                if (offsetX < newWidth && offsetY < newHeight && offsetX + newWidth > 0 && offsetY + newHeight > 0)
                {
                    // startx and endx, like their y-siblings, belong to the translated array.
                    int startX = Values.Max<int>(offsetX, 0);
                    int endX = Values.Min<int>(offsetX + (int)Width, (int)newWidth);
                    int startY = Values.Max<int>(offsetY, 0);
                    int endY = Values.Min<int>(offsetY + (int)Height, (int)newHeight);

                    // origin array use coordinates subtracting offsetX and offsetY.
                    for (int x = startX; x < endX; x++)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            result[(uint)x, (uint)y] = this[(uint)(x - offsetX), (uint)(y - offsetY)];
                        }
                    }
                }
                return result;
            }

            /// <summary>
            ///   Clones the bitmask. Essentially, by calling its copy constructor.
            /// </summary>
            /// <returns>The cloned bitmask.</returns>
            public Bitmask Clone()
            {
                return new Bitmask(this);
            }

            /// <summary>
            ///   Computes an in-place bitwise OR from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   Clone the current bitmask if you don't want an in-place operation, or use the | operator.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public void Unite(Bitmask other)
            {
                CheckSameDimensions(other);
                int l = bits.Length;
                for (int i = 0; i < l; i++)
                {
                    bits[i] |= other.bits[i];
                }
            }

            /// <summary>
            ///   Computes an in-place bitwise AND from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   Clone the current bitmask if you don't want an in-place operation, or use the & operator.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public void Intersect(Bitmask other)
            {
                CheckSameDimensions(other);
                int l = bits.Length;
                for (int i = 0; i < l; i++)
                {
                    bits[i] &= other.bits[i];
                }
            }

            /// <summary>
            ///   Computes an in-place bitwise difference (this & ~that) from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   Clone the current bitmask if you don't want an in-place operation, or use the - operator.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public void Subtract(Bitmask other)
            {
                CheckSameDimensions(other);
                int l = bits.Length;
                for (int i = 0; i < l; i++)
                {
                    bits[i] &= ~other.bits[i];
                }
            }

            /// <summary>
            ///   Computes an in-place bitwise symmetric difference (this ^ that) from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   Clone the current bitmask if you don't want an in-place operation, or use the ^ operator.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public void SymmetricSubtract(Bitmask other)
            {
                CheckSameDimensions(other);
                int l = bits.Length;
                for (int i = 0; i < l; i++)
                {
                    bits[i] ^= other.bits[i];
                }
            }

            /// <summary>
            ///   Computes an in-place bitwise NOT.
            /// </summary>
            /// <remarks>
            ///   Clone the current bitmask if you don't want an in-place operation, or use the ~ operator.
            /// </remarks>
            public void Invert()
            {
                int l = bits.Length;
                for (int i = 0; i < l; i++)
                {
                    bits[i] = ~bits[i];
                }
            }

            /// <summary>
            ///   Computes a bitwise OR from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   <see cref="Unite(Bitmask)"/> for in-place operation.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public static Bitmask operator |(Bitmask self, Bitmask other)
            {
                Bitmask result = self.Clone();
                result.Unite(other);
                return result;
            }

            /// <summary>
            ///   Computes a bitwise AND from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   <see cref="Intersect(Bitmask)"/> for in-place operation.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public static Bitmask operator &(Bitmask self, Bitmask other)
            {
                Bitmask result = self.Clone();
                result.Intersect(other);
                return result;
            }

            /// <summary>
            ///   Computes a bitwise NOT.
            /// </summary>
            /// <remarks>
            ///   <see cref="Invert()"/> for in-place operation.
            /// </remarks>
            public static Bitmask operator ~(Bitmask self)
            {
                Bitmask result = self.Clone();
                result.Invert();
                return result;
            }

            /// <summary>
            ///   Computes a bitwise difference from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   <see cref="Subtract(Bitmask)"/> for in-place operation.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public static Bitmask operator -(Bitmask self, Bitmask other)
            {
                Bitmask result = self.Clone();
                result.Subtract(other);
                return result;
            }

            /// <summary>
            ///   Computes a bitwise symmetric difference from another mask with same dimensions.
            /// </summary>
            /// <remarks>
            ///   <see cref="Unite(Bitmask)"/> for in-place operations.
            /// </remarks>
            /// <param name="other">The other bitmask.</param>
            /// <exception cref="ArgumentException" /> 
            public static Bitmask operator ^(Bitmask self, Bitmask other)
            {
                Bitmask result = self.Clone();
                result.SymmetricSubtract(other);
                return result;
            }

            /// <summary>
            ///   Fills a square with a certain value.
            /// </summary>
            /// <remarks>
            ///   Due to historical reasons, the argument holding the value is <c>blocked</c>.
            /// </remarks>
            /// <param name="xi">Initial X coordinate.</param>
            /// <param name="yi">Initial Y coordinate.</param>
            /// <param name="xf">Final X coordinate, included.</param>
            /// <param name="yf">Final Y coordinate, included.</param>
            /// <param name="blocked">The value to fill.</param>
            public void SetSquare(uint xi, uint yi, uint xf, uint yf, bool blocked)
            {
                xi = Values.Clamp<uint>(0, xi, Width - 1);
                yi = Values.Clamp<uint>(0, yi, Height - 1);
                xf = Values.Clamp<uint>(0, xf, Width - 1);
                yf = Values.Clamp<uint>(0, yf, Height - 1);

                uint xi_ = Values.Min<uint>(xi, xf);
                uint xf_ = Values.Max<uint>(xi, xf);
                uint yi_ = Values.Min<uint>(yi, yf);
                uint yf_ = Values.Max<uint>(yi, yf);

                for (uint x = xi_; x <= xf_; x++)
                {
                    for (uint y = yi_; y <= yf_; y++)
                    {
                        this[x, y] = blocked;
                    }
                }
            }

            /// <summary>
            ///   Gets a single flag value as an aggregate of the contents of a square.
            /// </summary>
            /// <param name="xi">Initial X coordinate.</param>
            /// <param name="yi">Initial Y coordinate.</param>
            /// <param name="xf">Final X coordinate, included.</param>
            /// <param name="yf">Final Y coordinate, included.</param>
            /// <param name="checkType">The aggregate operation to consider. See <see cref="CheckType"/> for more details.</param>
            /// <returns>The aggregated flag value.</returns>
            public bool GetSquare(uint xi, uint yi, uint xf, uint yf, CheckType checkType)
            {
                xi = Values.Clamp<uint>(0, xi, Width - 1);
                yi = Values.Clamp<uint>(0, yi, Height - 1);
                xf = Values.Clamp<uint>(0, xf, Width - 1);
                yf = Values.Clamp<uint>(0, yf, Height - 1);

                uint xi_ = Values.Min<uint>(xi, xf);
                uint xf_ = Values.Max<uint>(xi, xf);
                uint yi_ = Values.Min<uint>(yi, yf);
                uint yf_ = Values.Max<uint>(yi, yf);

                for (uint x = xi_; x <= xf_; x++)
                {
                    for (uint y = yi_; y <= yf_; y++)
                    {
                        switch (checkType)
                        {
                            case CheckType.ANY_BLOCKED:
                                if (this[x, y]) { return true; }
                                break;
                            case CheckType.ANY_FREE:
                                if (!this[x, y]) { return true; }
                                break;
                            case CheckType.ALL_BLOCKED:
                                if (!this[x, y]) { return false; }
                                break;
                            case CheckType.ALL_FREE:
                                if (this[x, y]) { return false; }
                                break;
                            default:
                                return false;
                        }
                    }
                }
                switch (checkType)
                {
                    case CheckType.ALL_BLOCKED:
                    case CheckType.ALL_FREE:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            ///   A special case of <see cref="SetSquare(uint, uint, uint, uint, bool)"/> on a single row.
            /// </summary>
            /// <param name="xi">Initial X coordinate.</param>
            /// <param name="xf">Final X coordinate, included.</param>
            /// <param name="y">Y coordinate.</param>
            /// <param name="blocked">The value to fill.</param>
            public void SetRow(uint xi, uint xf, uint y, bool blocked)
            {
                SetSquare(xi, y, xf, y, blocked);
            }

            /// <summary>
            ///   A special case of <see cref="GetSquare(uint, uint, uint, uint, CheckType)"/> on a single row. 
            /// </summary>
            /// <param name="xi">Initial X coordinate.</param>
            /// <param name="xf">Final X coordinate, included.</param>
            /// <param name="y">Y coordinate.</param>
            /// <param name="checkType">The aggregate operation to consider. See <see cref="CheckType"/> for more details.</param>
            /// <returns>The aggregated flag value.</returns>
            public bool GetRow(uint xi, uint xf, uint y, CheckType checkType)
            {
                return GetSquare(xi, y, xf, y, checkType);
            }

            /// <summary>
            ///   A special case of <see cref="SetSquare(uint, uint, uint, uint, bool)"/> on a single column. 
            /// </summary>
            /// <param name="x">X coordinate.</param>
            /// <param name="yi">Initial Y coordinate.</param>
            /// <param name="yf">Final Y coordinate.</param>
            /// <param name="blocked">The value to fill.</param>
            public void SetColumn(uint x, uint yi, uint yf, bool blocked)
            {
                SetSquare(x, yi, x, yf, blocked);
            }

            /// <summary>
            ///   A special case of <see cref="GetSquare(uint, uint, uint, uint, CheckType)"/> on a single column. 
            /// </summary>
            /// <param name="x">X coordinate.</param>
            /// <param name="yi">Initial Y coordinate.</param>
            /// <param name="yf">Final Y coordinate, included.</param>
            /// <param name="checkType">The aggregate operation to consider. See <see cref="CheckType"/> for more details.</param>
            /// <returns>The aggregated flag value.</returns>
            public bool GetColumn(uint x, uint yi, uint yf, CheckType checkType)
            {
                return GetSquare(x, yi, x, yf, checkType);
            }

            /// <summary>
            ///   Sets the value of a single cell.
            /// </summary>
            /// <remarks>
            ///   This function is a safer layer over <see cref="this[uint, uint]"/>. It clamps the values to
            ///     ensure appropriate access.
            /// </remarks>
            /// <param name="x">X coordinate.</param>
            /// <param name="y">Y coordinate.</param>
            /// <param name="blocked">The value to fill.</param>
            public void SetCell(uint x, uint y, bool blocked)
            {
                this[Values.Clamp<uint>(0, x, Width - 1), Values.Clamp<uint>(0, y, Height - 1)] = blocked;
            }

            /// <summary>
            ///   Gets the value of a single cell.
            /// </summary>
            /// <remarks>
            ///   This function is a safer layer over <see cref="this[uint, uint]"/>. It clamps the values to
            ///     ensure appropriate access.
            /// </remarks>
            /// <param name="x">X coordinate.</param>
            /// <param name="y">Y coordinate.</param>
            /// <returns>The flag.</returns>
            public bool GetCell(uint x, uint y)
            {
                return this[Values.Clamp<uint>(0, x, Width - 1), Values.Clamp<uint>(0, y, Height - 1)];
            }

            /// <summary>
            ///   Dumps all the bits of this mask.
            /// </summary>
            /// <returns>A copy of the internal bits.</returns>
            public ulong[] Dump()
            {
                return (ulong[])bits.Clone();
            }

            private void CheckSameDimensions(Bitmask other)
            {
                if (Width != other.Width || Height != other.Height) throw new ArgumentException("Dimensions must match between bitmasks");
            }
        }
    }
}
