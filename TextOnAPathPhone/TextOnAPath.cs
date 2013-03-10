using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Petzold.Shapes;


namespace TextOnAPath
{
    [ContentProperty("Text")]
    public class TextOnAPath : Control
    {
        static TextOnAPath()
        {

        }

        public static readonly DependencyProperty GeoPathColorProperty =
            DependencyProperty.Register("GeoPathColor", typeof(Brush), typeof(TextOnAPath), new PropertyMetadata(default(Brush), onGeoColorChangeCallback));

        private static void onGeoColorChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public Brush GeoPathColor
        {
            get { return (Brush)GetValue(GeoPathColorProperty); }
            set { SetValue(GeoPathColorProperty, value); }
        }

#if SILVERLIGHT

        // since silverlight has no OverrideMetadata like WPF, we recreate internal DP and 
        // setup bindings in LoadEvent to get notification of changes.

        private static readonly DependencyProperty internalFontSizeProperty =
            DependencyProperty.Register("internalFontSize", typeof(double), typeof(TextOnAPath),
                                        new PropertyMetadata(11.0, OnFontPropertyChanged));


        private static readonly DependencyProperty internalFontFamilyProperty =
            DependencyProperty.Register("internalFontFamily", typeof(FontFamily), typeof(TextOnAPath),
                                        new PropertyMetadata(new FontFamily("Portable User Interface"),
                                                             OnFontPropertyChanged));

        private static readonly DependencyProperty internalFontStretchProperty =
            DependencyProperty.Register("internalFontStretch", typeof(FontStretch), typeof(TextOnAPath),
                                        new PropertyMetadata(FontStretches.Normal,
                                                             OnFontPropertyChanged));

        private static readonly DependencyProperty internalFontStyleProperty =
            DependencyProperty.Register("internalFontStyle", typeof(FontStyle), typeof(TextOnAPath),
                                        new PropertyMetadata(FontStyles.Normal,
                                                             OnFontPropertyChanged));

        private static readonly DependencyProperty internalFontWeightProperty =
            DependencyProperty.Register("internalFontWeight", typeof(FontWeight), typeof(TextOnAPath),
                                        new PropertyMetadata(FontWeights.Normal,
                                                             OnFontPropertyChanged));
#endif

        private static void OnFontPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnAPath = d as TextOnAPath;

            if (textOnAPath == null)
                return;

            if (e.NewValue == null || e.NewValue == e.OldValue)
                return;

            textOnAPath.UpdateText();
            textOnAPath.Update();
        }

        private double[] _segmentLengths;
        private TextBlock[] _textBlocks;

        private Panel _layoutPanel;
        private bool _layoutHasValidSize;

        #region Text DP

        /// <summary>
        ///     This DP holds the string that gets displayed following the geometry path
        /// </summary>
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(TextOnAPath),
#if SILVERLIGHT
 new PropertyMetadata(null, OnStringPropertyChanged));
#else
            new PropertyMetadata(null, new PropertyChangedCallback(OnStringPropertyChanged),
                new CoerceValueCallback(CoerceTextValue)));
#endif

        private static void OnStringPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnAPath = d as TextOnAPath;

            if (textOnAPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                if (textOnAPath._layoutPanel != null)
                    textOnAPath._layoutPanel.Children.Clear();
                return;
            }

#if SILVERLIGHT
            // no coerce in silverlight, so this fakes it...
            if ((String)e.NewValue == "")
            {
                textOnAPath.TextPath = null;
                return;
            }
#endif

            textOnAPath.UpdateText();
            textOnAPath.Update();
        }

#if !SILVERLIGHT
        static object CoerceTextValue(DependencyObject d, object baseValue)
        {
            if ((String)baseValue == "")
                return null;

            return baseValue;
        }
#endif

        #endregion

        #region TextPath DP

        /// <summary>
        ///     This DP defines the Geometry that Text will follow
        /// </summary>
        public Geometry TextPath
        {
            get { return (Geometry)GetValue(TextPathProperty); }
            set { SetValue(TextPathProperty, value); }
        }

        public static readonly DependencyProperty TextPathProperty =
            DependencyProperty.Register("TextPath", typeof(Geometry), typeof(TextOnAPath),
                                        new PropertyMetadata(null, OnTextPathPropertyChanged));

        private static void OnTextPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnAPath = d as TextOnAPath;

            if (textOnAPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            textOnAPath.TextPath.Transform = null;

            textOnAPath.UpdateSize();
            textOnAPath.Update();
        }

        #endregion

        #region DrawLinePath DP

        /// <summary>
        ///     Set this property to True to display the line segments under the text (flattened path)
        /// </summary>
        public bool DrawLinePath
        {
            get { return (bool)GetValue(DrawLinePathProperty); }
            set { SetValue(DrawLinePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawFlattendPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawLinePathProperty =
            DependencyProperty.Register("DrawLinePath", typeof(bool), typeof(TextOnAPath),
                                        new PropertyMetadata(false, OnDrawLinePathPropertyChanged));

        private static void OnDrawLinePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnAPath = d as TextOnAPath;

            if (textOnAPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            textOnAPath.Update();
        }

        #endregion

        #region ScaleTextPath DP

        /// <summary>
        ///     If set to True (default) then the geometry defined by TextPath automatically gets scaled to fit the width/height of the control
        /// </summary>
        public bool ScaleTextPath
        {
            get { return (bool)GetValue(ScaleTextPathProperty); }
            set { SetValue(ScaleTextPathProperty, value); }
        }

        public static readonly DependencyProperty ScaleTextPathProperty =
            DependencyProperty.Register("ScaleTextPath", typeof(bool), typeof(TextOnAPath),
                                        new PropertyMetadata(false, OnScaleTextPathPropertyChanged));

        private static void OnScaleTextPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textOnAPath = d as TextOnAPath;

            if (textOnAPath == null)
                return;

            if (e.NewValue == e.OldValue)
                return;

            var value = (Boolean)e.NewValue;

            if (value == false && textOnAPath.TextPath != null)
                textOnAPath.TextPath.Transform = null;

            textOnAPath.UpdateSize();
            textOnAPath.Update();
        }

        #endregion

        private void UpdateText()
        {
            if (Text == null || FontFamily == null || FontWeight == null || FontStyle == null)
                return;

            _textBlocks = new TextBlock[Text.Length];
            _segmentLengths = new double[Text.Length];

            for (int i = 0; i < Text.Length; i++)
            {
                var t = new TextBlock();
                t.FontSize = FontSize;
                t.FontFamily = FontFamily;
                t.FontStretch = FontStretch;
                t.FontWeight = FontWeight;
                t.FontStyle = FontStyle;
                t.Text = new String(Text[i], 1);

#if SILVERLIGHT
                // seems Silverlight has valid ActualWidth at this point...no measure needed like WPF...hmmm???
                _segmentLengths[i] = t.ActualWidth;
#else
    // in WPF we must call measure to calculate width desired by TextBlock
                t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                _segmentLengths[i] = t.DesiredSize.Width;
#endif

                _textBlocks[i] = t;
            }
        }

        private void Update()
        {
            if (Text == null || TextPath == null || _layoutPanel == null || !_layoutHasValidSize)
                return;

#if SILVERLIGHT
            var pathGeoHelper = new PathGeometryHelper();
            List<Point> intersectionPoints = GeometryHelper.GetIntersectionPoints(pathGeoHelper.FlattenGeometry(TextPath, 0.25),
                                                                           _segmentLengths);
#endif

            _layoutPanel.Children.Clear();

            double maxHeight = 0.0;

            for (int i = 0; i < intersectionPoints.Count - 1; i++)
            {
                double oppositeLen =
                    Math.Sqrt(
                        Math.Pow(intersectionPoints[i].X + _segmentLengths[i] - intersectionPoints[i + 1].X, 2.0) +
                        Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0)) / 2.0;
                double hypLen =
                    Math.Sqrt(Math.Pow(intersectionPoints[i].X - intersectionPoints[i + 1].X, 2.0) +
                              Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0));

                double ratio = oppositeLen / hypLen;

                if (ratio > 1.0)
                    ratio = 1.0;
                else if (ratio < -1.0)
                    ratio = -1.0;

                double angle = 2.0 * Math.Asin(ratio) * 180.0 / Math.PI;

                // adjust sign on angle
                if ((intersectionPoints[i].X + _segmentLengths[i]) > intersectionPoints[i].X)
                {
                    if (intersectionPoints[i + 1].Y < intersectionPoints[i].Y)
                        angle = -angle;
                }
                else
                {
                    if (intersectionPoints[i + 1].Y > intersectionPoints[i].Y)
                        angle = -angle;
                }

                TextBlock currTextBlock = _textBlocks[i];

#if SILVERLIGHT
                double nextHeight = currTextBlock.ActualHeight;
#else
                double nextHeight = currTextBlock.DesiredSize.Height;
#endif
                if (nextHeight > maxHeight)
                {
                    maxHeight = nextHeight;
                    _layoutPanel.Margin = new Thickness(maxHeight);
                }

#if SILVERLIGHT
                var rotate = new RotateTransform { Angle = angle, CenterX = 0, CenterY = currTextBlock.ActualHeight };
                var translate = new TranslateTransform
                    {
                        X = intersectionPoints[i].X,
                        Y = intersectionPoints[i].Y - currTextBlock.ActualHeight
                    };
#else
                RotateTransform rotate = new RotateTransform { Angle = angle, CenterX = 0, CenterY = currTextBlock.DesiredSize.Height };
                TranslateTransform translate = new TranslateTransform { X = intersectionPoints[i].X, Y = intersectionPoints[i].Y - currTextBlock.DesiredSize.Height };
#endif

                var transformGrp = new TransformGroup();

                transformGrp.Children.Add(rotate);
                transformGrp.Children.Add(translate);

                currTextBlock.RenderTransform = transformGrp;

                _layoutPanel.Children.Add(currTextBlock);

                if (DrawLinePath)
                {
                    var line = new Line();
                    line.X1 = intersectionPoints[i].X;
                    line.Y1 = intersectionPoints[i].Y;
                    line.X2 = intersectionPoints[i + 1].X;
                    line.Y2 = intersectionPoints[i + 1].Y;
                    line.Stroke = GeoPathColor;
                    _layoutPanel.Children.Add(line);
                }
            }

#if !SILVERLIGHT
    // silverlight seems to have some bug/problems using Path 
    // class when the Transform property has value
            if (DrawPath == true && DrawLinePath == false)
            {
                Path path = new Path();
                path.Data = TextPath;
                path.Stroke = new SolidColorBrush(Colors.Black);
                path.StrokeThickness = 1;
                _layoutPanel.Children.Add(path);
            }
#endif
        }

        public TextOnAPath()
        {
            DefaultStyleKey = typeof(TextOnAPath);

#if SILVERLIGHT
            Loaded += TextOnAPath_Loaded;
#endif
        }

#if SILVERLIGHT
        private void TextOnAPath_Loaded(object sender, RoutedEventArgs e)
        {
            // set up bindings for Silverlight to get notification when these DPs change

            var fontSizeBinding = new Binding();
            fontSizeBinding.Source = FontSize;
            SetBinding(internalFontSizeProperty, fontSizeBinding);

            var fontFamilyBinding = new Binding();
            fontFamilyBinding.Source = FontFamily;
            SetBinding(internalFontFamilyProperty, fontFamilyBinding);

            var fontStretchBinding = new Binding();
            fontStretchBinding.Source = FontStretch;
            SetBinding(internalFontStretchProperty, fontStretchBinding);

            var fontStyleBinding = new Binding();
            fontStyleBinding.Source = FontStyle;
            SetBinding(internalFontStyleProperty, fontStyleBinding);

            var fontWeightBinding = new Binding();
            fontWeightBinding.Source = FontWeight;
            SetBinding(internalFontWeightProperty, fontWeightBinding);
        }
#endif

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _layoutPanel = GetTemplateChild("LayoutPanel") as Panel;
            if (_layoutPanel == null)
                throw new Exception("Could not find template part: LayoutPanel");

            _layoutPanel.SizeChanged += _layoutPanel_SizeChanged;
        }

        private Size _newSize;

        private void _layoutPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _newSize = e.NewSize;

            UpdateSize();
            Update();
        }

        private void UpdateSize()
        {
            if (_newSize == null || TextPath == null)
                return;

            _layoutHasValidSize = true;

            double xScale = _newSize.Width / TextPath.Bounds.Width;
            double yScale = _newSize.Height / TextPath.Bounds.Height;

            if (TextPath.Bounds.Width <= 0)
                xScale = 1.0;

            if (TextPath.Bounds.Height <= 0)
                xScale = 1.0;

            if (xScale <= 0 || yScale <= 0)
                return;

            if (TextPath.Transform is TransformGroup)
            {
                var grp = TextPath.Transform as TransformGroup;
                if (grp.Children[0] is ScaleTransform && grp.Children[1] is TranslateTransform)
                {
                    if (ScaleTextPath)
                    {
#if SILVERLIGHT
                        // unlike WPF, Silverlight seems to require reapplying Transform property
                        // to see changes.
                        // Also, in Silverlight the Transform property on a Geometry does not
                        // affect the size of the bounding box but WPF it does!!!
                        var scaleAgain = new ScaleTransform { ScaleX = xScale, ScaleY = yScale };
                        var translateAgain = new TranslateTransform
                            {
                                X = -TextPath.Bounds.X * xScale,
                                Y = -TextPath.Bounds.Y * yScale
                            };
                        var grpAgain = new TransformGroup();
                        grpAgain.Children.Add(scaleAgain);
                        grpAgain.Children.Add(translateAgain);
                        TextPath.Transform = grpAgain;
#else
                        ScaleTransform scale = grp.Children[0] as ScaleTransform;
                        scale.ScaleX *= xScale;
                        scale.ScaleY *= yScale;
#endif
                    }

#if !SILVERLIGHT
                    TranslateTransform translate = grp.Children[1] as TranslateTransform;
                    translate.X += -TextPath.Bounds.X;
                    translate.Y += -TextPath.Bounds.Y;
#endif
                }
            }
            else
            {
                ScaleTransform scale;
                TranslateTransform translate;

                if (ScaleTextPath)
                {
                    scale = new ScaleTransform { ScaleX = xScale, ScaleY = yScale };
                    translate = new TranslateTransform { X = -TextPath.Bounds.X * xScale, Y = -TextPath.Bounds.Y * yScale };
                }
                else
                {
                    scale = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
                    translate = new TranslateTransform { X = -TextPath.Bounds.X, Y = -TextPath.Bounds.Y };
                }

                var grp = new TransformGroup();
                grp.Children.Add(scale);
                grp.Children.Add(translate);
                TextPath.Transform = grp;
            }
        }
    }
}