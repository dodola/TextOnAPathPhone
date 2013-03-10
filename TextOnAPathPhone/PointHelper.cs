using System.Windows;

namespace MatrixMathHelpers
{
    public class PointHelper
    {
        private Point _value;

        public PointHelper(Point Input)
        {
            _value = Input;
        }

        public PointHelper()
        {
        }

        public Point Value
        {
            get { return _value; }
            set { _value = value; }
        }

        //public static explicit operator Vector(PointHelper point)
        //{
        //    return (new Vector(point._value.X, point._value.Y));
        //}

        public static Vector operator -(PointHelper point1, PointHelper point2)
        {
            return new Vector(point1._value.X - point2._value.X, point1._value.Y - point2._value.Y);
        }

        //
        // Summary:
        //     Translates the specified System.Windows.Point by the specified System.Windows.Vector
        //     and returns the result.
        //
        // Parameters:
        //   point:
        //     The point to translate.
        //
        //   vector:
        //     The amount by which to translate point.
        //
        // Returns:
        //     The result of translating the specified point by the specified vector.

        public static PointHelper operator +(PointHelper point, Vector vector)
        {
            var result = new PointHelper();

            result._value.X = vector.X + point.Value.X;
            result._value.Y = vector.Y + point.Value.Y;

            return result;
        }
    }
}