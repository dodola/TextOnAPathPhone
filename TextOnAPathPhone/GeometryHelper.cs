using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TextOnAPath
{
    public static class GeometryHelper
    {
        public static List<Point> GetIntersectionPoints(PathGeometry FlattenedPath, double[] SegmentLengths)
        {
            var intersectionPoints = new List<Point>();

            List<Point> pointsOnFlattenedPath = GetPointsOnFlattenedPath(FlattenedPath);

            if (pointsOnFlattenedPath == null || pointsOnFlattenedPath.Count < 2)
                return intersectionPoints;

            Point currPoint = pointsOnFlattenedPath[0];
            intersectionPoints.Add(currPoint);

            // find point on flattened path that is segment length away from current point

            int flattedPathIndex = 0;

            int segmentIndex = 1;

            while (flattedPathIndex < pointsOnFlattenedPath.Count - 1 &&
                   segmentIndex < SegmentLengths.Length + 1)
            {
                Point? intersectionPoint = GetIntersectionOfSegmentAndCircle(
                    pointsOnFlattenedPath[flattedPathIndex],
                    pointsOnFlattenedPath[flattedPathIndex + 1], currPoint, SegmentLengths[segmentIndex - 1]);

                if (intersectionPoint == null)
                    flattedPathIndex++;
                else
                {
                    intersectionPoints.Add((Point) intersectionPoint);
                    currPoint = (Point) intersectionPoint;
                    pointsOnFlattenedPath[flattedPathIndex] = currPoint;
                    segmentIndex++;
                }
            }

            return intersectionPoints;
        }

        private static List<Point> GetPointsOnFlattenedPath(PathGeometry FlattenedPath)
        {
            var flattenedPathPoints = new List<Point>();

            // for flattened geometry there should be just one PathFigure in the Figures
            if (FlattenedPath.Figures.Count != 1)
                return null;

            PathFigure pathFigure = FlattenedPath.Figures[0];

            flattenedPathPoints.Add(pathFigure.StartPoint);

            // SegmentsCollection should contain PolyLineSegment and LineSegment
            foreach (PathSegment pathSegment in pathFigure.Segments)
            {
                if (pathSegment is PolyLineSegment)
                {
                    var seg = pathSegment as PolyLineSegment;

                    flattenedPathPoints.AddRange(seg.Points);
                }
                else if (pathSegment is LineSegment)
                {
                    var seg = pathSegment as LineSegment;

                    flattenedPathPoints.Add(seg.Point);
                }
                else
                    throw new Exception("GetIntersectionPoint - unexpected path segment type: " + pathSegment);
            }

            return (flattenedPathPoints);
        }

        private static Point? GetIntersectionOfSegmentAndCircle(Point SegmentPoint1, Point SegmentPoint2,
                                                                Point CircleCenter, double CircleRadius)
        {
            // linear equation for segment: y = mx + b
            double slope = (SegmentPoint2.Y - SegmentPoint1.Y)/(SegmentPoint2.X - SegmentPoint1.X);
            double intercept = SegmentPoint1.Y - (slope*SegmentPoint1.X);

            // special case when segment is vertically oriented
            if (double.IsInfinity(slope))
            {
                double root = Math.Pow(CircleRadius, 2.0) - Math.Pow(SegmentPoint1.X - CircleCenter.X, 2.0);

                if (root < 0)
                    return null;

                // soln 1
                double SolnX1 = SegmentPoint1.X;
                double SolnY1 = CircleCenter.Y - Math.Sqrt(root);
                var Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX1, SegmentPoint1.X, SegmentPoint2.X) &&
                    IsBetween(SolnY1, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = SegmentPoint1.X;
                double SolnY2 = CircleCenter.Y + Math.Sqrt(root);
                var Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX2, SegmentPoint1.X, SegmentPoint2.X) &&
                    IsBetween(SolnY2, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln2);
                }
            }
            else
            {
                // use soln to quadradratic equation to solve intersection of segment and circle:
                // x = (-b +/ sqrt(b^2-4ac))/(2a)
                double a = 1 + Math.Pow(slope, 2.0);
                double b = (-2*CircleCenter.X) + (2*(intercept - CircleCenter.Y)*slope);
                double c = Math.Pow(CircleCenter.X, 2.0) + Math.Pow(intercept - CircleCenter.Y, 2.0) -
                           Math.Pow(CircleRadius, 2.0);

                // check for no solutions, is sqrt negative?
                double root = Math.Pow(b, 2.0) - (4*a*c);

                if (root < 0)
                    return null;

                // we might have two solns...

                // soln 1
                double SolnX1 = (-b + Math.Sqrt(root))/(2*a);
                double SolnY1 = slope*SolnX1 + intercept;
                var Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX1, SegmentPoint1.X, SegmentPoint2.X) &&
                    IsBetween(SolnY1, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = (-b - Math.Sqrt(root))/(2*a);
                double SolnY2 = slope*SolnX2 + intercept;
                var Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (IsBetween(SolnX2, SegmentPoint1.X, SegmentPoint2.X) &&
                    IsBetween(SolnY2, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln2);
                }
            }

            // shouldn't get here...but in case
            return null;
        }

        private static bool IsBetween(double X, double X1, double X2)
        {
            if (X1 >= X2 && X <= X1 && X >= X2)
                return true;

            if (X1 <= X2 && X >= X1 && X <= X2)
                return true;

            return false;
        }
    }
}