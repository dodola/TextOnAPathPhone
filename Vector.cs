using System;
using System.Windows;

namespace MatrixMathHelpers
{
    public struct Vector
    {
        private Point _point;

        public Vector(double x, double y)
        {
            _point = new Point(x, y);
        }

        public double Length
        {
            get { return (Math.Sqrt(Math.Pow(_point.X, 2.0) + Math.Pow(_point.Y, 2.0))); }
        }

        public double X
        {
            get { return _point.X; }
            set { _point.X = value; }
        }

        public double Y
        {
            get { return _point.Y; }
            set { _point.Y = value; }
        }

        public void Normalize()
        {
            if (Length == 0)
                throw new InvalidOperationException("Vector Length is zero, can not normalize");

            double l = Length;
            _point.X /= l;
            _point.Y /= l;
        }

        //
        // Summary:
        //     Subtracts one specified vector from another.
        //
        // Parameters:
        //   vector1:
        //     The vector from which vector2 is subtracted.
        //
        //   vector2:
        //     The vector to subtract from vector1.
        //
        // Returns:
        //     The difference between vector1 and vector2.
        public static Vector operator -(Vector vector1, Vector vector2)

        {
            return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);
        }


        public static Vector operator -(Vector vector)
        {
            return new Vector(-vector.X, -vector.Y);
        }

        //
        // Summary:
        //     Multiplies the specified scalar by the specified vector and returns the resulting
        //     vector.
        //
        // Parameters:
        //   scalar:
        //     The scalar to multiply.
        //
        //   vector:
        //     The vector to multiply.
        //
        // Returns:
        //     The result of multiplying scalar and vector.
        public static Vector operator *(double scalar, Vector vector)
        {
            return new Vector(vector.X*scalar, vector.Y*scalar);
        }

        //
        // Summary:
        //     Multiplies the specified vector by the specified scalar and returns the resulting
        //     vector.
        //
        // Parameters:
        //   vector:
        //     The vector to multiply.
        //
        //   scalar:
        //     The scalar to multiply.
        //
        // Returns:
        //     The result of multiplying vector and scalar.
        public static Vector operator *(Vector vector, double scalar)
        {
            return new Vector(vector.X*scalar, vector.Y*scalar);
        }

        //
        // Summary:
        //     Transforms the coordinate space of the specified vector using the specified
        //     System.Windows.Media.Matrix.
        //
        // Parameters:
        //   vector:
        //     The vector to transform.
        //
        //   matrix:
        //     The transformation to apply to vector.
        //
        // Returns:
        //     The result of transforming vector by matrix.
        //public static Vector operator *(Vector vector, Matrix matrix)
        //{
        //    Vector result = new Vector();
        //    result.X = (matrix.M11*vector.X) + (matrix.M12*vector.Y);
        //    result.Y = (matrix.M21*vector.X) + (matrix.M22*vector.Y);
        //    return result;
        //}

        //
        // Summary:
        //     Calculates the dot product of the two specified vector structures and returns
        //     the result as a System.Double.
        //
        // Parameters:
        //   vector1:
        //     The first vector to multiply.
        //
        //   vector2:
        //     The second vector to multiply.
        //
        // Returns:
        //     Returns a System.Double containing the scalar dot product of vector1 and
        //     vector2, which is calculated using the following formula:vector1.X * vector2.X
        //     + vector1.Y * vector2.Y
        public static double operator *(Vector vector1, Vector vector2)
        {
            return (vector1.X*vector2.X) + (vector1.Y*vector2.Y);
        }

        //
        // Summary:
        //     Adds two vectors and returns the result as a vector.
        //
        // Parameters:
        //   vector1:
        //     The first vector to add.
        //
        //   vector2:
        //     The second vector to add.
        //
        // Returns:
        //     The sum of vector1 and vector2.
        public static Vector operator +(Vector vector1, Vector vector2)
        {
            return (new Vector(vector2.X + vector1.X, vector2.Y + vector1.Y));
        }

        //
        // Summary:
        //     Translates a point by the specified vector and returns the resulting point.
        //
        // Parameters:
        //   vector:
        //     The vector used to translate point.
        //
        //   point:
        //     The point to translate.
        //
        // Returns:
        //     The result of translating point by vector.
        public static Point operator +(Point point, Vector vector)
        {
            return new Point(point.X + vector.X, point.Y + vector.Y);
        }
    }
}