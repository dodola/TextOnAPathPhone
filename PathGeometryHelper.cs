using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MatrixMathHelpers;
#if SILVERLIGHT

#endif

// PathGeometryHelper.cs by Charles Petzold, December 2007

namespace Petzold.Shapes
{
    public class PathGeometryHelper
    {
        // Cache stacks
        private readonly Stack<PathGeometry> pathStack = new Stack<PathGeometry>();
        private readonly Stack<PathFigure> figStack = new Stack<PathFigure>();
        private readonly Stack<PolyLineSegment> segStack = new Stack<PolyLineSegment>();

        // Re-usable collection
        private readonly List<Point> points = new List<Point>();

#if !SILVERLIGHT
        public void CacheAll(PathGeometry pathGeo)
        {
            if (pathGeo == null)
                return;

            if (pathGeo.IsFrozen || pathGeo.Figures.IsFrozen)
                return;

            foreach (PathFigure fig in pathGeo.Figures)
            {
                if (fig.IsFrozen || fig.Segments.IsFrozen)
                    continue;

                foreach (PathSegment seg in fig.Segments)
                {
                    PolyLineSegment polylineSeg = seg as PolyLineSegment;

                    if (polylineSeg != null)
                    {
                        if (polylineSeg.IsFrozen || polylineSeg.Points.IsFrozen)
                            continue;

                        polylineSeg.IsSmoothJoin = false;
                        polylineSeg.IsStroked = true;
                        polylineSeg.Points.Clear();
                        segStack.Push(polylineSeg);
                    }
                }
                fig.IsClosed = false;
                fig.IsFilled = true;
                fig.Segments.Clear();
                figStack.Push(fig);
            }
            pathGeo.Figures.Clear();
            pathGeo.Transform = Transform.Identity;
            pathGeo.FillRule = FillRule.EvenOdd;
            pathStack.Push(pathGeo);
        }
#endif

        public PathGeometry GetPathGeometry()
        {
            if (pathStack.Count > 0)
                return pathStack.Pop();

            return new PathGeometry();
        }

        public PathFigure GetPathFigure()
        {
            if (figStack.Count > 0)
                return figStack.Pop();

            return new PathFigure();
        }

        public PolyLineSegment GetPolyLineSegment()
        {
            if (segStack.Count > 0)
                return segStack.Pop();

            return new PolyLineSegment();
        }

        public PathGeometry FlattenGeometry(Geometry geoSrc, double tolerance)
        {
            // Return empty PathGeometry if geo is null
            if (geoSrc == null)
                return GetPathGeometry();

#if !SILVERLIGHT
    // Let system flatten it if any part of the Geometry 
    //      is a StreamGeometry of CombinedGeometry
            if (HasEmbeddedStreamGeometryOrCombinedGeometry(geoSrc))
                return geoSrc.GetFlattenedPathGeometry(tolerance, ToleranceType.Absolute);
#endif
            PathGeometry pathGeoDst = GetPathGeometry();
            FlattenGeometry(pathGeoDst, geoSrc, tolerance, Matrix.Identity);

            return pathGeoDst;
        }

#if !SILVERLIGHT
        bool HasEmbeddedStreamGeometryOrCombinedGeometry(Geometry geoSrc)
        {
            return (geoSrc is StreamGeometry || geoSrc is CombinedGeometry);

            if (geoSrc is GeometryGroup)
            {
                foreach (Geometry geoChild in (geoSrc as GeometryGroup).Children)
                    return HasEmbeddedStreamGeometryOrCombinedGeometry(geoSrc);
            }

            return false;
        }
#endif

        // By the time we get here, we know that no part of the Geometry is
        //  a StreamGeometry or a CombinedGeometry
        private void FlattenGeometry(PathGeometry pathGeoDst, Geometry geoSrc, double tolerance, Matrix matxPrevious)
        {
#if SILVERLIGHT
            MatrixHelper matxHelper = new MatrixHelper(TransformHelper.GetTransformValue(geoSrc.Transform)) *
                                      new MatrixHelper(matxPrevious);
            Matrix matx = matxHelper.Value;
#else
            Matrix matx = geoSrc.Transform.Value * matxPrevious;
#endif
            if (geoSrc is GeometryGroup)
            {
                foreach (Geometry geoChild in (geoSrc as GeometryGroup).Children)
                {
                    FlattenGeometry(pathGeoDst, geoChild, tolerance, matx);
                }
            }
            else if (geoSrc is LineGeometry)
            {
                var lineGeoSrc = geoSrc as LineGeometry;

                PathFigure figDst = GetPathFigure();

                PolyLineSegment segDst = GetPolyLineSegment();

                figDst.StartPoint = matx.Transform(lineGeoSrc.StartPoint);
                segDst.Points.Add(matx.Transform(lineGeoSrc.EndPoint));

                figDst.Segments.Add(segDst);
                pathGeoDst.Figures.Add(figDst);
            }
            else if (geoSrc is RectangleGeometry)
            {
                var rectGeoSrc = geoSrc as RectangleGeometry;
                PathFigure figDst = GetPathFigure();
                PolyLineSegment segDst = GetPolyLineSegment();

                if (rectGeoSrc.RadiusX == 0 || rectGeoSrc.RadiusY == 0)
                {
#if SILVERLIGHT
                    figDst.StartPoint = matx.Transform(new RectHelper(rectGeoSrc.Rect).TopLeft);
                    segDst.Points.Add(matx.Transform(new RectHelper(rectGeoSrc.Rect).TopRight));
                    segDst.Points.Add(matx.Transform(new RectHelper(rectGeoSrc.Rect).BottomRight));
                    segDst.Points.Add(matx.Transform(new RectHelper(rectGeoSrc.Rect).BottomLeft));
                    segDst.Points.Add(matx.Transform(new RectHelper(rectGeoSrc.Rect).TopLeft));
#else
                    figDst.StartPoint = matx.Transform(rectGeoSrc.Rect.TopLeft);
                    segDst.Points.Add(matx.Transform(rectGeoSrc.Rect.TopRight));
                    segDst.Points.Add(matx.Transform(rectGeoSrc.Rect.BottomRight));
                    segDst.Points.Add(matx.Transform(rectGeoSrc.Rect.BottomLeft));
                    // Note: added line below to Petzold code, added for compatability with WPF's method
                    segDst.Points.Add(matx.Transform(rectGeoSrc.Rect.TopLeft));
#endif
                }
                else
                {
                    double radiusX = Math.Min(rectGeoSrc.Rect.Width / 2, rectGeoSrc.RadiusX);
                    double radiusY = Math.Min(rectGeoSrc.Rect.Height / 2, rectGeoSrc.RadiusY);
                    Rect rect = rectGeoSrc.Rect;

                    figDst.StartPoint = matx.Transform(new Point(rect.Left + radiusX, rect.Top));

                    AddCorner(segDst, matx, new Point(rect.Right - radiusX, rect.Top),
                              new Point(rect.Right, rect.Top + radiusY), radiusX, radiusY, tolerance);
                    AddCorner(segDst, matx, new Point(rect.Right, rect.Bottom - radiusY),
                              new Point(rect.Right - radiusX, rect.Bottom), radiusX, radiusY, tolerance);
                    AddCorner(segDst, matx, new Point(rect.Left + radiusX, rect.Bottom),
                              new Point(rect.Left, rect.Bottom - radiusY), radiusX, radiusY, tolerance);
                    AddCorner(segDst, matx, new Point(rect.Left, rect.Top + radiusY),
                              new Point(rect.Left + radiusX, rect.Top), radiusX, radiusY, tolerance);
                }

                figDst.IsClosed = true;
                figDst.Segments.Add(segDst);
                pathGeoDst.Figures.Add(figDst);
            }
            else if (geoSrc is EllipseGeometry)
            {
                var elipGeoSrc = geoSrc as EllipseGeometry;
                PathFigure figDst = GetPathFigure();
                PolyLineSegment segDst = GetPolyLineSegment();

                var max = (int)(4 * (elipGeoSrc.RadiusX + elipGeoSrc.RadiusY) / tolerance);

                for (int i = 0; i < max; i++)
                {
                    double x = elipGeoSrc.Center.X + elipGeoSrc.RadiusX * Math.Sin(i * 2 * Math.PI / max);
                    double y = elipGeoSrc.Center.Y - elipGeoSrc.RadiusY * Math.Cos(i * 2 * Math.PI / max);
                    Point pt = matx.Transform(new Point(x, y));

                    if (i == 0)
                        figDst.StartPoint = pt;
                    else
                        segDst.Points.Add(pt);
                }

                figDst.IsClosed = true;
                figDst.Segments.Add(segDst);
                pathGeoDst.Figures.Add(figDst);
            }

            else if (geoSrc is PathGeometry)
            {
                var pathGeoSrc = geoSrc as PathGeometry;
                pathGeoDst.FillRule = pathGeoSrc.FillRule;

                foreach (PathFigure figSrc in pathGeoSrc.Figures)
                {
                    PathFigure figDst = GetPathFigure();
                    figDst.IsFilled = figSrc.IsFilled;
                    figDst.IsClosed = figSrc.IsClosed;
                    figDst.StartPoint = matx.Transform(figSrc.StartPoint);
                    Point ptLast = figDst.StartPoint;

                    foreach (PathSegment segSrc in figSrc.Segments)
                    {
                        PolyLineSegment segDst = GetPolyLineSegment();
#if !SILVERLIGHT
                        segDst.IsStroked = segSrc.IsStroked;
                        segDst.IsSmoothJoin = segSrc.IsSmoothJoin;
#endif
                        if (segSrc is LineSegment)
                        {
                            var lineSegSrc = segSrc as LineSegment;
                            ptLast = matx.Transform(lineSegSrc.Point);
                            segDst.Points.Add(ptLast);
                        }

                        else if (segSrc is PolyLineSegment)
                        {
                            var polySegSrc = segSrc as PolyLineSegment;

                            foreach (Point pt in polySegSrc.Points)
                            {
                                ptLast = matx.Transform(pt);
                                segDst.Points.Add(ptLast);
                            }
                        }

                        else if (segSrc is BezierSegment)
                        {
                            var bezSeg = segSrc as BezierSegment;
                            Point pt0 = ptLast;
                            Point pt1 = matx.Transform(bezSeg.Point1);
                            Point pt2 = matx.Transform(bezSeg.Point2);
                            Point pt3 = matx.Transform(bezSeg.Point3);

                            points.Clear();
                            FlattenCubicBezier(points, pt0, pt1, pt2, pt3, tolerance);

                            for (int i = 1; i < points.Count; i++)
                                segDst.Points.Add(points[i]);

                            ptLast = points[points.Count - 1];
                        }

                        else if (segSrc is PolyBezierSegment)
                        {
                            var polyBezSeg = segSrc as PolyBezierSegment;

                            for (int bez = 0; bez < polyBezSeg.Points.Count; bez += 3)
                            {
                                if (bez + 2 > polyBezSeg.Points.Count - 1)
                                    break;

                                Point pt0 = ptLast;
                                Point pt1 = matx.Transform(polyBezSeg.Points[bez]);
                                Point pt2 = matx.Transform(polyBezSeg.Points[bez + 1]);
                                Point pt3 = matx.Transform(polyBezSeg.Points[bez + 2]);

                                points.Clear();
                                FlattenCubicBezier(points, pt0, pt1, pt2, pt3, tolerance);

                                for (int i = 1; i < points.Count; i++)
                                    segDst.Points.Add(points[i]);

                                ptLast = points[points.Count - 1];
                            }
                        }

                        else if (segSrc is QuadraticBezierSegment)
                        {
                            var quadBezSeg = segSrc as QuadraticBezierSegment;
                            Point pt0 = ptLast;
                            Point pt1 = matx.Transform(quadBezSeg.Point1);
                            Point pt2 = matx.Transform(quadBezSeg.Point2);

                            points.Clear();
                            FlattenQuadraticBezier(points, pt0, pt1, pt2, tolerance);

                            for (int i = 1; i < points.Count; i++)
                                segDst.Points.Add(points[i]);

                            ptLast = points[points.Count - 1];
                        }

                        else if (segSrc is PolyQuadraticBezierSegment)
                        {
                            var polyQuadBezSeg = segSrc as PolyQuadraticBezierSegment;

                            for (int bez = 0; bez < polyQuadBezSeg.Points.Count; bez += 2)
                            {
                                if (bez + 1 > polyQuadBezSeg.Points.Count - 1)
                                    break;

                                Point pt0 = ptLast;
                                Point pt1 = matx.Transform(polyQuadBezSeg.Points[bez]);
                                Point pt2 = matx.Transform(polyQuadBezSeg.Points[bez + 1]);

                                points.Clear();
                                FlattenQuadraticBezier(points, pt0, pt1, pt2, tolerance);

                                for (int i = 1; i < points.Count; i++)
                                    segDst.Points.Add(points[i]);

                                ptLast = points[points.Count - 1];
                            }
                        }

                        else if (segSrc is ArcSegment)
                        {
                            var arcSeg = segSrc as ArcSegment;
                            points.Clear();

                            FlattenArc(points, ptLast, arcSeg.Point,
                                       arcSeg.Size.Width, arcSeg.Size.Height,
                                       arcSeg.RotationAngle,
                                       arcSeg.IsLargeArc,
                                       arcSeg.SweepDirection == SweepDirection.Counterclockwise,
                                       tolerance);

                            // Set ptLast while transferring points
                            for (int i = 1; i < points.Count; i++)
                                segDst.Points.Add(ptLast = points[i]);
                        }
                        figDst.Segments.Add(segDst);
                    }
                    pathGeoDst.Figures.Add(figDst);
                }
            }
        }


        private void FlattenCubicBezier(List<Point> points, Point ptStart, Point ptCtrl1, Point ptCtrl2, Point ptEnd,
                                        double tolerance)
        {
#if SILVERLIGHT
            var max = (int)(((new PointHelper(ptCtrl1) - new PointHelper(ptStart)).Length +
                              (new PointHelper(ptCtrl2) - new PointHelper(ptCtrl1)).Length +
                              (new PointHelper(ptEnd) - new PointHelper(ptCtrl2)).Length) / tolerance);
#else
            int max = (int)(((ptCtrl1 - ptStart).Length + 
                             (ptCtrl2 - ptCtrl1).Length + 
                             (ptEnd   - ptCtrl2).Length) / tolerance);
#endif

            for (int i = 0; i <= max; i++)
            {
                double t = (double)i / max;

                double x = (1 - t) * (1 - t) * (1 - t) * ptStart.X +
                           3 * t * (1 - t) * (1 - t) * ptCtrl1.X +
                           3 * t * t * (1 - t) * ptCtrl2.X +
                           t * t * t * ptEnd.X;

                double y = (1 - t) * (1 - t) * (1 - t) * ptStart.Y +
                           3 * t * (1 - t) * (1 - t) * ptCtrl1.Y +
                           3 * t * t * (1 - t) * ptCtrl2.Y +
                           t * t * t * ptEnd.Y;

                points.Add(new Point(x, y));
            }
        }

        private void FlattenQuadraticBezier(List<Point> points, Point ptStart, Point ptCtrl, Point ptEnd,
                                            double tolerance)
        {
#if SILVERLIGHT
            var max = (int)(((new PointHelper(ptCtrl) - new PointHelper(ptStart)).Length +
                              (new PointHelper(ptEnd) - new PointHelper(ptCtrl)).Length) / tolerance);
#else
            int max = (int)(((ptCtrl - ptStart).Length + 
                             (ptEnd - ptCtrl).Length) / tolerance);
#endif

            for (int i = 0; i <= max; i++)
            {
                double t = (double)i / max;

                double x = (1 - t) * (1 - t) * ptStart.X +
                           2 * t * (1 - t) * ptCtrl.X +
                           t * t * ptEnd.X;

                double y = (1 - t) * (1 - t) * ptStart.Y +
                           2 * t * (1 - t) * ptCtrl.Y +
                           t * t * ptEnd.Y;

                points.Add(new Point(x, y));
            }
        }

        private void FlattenArc(List<Point> points, Point pt1, Point pt2,
                                double radiusX, double radiusY, double angleRotation,
                                bool isLargeArc, bool isCounterclockwise,
                                double tolerance)
        {
            // Adjust for different radii and rotation angle
#if SILVERLIGHT
            var matx = new MatrixHelper();
            matx.Rotate(-angleRotation);
            matx.Scale(radiusY / radiusX, 1);
            pt1 = matx.Value.Transform(pt1);
            pt2 = matx.Value.Transform(pt2);
#else
            Matrix matx = new Matrix();
            matx.Rotate(-angleRotation);
            matx.Scale(radiusY / radiusX, 1);
            pt1 = matx.Transform(pt1);
            pt2 = matx.Transform(pt2);
#endif

            // Get info about chord that connects both points
            var midPoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
#if SILVERLIGHT
            Vector vect = new PointHelper(pt2) - new PointHelper(pt1);
#else
            Vector vect = pt2 - pt1;
#endif
            double halfChord = vect.Length / 2;

            // Get vector from chord to center
            Vector vectRotated;

            // (comparing two Booleans here!)
            vectRotated = isLargeArc == isCounterclockwise ? new Vector(-vect.Y, vect.X) : new Vector(vect.Y, -vect.X);

            vectRotated.Normalize();

            // Distance from chord to center 
            double centerDistance = Math.Sqrt(radiusY * radiusY - halfChord * halfChord);

            // Calculate two center points
            Point center = midPoint + centerDistance * vectRotated;

            // Get angles from center to the two points
            double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
            double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

            // (another comparison of two Booleans!)
            if (isLargeArc == (Math.Abs(angle2 - angle1) < Math.PI))
            {
                if (angle1 < angle2)
                    angle1 += 2 * Math.PI;
                else
                    angle2 += 2 * Math.PI;
            }

            // Invert matrix for final point calculation
            matx.Invert();

            // Calculate number of points for polyline approximation
            var max = (int)((4 * (radiusX + radiusY) * Math.Abs(angle2 - angle1) / (2 * Math.PI)) / tolerance);

            // Loop through the points
            for (int i = 0; i <= max; i++)
            {
                double angle = ((max - i) * angle1 + i * angle2) / max;
                double x = center.X + radiusY * Math.Cos(angle);
                double y = center.Y + radiusY * Math.Sin(angle);

                // Transform the point back
                Point pt = matx.Transform(new Point(x, y));
                points.Add(pt);
            }
        }

        // Helper routine for RectangleGeometry
        private void AddCorner(PolyLineSegment segDst, Matrix matx, Point pt1, Point pt2,
                               double radiusX, double radiusY, double tolerance)
        {
            points.Clear();
            FlattenArc(points, pt1, pt2, radiusX, radiusY, 0, false, false, tolerance);

            foreach (Point t in points)
                segDst.Points.Add(matx.Transform(t));
        }

        // Transfers Pathfigure to List<Point>
        public void DumpFigureToList(List<Point> points, PathFigure figSrc)
        {
            points.Clear();
            points.Add(figSrc.StartPoint);

            foreach (PathSegment seg in figSrc.Segments)
            {
                if (seg is LineSegment)
                {
                    points.Add((seg as LineSegment).Point);
                }
                else if (seg is PolyLineSegment)
                {
                    foreach (Point t in (seg as PolyLineSegment).Points)
                        if (!PointsEqual(t, points[points.Count - 1]))
                            points.Add(t);
                }
            }

            if (figSrc.IsClosed && PointsEqual(points[points.Count - 1], points[0]))
                points.RemoveAt(points.Count - 1);
        }

        // Hack because point comparison in previous method wasn't working due
        //  to a difference of 1E-14.
        private bool PointsEqual(Point pt1, Point pt2)
        {
            return (Math.Abs(pt1.X - pt2.X) < 1E-10 &&
                    Math.Abs(pt1.Y - pt2.Y) < 1E-10);
        }
    }
}