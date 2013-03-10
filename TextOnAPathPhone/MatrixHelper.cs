using System;
using System.Windows;
using System.Windows.Media;

namespace MatrixMathHelpers
{
    /// <summary>
    ///     Helper class to Silverlight Matrix struct which lacks some methods
    ///     Note: Matrix struct represents a 3x3 affine transformation matrix
    ///     used for transformations in 2-D space.
    /// </summary>
    public class MatrixHelper
    {
        private Matrix _value;

        public MatrixHelper()
        {
            _value = Matrix.Identity;
        }

        public MatrixHelper(Matrix Input)
        {
            _value = Input;
        }

        public Matrix Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool HasInverse
        {
            get
            {
                if (Determinant == 0)
                    return false;

                return true;
            }
        }

        public double Determinant
        {
            get { return (Value.M11 * Value.M22) - (Value.M12 * Value.M21); }
        }

        public static MatrixHelper operator *(MatrixHelper Matrix1, MatrixHelper Matrix2)
        {
            var result = new MatrixHelper();

            result._value.M11 = (Matrix1.Value.M11 * Matrix2.Value.M11) + (Matrix1.Value.M12 * Matrix2.Value.M21);
            result._value.M12 = (Matrix1.Value.M11 * Matrix2.Value.M12) + (Matrix1.Value.M12 * Matrix2.Value.M22);
            result._value.M21 = (Matrix1.Value.M21 * Matrix2.Value.M11) + (Matrix1.Value.M22 * Matrix2.Value.M21);
            result._value.M22 = (Matrix1.Value.M21 * Matrix2.Value.M12) + (Matrix1.Value.M22 * Matrix2.Value.M22);
            result._value.OffsetX = (Matrix1.Value.OffsetX * Matrix2.Value.M11) +
                                    (Matrix1.Value.OffsetY * Matrix2.Value.M21) + Matrix2.Value.OffsetX;
            result._value.OffsetY = (Matrix1.Value.OffsetX * Matrix2.Value.M12) +
                                    (Matrix1.Value.OffsetY * Matrix2.Value.M22) + Matrix2.Value.OffsetY;

            return (result);
        }

        public static MatrixHelper Multiply(MatrixHelper Matrix1, MatrixHelper Matrix2)
        {
            return (Matrix1 * Matrix2);
        }

        public Point Transform(Point Point)
        {
            var result = new Point();

            result.X = (Value.M11 * Point.X) + (Value.M12 * Point.Y);
            result.Y = (Value.M21 * Point.X) + (Value.M22 * Point.Y);

            return (result);
        }

        public void Rotate(double Angle)
        {
            double angleRadians = Angle * Math.PI / 180.0;

            var rotationMatrix = new MatrixHelper();
            rotationMatrix._value.M11 = Math.Cos(angleRadians);
            rotationMatrix._value.M12 = Math.Sin(angleRadians);
            rotationMatrix._value.M21 = -rotationMatrix.Value.M12;
            rotationMatrix._value.M22 = rotationMatrix.Value.M11;

            MatrixHelper result = this * rotationMatrix;
            _value.M11 = result.Value.M11;
            _value.M12 = result.Value.M12;
            _value.M21 = result.Value.M21;
            _value.M22 = result.Value.M22;
            _value.OffsetX = result.Value.OffsetX;
            _value.OffsetY = result.Value.OffsetY;
        }

        public void Scale(double XScale, double YScale)
        {
            var scaleMatrix = new MatrixHelper();
            scaleMatrix._value.M11 = XScale;
            scaleMatrix._value.M12 = 0;
            scaleMatrix._value.M21 = 0;
            scaleMatrix._value.M22 = YScale;

            MatrixHelper result = this * scaleMatrix;
            _value.M11 = result.Value.M11;
            _value.M12 = result.Value.M12;
            _value.M21 = result.Value.M21;
            _value.M22 = result.Value.M22;
            _value.OffsetX = result.Value.OffsetX;
            _value.OffsetY = result.Value.OffsetY;
        }

        public void Invert()
        {
            if (Determinant == 0)
                throw new InvalidOperationException("Matrix can not be inverted, determinant is zero");

            double M11 = _value.M22 / Determinant;
            double M12 = -_value.M12 / Determinant;
            double M21 = -_value.M21 / Determinant;
            double M22 = _value.M11 / Determinant;
            double xOffset = ((_value.OffsetY * _value.M21) - (_value.OffsetX * _value.M22)) / Determinant;
            double yOffset = ((_value.OffsetX * _value.M12) - (_value.OffsetY * _value.M11)) / Determinant;

            _value.M11 = M11;
            _value.M12 = M12;
            _value.M21 = M21;
            _value.M22 = M22;
            _value.OffsetX = xOffset;
            _value.OffsetY = yOffset;
        }
    }
}