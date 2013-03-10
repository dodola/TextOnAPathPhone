using System;
using System.Windows.Media;

namespace MatrixMathHelpers
{
    public class TransformHelper
    {
        //
        // Summary:
        //     Gets the current transformation as a System.Windows.Media.Matrix object.
        //
        // Returns:
        //     The current matrix transformation.
        public static Matrix GetTransformValue(Transform Transform)
        {
            Matrix m = Matrix.Identity;

            if (Transform is TransformGroup)
            {
                var transformGroup = Transform as TransformGroup;

                m = transformGroup.Value;
            }
            else if (Transform is RotateTransform)
            {
                var rotateTransform = Transform as RotateTransform;

                double angleRadians = rotateTransform.Angle*Math.PI/180;
                m.M11 = Math.Cos(angleRadians);
                m.M12 = Math.Sin(angleRadians);
                m.M21 = -m.M12;
                m.M22 = m.M11;
            }
            else if (Transform is TranslateTransform)
            {
                var translateTranform = Transform as TranslateTransform;
                m.OffsetX = translateTranform.X;
                m.OffsetY = translateTranform.Y;
            }
            else if (Transform is ScaleTransform)
            {
                var scaleTransform = Transform as ScaleTransform;
                m.M11 = scaleTransform.ScaleX;
                m.M12 = 0;
                m.M21 = 0;
                m.M22 = scaleTransform.ScaleY;
            }
            else if (Transform is SkewTransform)
            {
                throw new Exception("Sorry Skew not implemented");
            }

            return m;
        }
    }
}