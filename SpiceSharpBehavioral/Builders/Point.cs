namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A point with two coordinates.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        public double X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        public double Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
