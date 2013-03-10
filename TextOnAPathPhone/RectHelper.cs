using System.Windows;

namespace MatrixMathHelpers
{
    public class RectHelper
    {
        private Rect _value;

        public RectHelper(Rect Input)
        {
            _value = Input;
        }

        public RectHelper()
        {
        }

        public Rect Value
        {
            get { return _value; }
            set { _value = value; }
        }

        // Summary:
        //     Gets the position of the top-left corner of the rectangle.
        //
        // Returns:
        //     The position of the top-left corner of the rectangle.
        public Point TopLeft
        {
            get { return (new Point(_value.X, _value.Y)); }
        }

        //
        // Summary:
        //     Gets the position of the top-right corner of the rectangle.
        //
        // Returns:
        //     The position of the top-right corner of the rectangle.
        public Point TopRight
        {
            get { return (new Point(_value.X + _value.Width, _value.Y)); }
        }

        //
        // Summary:
        //     Gets the position of the bottom-left corner of the rectangle
        //
        // Returns:
        //     The position of the bottom-left corner of the rectangle.
        public Point BottomLeft
        {
            get { return (new Point(_value.X, _value.Y + _value.Height)); }
        }

        //
        // Summary:
        //     Gets the position of the bottom-right corner of the rectangle.
        //
        // Returns:
        //     The position of the bottom-right corner of the rectangle.
        public Point BottomRight
        {
            get { return (new Point(_value.X + _value.Width, _value.Y + _value.Height)); }
        }
    }
}