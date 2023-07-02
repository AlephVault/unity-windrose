using System;
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
                                ///   Solidness masks are an bidimensional mask of integers telling to what extent is the
                                ///     map occupied. Dimensions of this mask will match the dimensions of the map needing
                                ///     it, and each cell's values involves to what degree the cell is overlapped by
                                ///     solid or solid-for-others (or hole) objects.
                                /// </summary>
                                public class SolidMask
                                {
                                    /// <summary>
                                    ///   Tells when the involved object bounds are not valid, in position/dimensions, to
                                    ///     this mask.
                                    /// </summary>
                                    public class InvalidSpatialSpecException : Types.Exception
                                    {
                                        public InvalidSpatialSpecException() { }
                                        public InvalidSpatialSpecException(string message) : base(message) { }
                                        public InvalidSpatialSpecException(string message, Exception inner) : base(message, inner) { }
                                    }

                                    /// <summary>
                                    ///   Tells whether a cell's value cannot be decremented due to overflow.
                                    /// </summary>
                                    public class CannotDecrementException : Types.Exception
                                    {
                                        public CannotDecrementException() { }
                                        public CannotDecrementException(string message) : base(message) { }
                                        public CannotDecrementException(string message, Exception inner) : base(message, inner) { }
                                    }

                                    /// <summary>
                                    ///   Tells whether a cell's value cannot be incremented due to overflow.
                                    /// </summary>
                                    public class CannotIncrementException : Types.Exception
                                    {
                                        public CannotIncrementException() { }
                                        public CannotIncrementException(string message) : base(message) { }
                                        public CannotIncrementException(string message, Exception inner) : base(message, inner) { }
                                    }

                                    /// <summary>
                                    ///   Tells whether a cell's value cannot by incremented/decremented (by an
                                    ///     arbitrary amount) due to overflow.
                                    /// </summary>
                                    public class CannotChangeException : Types.Exception
                                    {
                                        public CannotChangeException() { }
                                        public CannotChangeException(string message) : base(message) { }
                                        public CannotChangeException(string message, Exception inner) : base(message, inner) { }
                                    }

                                    /// <summary>
                                    ///   Mask dimensions.
                                    /// </summary>
                                    public readonly uint width, height;
                                    private short[] positions;

                                    public SolidMask(uint width, uint height)
                                    {
                                        this.width = Values.Clamp(1, width, Map.MaxWidth);
                                        this.height = Values.Clamp(1, height, Map.MaxHeight);
                                        this.positions = new short[this.width * this.height];
                                        Array.Clear(this.positions, 0, (int)(this.width * this.height));
                                    }

                                    private void CheckDimensions(uint x, uint y, uint width, uint height)
                                    {
                                        if (x + width > this.width || y + height > this.height)
                                        {
                                            throw new InvalidSpatialSpecException("Dimensions " + width + "x" + height + " starting at (" + x + ", " + y + ") cannot be contained on a map of " + this.width + "x" + this.height);
                                        }
                                    }

                                    /// <summary>
                                    ///   Increments by 1 all the cells in the given bounds inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down-left corner</param>
                                    /// <param name="y">The Y coordinate of the square's down-left corner</param>
                                    /// <param name="width">The width of the square</param>
                                    /// <param name="height">The height of the square</param>
                                    public void IncSquare(uint x, uint y, uint width, uint height)
                                    {
                                        CheckDimensions(x, y, width, height);
                                        uint yEnd = y + height;
                                        for (uint j = y; j < yEnd; j++)
                                        {
                                            uint offset = j * this.width + x;
                                            for (uint i = 0; i < width; i++)
                                            {
                                                if (this.positions[offset] < short.MaxValue)
                                                {
                                                    this.positions[offset++]++;
                                                }
                                                else
                                                {
                                                    throw new CannotIncrementException("Cannot increment position (" + x + ", " + y + ") beyond its maximum");
                                                }
                                            }
                                        }
                                    }

                                    /// <summary>
                                    ///   Increments by 1 all the cells in the given row inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's left cell</param>
                                    /// <param name="y">The Y coordinate of the square's left cell</param>
                                    /// <param name="width">The width of the row</param>
                                    public void IncRow(uint x, uint y, uint width)
                                    {
                                        IncSquare(x, y, width, 1);
                                    }

                                    /// <summary>
                                    ///   Increments by 1 all the cells in the given column inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down cell</param>
                                    /// <param name="y">The Y coordinate of the square's down cell</param>
                                    /// <param name="height">The height of the column</param>
                                    public void IncColumn(uint x, uint y, uint height)
                                    {
                                        IncSquare(x, y, 1, height);
                                    }

                                    /// <summary>
                                    ///   Decrements by 1 all the cells in the given bounds inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down-left corner</param>
                                    /// <param name="y">The Y coordinate of the square's down-left corner</param>
                                    /// <param name="width">The width of the square</param>
                                    /// <param name="height">The height of the square</param>
                                    public void DecSquare(uint x, uint y, uint width, uint height)
                                    {
                                        CheckDimensions(x, y, width, height);
                                        uint yEnd = y + height;
                                        for (uint j = y; j < yEnd; j++)
                                        {
                                            uint offset = j * this.width + x;
                                            for (uint i = 0; i < width; i++)
                                            {
                                                if (this.positions[offset] > short.MinValue)
                                                {
                                                    this.positions[offset++]--;
                                                }
                                                else
                                                {
                                                    throw new CannotIncrementException("Cannot decrement position (" + x + ", " + y + ") beyond its minimum");
                                                }
                                            }
                                        }
                                    }

                                    /// <summary>
                                    ///   Decrements by 1 all the cells in the given row inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's left cell</param>
                                    /// <param name="y">The Y coordinate of the square's left cell</param>
                                    /// <param name="width">The width of the row</param>
                                    public void DecRow(uint x, uint y, uint width)
                                    {
                                        DecSquare(x, y, width, 1);
                                    }

                                    /// <summary>
                                    ///   Decrements by 1 all the cells in the given column inside the mask.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down cell</param>
                                    /// <param name="y">The Y coordinate of the square's down cell</param>
                                    /// <param name="height">The height of the column</param>
                                    public void DecColumn(uint x, uint y, uint height)
                                    {
                                        DecSquare(x, y, 1, height);
                                    }

                                    /// <summary>
                                    ///   Checks whether all the cells in the given square are empty.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down-left corner</param>
                                    /// <param name="y">The Y coordinate of the square's down-left corner</param>
                                    /// <param name="width">The width of the square</param>
                                    /// <param name="height">The height of the square</param>
                                    /// <returns><c>true</c> if all the checked cells are empty. <c>false</c> if at least one is not</returns>
                                    public bool EmptySquare(uint x, uint y, uint width, uint height)
                                    {
                                        CheckDimensions(x, y, width, height);
                                        uint yEnd = y + height;
                                        for (uint j = y; j < yEnd; j++)
                                        {
                                            uint offset = j * this.width + x;

                                            for (uint i = 0; i < width; i++)
                                            {
                                                if (this.positions[offset++] > 0)
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                        return true;
                                    }

                                    /// <summary>
                                    ///   Checks whether all the cells in the given row are empty.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down-left corner</param>
                                    /// <param name="y">The Y coordinate of the square's down-left corner</param>
                                    /// <param name="width">The width of the square</param>
                                    /// <param name="height">The height of the square</param>
                                    /// <returns><c>true</c> if all the checked cells are empty. <c>false</c> if at least one is not</returns>
                                    public bool EmptyRow(uint x, uint y, uint width)
                                    {
                                        return EmptySquare(x, y, width, 1);
                                    }

                                    /// <summary>
                                    ///   Checks whether all the cells in the given column are empty.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the square's down-left corner</param>
                                    /// <param name="y">The Y coordinate of the square's down-left corner</param>
                                    /// <param name="width">The width of the square</param>
                                    /// <param name="height">The height of the square</param>
                                    /// <returns><c>true</c> if all the checked cells are empty. <c>false</c> if at least one is not</returns>
                                    public bool EmptyColumn(uint x, uint y, uint height)
                                    {
                                        return EmptySquare(x, y, 1, height);
                                    }

                                    /// <summary>
                                    ///   Changes a given cell's value by the given (positive or negative)
                                    ///     amount.
                                    /// </summary>
                                    /// <param name="x">The X coordinate of the cell to change</param>
                                    /// <param name="y">The Y coordinate of the cell to change</param>
                                    /// <param name="amount">The amount to change the ccell by</param>
                                    public void ChangeCellBy(uint x, uint y, short amount)
                                    {
                                        uint offset = y * width + x;
                                        short current = positions[offset];
                                        if (amount > (short.MaxValue - current) || amount < (short.MinValue - current))
                                        {
                                            throw new CannotChangeException("Cannot increment position (" + x + ", " + y + ") beyond its boundaries");
                                        }
                                        positions[offset] += amount;
                                    }

                                    /// <summary>
                                    ///   Gets the occupancy of a single cell
                                    /// </summary>
                                    /// <param name="x">The X coordinate to check</param>
                                    /// <param name="y">The Y coordinate to check</param>
                                    /// <returns>Whether the cell is occupied</returns>
                                    public bool this[uint x, uint y]
                                    {
                                        get
                                        {
                                            return positions[y * width + x] == 0;
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
}