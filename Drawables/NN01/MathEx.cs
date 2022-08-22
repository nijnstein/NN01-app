using Microsoft.Maui.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NN01
{
    public static class MathEx
    {
        /// <summary>
        /// Check if 2 line segments are intersecting in 2d space
        /// p1 and p2 belong to line 1, p3 and p4 belong to line 2
        /// </summary>
        public static bool LinesIntersect(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

            // make sure the denominator is != 0, if 0 the lines are parallel
            if (denominator != 0)
            {
                // most common case = not parallel 
                float u_a = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
                float u_b = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// intersect lines through points (not linesegments) 
        /// 
        /// line 1 through = p1, p2
        /// line 2 through = p3, p4 
        /// 
        /// line = (x1, y1) and (x2, y2)
        ///         A = y2-y1
        ///         B = x1 - x2
        ///         C = Ax1+By1
        /// 
        ///  A1x + B1y = C1
        ///  A2x + B2y = C2
        ///         
        ///  so d = A1 * B2 - A2 * B1
        ///  and (x,y) = ((B2 * C1 - B1 * C2) / d, (A1 * C2 - A2 * C1) / d)
        /// 
        /// </summary>
        public static bool TryGetIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            float a1 = p2.Y - p1.Y;
            float a2 = p4.Y - p3.Y;

            float b1 = p2.X - p1.X;
            float b2 = p4.X - p3.X;

            float d = a1 * b2 - a2 * b1;
            if (d == 0)
            {
                intersection = PointF.Zero;
                return false;
            }

            float c1 = a1 * p1.X + b1 * p1.Y;
            float c2 = a2 * p3.X + b2 * p3.Y;

            intersection = new PointF((b2 * c1 - b1 * c2) / d, (a1 * c2 - a2 * c1) / d);
            return true;
        }

        /// <summary>
        /// get intersection of 2 line sections 
        /// 
        /// p1-p2 / p3-p4
        /// a-b / c-d 
        /// 
        ///  E = B-A = ( Bx-Ax, By-Ay )
        ///  F = D-C = (Dx-Cx, Dy-Cy ) 
        ///  P = ( -Ey, Ex )
        ///  h = ((A-C) * P ) / (F* P )
        ///  
        ///  point of intersection is C + F*h
        /// </summary>
        /// <returns></returns>

        public static bool TryGetSegmentIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection, bool includeEndPoints = false)
        {
            float ex = p2.X - p1.X;
            float ey = p2.Y - p1.Y;

            float fx = p4.X - p3.X;
            float fy = p4.Y - p3.Y;

            if (ex == 0 & ey == 0 | fx == 0 & fy == 0)
            {
                intersection = PointF.Zero;
                return false;
            }


            float px = -ey;
            float py = ex;

            float ax = p1.X - p3.X;
            float ay = p1.Y - p3.Y;

            // h == how much to multiply the lenght of the line to touch the other line
            float h = (ax * px + ay * py) / (fx * px + fy * py);


            // if h < 0 then line p3-p4 is behind p1-p2
            // if h > 1 then its in front
            // if h == 0 or h == 1 then it intersects an endpoint
            if (includeEndPoints)
            {
                if (h < 0 | h > 1)
                {
                    intersection = PointF.Zero;
                    return false;
                }
            }
            else
            {
                // if h is exactly 1 or 0 its on the endpoint of a line segment 
                if (h <= 0 | h >= 1)
                {
                    intersection = PointF.Zero;
                    return false;
                }
            }

            float cx = p3.X - p1.X;
            float cy = p3.Y - p1.Y;

            // same as h but from p3.p4 - p1.p2
            px = -fy;
            py = fx;

            float g = (cx * px + cy * py) / (ex * px + ey * py);
            if (includeEndPoints)
            {
                if (g < 0 | g > 1)
                {
                    intersection = PointF.Zero;
                    return false;
                }
            }
            else
            {
                if (g <= 0 | g >= 1)
                {
                    intersection = PointF.Zero;
                    return false;
                }
            }

            // intersection at C + F*h
            intersection = new PointF(p3.X + fx * h, p3.Y + fy * h);
            return true;
        }

        /*  public static PointF ClosestPointOnLine(PointF p1, PointF p2, PointF test, bool restrictToLineSegment)
          {
              PointF ab = new PointF(p2.X - p1.X, p2.Y - p1.Y);
              PointF ap = new PointF(test.X - p1.X, test.Y - p1.Y);

              float lengthSqrAB = ab.X * ab.X + ab.Y * ab.Y;
              float t = (ap.X * ab.X + ap.Y * ab.Y) / lengthSqrAB;

              // restrict point to segment by clamping t (the interpolation from p1 to p2
              if (restrictToLineSegment)
              {
                  t = Math.Max(0, Math.Min(1, t)); 
              }

              // get point on segment 
              return new PointF(ap.X + t * ab.X, ap.Y + t * ab.Y);
          }*/

        public static PointF ClosestPointOnLine(PointF p1, PointF p2, PointF position, bool restrictToLineSegment = true)
        {
            PointF ba = new PointF(p2.X - p1.X, p2.Y - p1.Y);
            PointF ap = new PointF(position.X - p1.X, position.Y - p1.Y);

            float t = Dot(ap, ba) / Dot(ba, ba);

            if (restrictToLineSegment)
            {
                t = t.Clamp01();
            }
            return p1.Lerp(p2, (double)t);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(this PointF p)
        {
            return MathF.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF Normalize(this PointF p)
        {
            float im = 1 / p.Magnitude();
            return new PointF(p.X * im, p.Y * im);
        }

        /// <summary>
        /// get normal vector to line (not normalized)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF Normal(PointF p1, PointF p2)
        {
            return new PointF(-(p2.Y - p1.Y), p2.X - p1.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(PointF p1, PointF p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF Negate(this PointF p)
        {
            return new PointF(-p.X, -p.Y);
        }

    }
}
