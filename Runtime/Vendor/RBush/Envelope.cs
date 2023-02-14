using AlephVault.Unity.Support.Utils;

namespace GameMeanMachine.Unity.WindRose.Vendor.RBush
{
	/// <summary>
	/// A bounding envelope, used to identify the bounds of of the points within
	/// a particular node.
	/// </summary>
	/// <param name="MinX">The minimum X value of the bounding box.</param>
	/// <param name="MinY">The minimum Y value of the bounding box.</param>
	/// <param name="MaxX">The maximum X value of the bounding box.</param>
	/// <param name="MaxY">The maximum Y value of the bounding box.</param>
	public struct Envelope
	{
		public double MinX;
		public double MinY;
		public double MaxX;
		public double MaxY;

		/// <summary>
		/// The calculated area of the bounding box.
		/// </summary>
		public double Area => Values.Max(MaxX - MinX, 0) * Values.Max(MaxY - MinY, 0);

		/// <summary>
		/// Half of the linear perimeter of the bounding box
		/// </summary>
		public double Margin => Values.Max(MaxX - MinX, 0) + Values.Max(MaxY - MinY, 0);

		/// <summary>
		/// Extends a bounding box to include another bounding box
		/// </summary>
		/// <param name="other">The other bounding box</param>
		/// <returns>A new bounding box that encloses both bounding boxes.</returns>
		/// <remarks>Does not affect the current bounding box.</remarks>
		public Envelope Extend(in Envelope other)
		{
			return new Envelope
			{
				MinX = Values.Min(MinX, other.MinX),
				MinY = Values.Min(MinY, other.MinY),
				MaxX = Values.Max(MaxX, other.MaxX),
				MaxY = Values.Max(MaxY, other.MaxY)
			};
		}

		/// <summary>
		/// Intersects a bounding box to only include the common area
		/// of both bounding boxes
		/// </summary>
		/// <param name="other">The other bounding box</param>
		/// <returns>A new bounding box that is the intersection of both bounding boxes.</returns>
		/// <remarks>Does not affect the current bounding box.</remarks>
		public Envelope Intersection(in Envelope other)
		{
			return new Envelope
			{
				MinX = Values.Max(MinX, other.MinX),
				MinY = Values.Max(MinY, other.MinY),
				MaxX = Values.Min(MaxX, other.MaxX),
				MaxY = Values.Min(MaxY, other.MaxY)
			};
		}

		/// <summary>
		/// Determines whether <paramref name="other"/> is contained
		/// within this bounding box.
		/// </summary>
		/// <param name="other">The other bounding box</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="other"/> is
		/// completely contained within this bounding box; 
		/// <see langword="false" /> otherwise.
		/// </returns>
		public bool Contains(in Envelope other) =>
			this.MinX <= other.MinX &&
			this.MinY <= other.MinY &&
			this.MaxX >= other.MaxX &&
			this.MaxY >= other.MaxY;

		/// <summary>
		/// Determines whether <paramref name="other"/> intersects
		/// this bounding box.
		/// </summary>
		/// <param name="other">The other bounding box</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="other"/> is
		/// intersects this bounding box in any way; 
		/// <see langword="false" /> otherwise.
		/// </returns>
		public bool Intersects(in Envelope other) =>
			this.MinX <= other.MaxX &&
			this.MinY <= other.MaxY &&
			this.MaxX >= other.MinX &&
			this.MaxY >= other.MinY;

		/// <summary>
		/// A bounding box that contains the entire 2-d plane.
		/// </summary>
		public static Envelope InfiniteBounds { get; } = new Envelope
		{
			MinX = double.NegativeInfinity,
			MinY = double.NegativeInfinity,
			MaxX = double.PositiveInfinity,
			MaxY = double.PositiveInfinity
		};

		/// <summary>
		/// An empty bounding box.
		/// </summary>
		public static Envelope EmptyBounds { get; } = new Envelope {
			MinX = double.PositiveInfinity,
			MinY = double.PositiveInfinity,
			MaxX = double.NegativeInfinity,
			MaxY = double.NegativeInfinity
		};
	}
	
}
