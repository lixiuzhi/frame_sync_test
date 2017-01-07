using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pathfinding {
	/** Contains various spline functions.
	 * \ingroup utils
	 */
	static class AstarSplines {
		public static Vector3 CatmullRom (Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			float percentComplete = elapsedTime;
			float percentCompleteSquared = percentComplete * percentComplete;
			float percentCompleteCubed = percentCompleteSquared * percentComplete;

			return
				previous * (-0.5F*percentCompleteCubed +
							percentCompleteSquared -
							0.5F*percentComplete) +

				start *
				(1.5F*percentCompleteCubed +
				 -2.5F*percentCompleteSquared + 1.0F) +

				end *
				(-1.5F*percentCompleteCubed +
				 2.0F*percentCompleteSquared +
				 0.5F*percentComplete) +

				next *
				(0.5F*percentCompleteCubed -
				 0.5F*percentCompleteSquared);
		}

		[System.Obsolete("Use CatmullRom")]
		public static Vector3 CatmullRomOLD (Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime) {
			return CatmullRom(previous, start, end, next, elapsedTime);
		}

		/** Returns a point on a cubic bezier curve. \a t is clamped between 0 and 1 */
		public static Vector3 CubicBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return t2*t2*t2 * p0 + 3 * t2*t2 * t * p1 + 3 * t2 * t*t * p2 + t*t*t * p3;
		}

		/** Returns the derivative for a point on a cubic bezier curve. \a t is clamped between 0 and 1 */
		public static Vector3 CubicBezierDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return 3*t2*t2*(p1-p0) + 6*t2*t*(p2 - p1) + 3*t*t*(p3 - p2);
		}

		/** Returns the second derivative for a point on a cubic bezier curve. \a t is clamped between 0 and 1 */
		public static Vector3 CubicBezierSecondDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float t2 = 1-t;
			return 6*t2*(p2 - 2*p1 + p0) + 6*t*(p3 - 2*p2 + p1);
		}
	}

	/** Various vector math utility functions.
	 * \version A lot of functions in the Polygon class have been moved to this class
	 * the names have changed slightly and everything now consistently assumes a left handed
	 * coordinate system now instead of sometimes using a left handed one and sometimes
	 * using a right handed one. This is why the 'Left' methods in the Polygon class redirect
	 * to methods named 'Right'. The functionality is exactly the same.
	 *
	 * Note the difference between segments and lines. Lines are infinitely
	 * long but segments have only a finite length.
	 *
	 * \ingroup utils
	 */
	public static class VectorMath {
		/** Returns the closest point on the line.
		 * The line is treated as infinite.
		 * \see ClosestPointOnSegment
		 * \see ClosestPointOnLineFactor
		 */
		public static Vector3 ClosestPointOnLine (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
			float dot = Vector3.Dot(point - lineStart, lineDirection);

			return lineStart + (dot*lineDirection);
		}

		/** Factor along the line which is closest to the point.
		 * Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		 * The closest point can be calculated using (end-start)*factor + start.
		 *
		 * \see ClosestPointOnLine
		 * \see ClosestPointOnSegment
		 */
		public static float ClosestPointOnLineFactor (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			var dir = lineEnd - lineStart;
			float sqrMagn = dir.sqrMagnitude;

			if (sqrMagn <= 0.000001) return 0;

			return Vector3.Dot(point - lineStart, dir) / sqrMagn;
		}

		/** Factor along the line which is closest to the point.
		 * Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		 * The closest point can be calculated using (end-start)*factor + start
		 */
		public static float ClosestPointOnLineFactor (Int3 lineStart, Int3 lineEnd, Int3 point) {
			var lineDirection = lineEnd - lineStart;
			double magn = lineDirection.sqrMagnitude;

			double closestPoint = Int3.Dot((point - lineStart), lineDirection);

			if (magn != 0)  closestPoint /= magn;

			return (float)closestPoint;
		}

		/** Factor of the nearest point on the segment.
		 * Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		 * The closest point can be calculated using (end-start)*factor + start;
		 */
		public static float ClosestPointOnLineFactor (Int2 lineStart, Int2 lineEnd, Int2 point) {
			var lineDirection = lineEnd - lineStart;
			double magn = lineDirection.sqrMagnitudeLong;

			double closestPoint = Int2.DotLong(point - lineStart, lineDirection);

			if (magn != 0) closestPoint /= magn;

			return (float)closestPoint;
		}

		/** Returns the closest point on the segment.
		 * The segment is NOT treated as infinite.
		 * \see ClosestPointOnLine
		 * \see ClosestPointOnSegmentXZ
		 */
		public static Vector3 ClosestPointOnSegment (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			var dir = lineEnd - lineStart;
			float sqrMagn = dir.sqrMagnitude;

			if (sqrMagn <= 0.000001) return lineStart;

			float factor = Vector3.Dot(point - lineStart, dir) / sqrMagn;
			return lineStart + Mathf.Clamp01(factor)*dir;
		}

		/** Returns the closest point on the segment in the XZ plane.
		 * The y coordinate of the result will be the same as the y coordinate of the \a point parameter.
		 *
		 * The segment is NOT treated as infinite.
		 * \see ClosestPointOnSegment
		 * \see ClosestPointOnLine
		 */
		public static Vector3 ClosestPointOnSegmentXZ (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			lineStart.y = point.y;
			lineEnd.y = point.y;
			Vector3 fullDirection = lineEnd-lineStart;
			Vector3 fullDirection2 = fullDirection;
			fullDirection2.y = 0;
			float magn = fullDirection2.magnitude;
			Vector3 lineDirection = magn > float.Epsilon ? fullDirection2/magn : Vector3.zero;

			float closestPoint = Vector3.Dot((point-lineStart), lineDirection);
			return lineStart+(Mathf.Clamp(closestPoint, 0.0f, fullDirection2.magnitude)*lineDirection);
		}

		/** Returns the approximate shortest squared distance between x,z and the segment p-q.
		 * The segment is not considered infinite.
		 * This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		 * \todo Is this actually approximate? It looks exact.
		 */
		public static float SqrDistancePointSegmentApproximate (int x, int z, int px, int pz, int qx, int qz) {
			float pqx = (float)(qx - px);
			float pqz = (float)(qz - pz);
			float dx = (float)(x - px);
			float dz = (float)(z - pz);
			float d = pqx*pqx + pqz*pqz;
			float t = pqx*dx + pqz*dz;

			if (d > 0)
				t /= d;
			if (t < 0)
				t = 0;
			else if (t > 1)
				t = 1;

			dx = px + t*pqx - x;
			dz = pz + t*pqz - z;

			return dx*dx + dz*dz;
		}

		/** Returns the approximate shortest squared distance between x,z and the segment p-q.
		 * The segment is not considered infinite.
		 * This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		 * \todo Is this actually approximate? It looks exact.
		 */
		public static float SqrDistancePointSegmentApproximate (Int3 a, Int3 b, Int3 p) {
			float pqx = (float)(b.x - a.x);
			float pqz = (float)(b.z - a.z);
			float dx = (float)(p.x - a.x);
			float dz = (float)(p.z - a.z);
			float d = pqx*pqx + pqz*pqz;
			float t = pqx*dx + pqz*dz;

			if (d > 0)
				t /= d;
			if (t < 0)
				t = 0;
			else if (t > 1)
				t = 1;

			dx = a.x + t*pqx - p.x;
			dz = a.z + t*pqz - p.z;

			return dx*dx + dz*dz;
		}

		/** Returns the squared distance between p and the segment a-b.
		 * The line is not considered infinite.
		 */
		public static float SqrDistancePointSegment (Vector3 a, Vector3 b, Vector3 p) {
			var nearest = ClosestPointOnSegment(a, b, p);

			return (nearest-p).sqrMagnitude;
		}

		/** 3D minimum distance between 2 segments.
		 * Input: two 3D line segments S1 and S2
		 * \returns the shortest squared distance between S1 and S2
		 */
		public static float SqrDistanceSegmentSegment (Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2) {
			Vector3 u = e1 - s1;
			Vector3 v = e2 - s2;
			Vector3 w = s1 - s2;
			float a = Vector3.Dot(u, u);           // always >= 0
			float b = Vector3.Dot(u, v);
			float c = Vector3.Dot(v, v);           // always >= 0
			float d = Vector3.Dot(u, w);
			float e = Vector3.Dot(v, w);
			float D = a*c - b*b;           // always >= 0
			float sc, sN, sD = D;          // sc = sN / sD, default sD = D >= 0
			float tc, tN, tD = D;          // tc = tN / tD, default tD = D >= 0

			// compute the line parameters of the two closest points
			if (D < 0.000001f) { // the lines are almost parallel
				sN = 0.0f;         // force using point P0 on segment S1
				sD = 1.0f;         // to prevent possible division by 0.0 later
				tN = e;
				tD = c;
			} else {               // get the closest points on the infinite lines
				sN = (b*e - c*d);
				tN = (a*e - b*d);
				if (sN < 0.0f) {        // sc < 0 => the s=0 edge is visible
					sN = 0.0f;
					tN = e;
					tD = c;
				} else if (sN > sD) { // sc > 1  => the s=1 edge is visible
					sN = sD;
					tN = e + b;
					tD = c;
				}
			}

			if (tN < 0.0f) {            // tc < 0 => the t=0 edge is visible
				tN = 0.0f;
				// recompute sc for this edge
				if (-d < 0.0f)
					sN = 0.0f;
				else if (-d > a)
					sN = sD;
				else {
					sN = -d;
					sD = a;
				}
			} else if (tN > tD) {    // tc > 1  => the t=1 edge is visible
				tN = tD;
				// recompute sc for this edge
				if ((-d + b) < 0.0f)
					sN = 0;
				else if ((-d + b) > a)
					sN = sD;
				else {
					sN = (-d +  b);
					sD = a;
				}
			}
			// finally do the division to get sc and tc
			sc = (Math.Abs(sN) < 0.000001f ? 0.0f : sN / sD);
			tc = (Math.Abs(tN) < 0.000001f ? 0.0f : tN / tD);

			// get the difference of the two closest points
			Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)

			return dP.sqrMagnitude;   // return the closest distance
		}

		/** Squared distance between two points in the XZ plane */
		public static float SqrDistanceXZ (Vector3 a, Vector3 b) {
			var delta = a-b;

			return delta.x*delta.x+delta.z*delta.z;
		}

		/** Signed area of a triangle in the XZ plane multiplied by 2.
		 * This will be negative for clockwise triangles and positive for counter-clockwise ones
		 */
		public static long SignedTriangleAreaTimes2XZ (Int3 a, Int3 b, Int3 c) {
			return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
		}

		/** Signed area of a triangle in the XZ plane multiplied by 2.
		 * This will be negative for clockwise triangles and positive for counter-clockwise ones.
		 */
		public static float SignedTriangleAreaTimes2XZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
		}

		/** Returns if \a p lies on the right side of the line \a a - \a b.
		 * Uses XZ space. Does not return true if the points are colinear.
		 */
		public static bool RightXZ (Vector3 a, Vector3 b, Vector3 p) {
			return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) < -float.Epsilon;
		}

		/** Returns if \a p lies on the right side of the line \a a - \a b.
		 * Uses XZ space. Does not return true if the points are colinear.
		 */
		public static bool RightXZ (Int3 a, Int3 b, Int3 p) {
			return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) < 0;
		}

		/** Returns if \a p lies on the right side of the line \a a - \a b.
		 * Also returns true if the points are colinear.
		 */
		public static bool RightOrColinear (Vector2 a, Vector2 b, Vector2 p) {
			return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) <= 0;
		}

		/** Returns if \a p lies on the right side of the line \a a - \a b.
		 * Also returns true if the points are colinear.
		 */
		public static bool RightOrColinear (Int2 a, Int2 b, Int2 p) {
			return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) <= 0;
		}

		/** Returns if \a p lies on the left side of the line \a a - \a b.
		 * Uses XZ space. Also returns true if the points are colinear.
		 */
		public static bool RightOrColinearXZ (Vector3 a, Vector3 b, Vector3 p) {
			return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0;
		}

		/** Returns if \a p lies on the left side of the line \a a - \a b.
		 * Uses XZ space. Also returns true if the points are colinear.
		 */
		public static bool RightOrColinearXZ (Int3 a, Int3 b, Int3 p) {
			return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) <= 0;
		}

		/** Returns if the points a in a clockwise order.
		 * Will return true even if the points are colinear or very slightly counter-clockwise
		 * (if the signed area of the triangle formed by the points has an area less than or equals to float.Epsilon) */
		public static bool IsClockwiseMarginXZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z) <= float.Epsilon;
		}

		/** Returns if the points a in a clockwise order */
		public static bool IsClockwiseXZ (Vector3 a, Vector3 b, Vector3 c) {
			return (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z) < 0;
		}

		/** Returns if the points a in a clockwise order */
		public static bool IsClockwiseXZ (Int3 a, Int3 b, Int3 c) {
			return RightXZ(a, b, c);
		}

		/** Returns true if the points a in a clockwise order or if they are colinear */
		public static bool IsClockwiseOrColinearXZ (Int3 a, Int3 b, Int3 c) {
			return RightOrColinearXZ(a, b, c);
		}

		/** Returns true if the points a in a clockwise order or if they are colinear */
		public static bool IsClockwiseOrColinear (Int2 a, Int2 b, Int2 c) {
			return RightOrColinear(a, b, c);
		}

		/** Returns if the points are colinear (lie on a straight line) */
		public static bool IsColinearXZ (Int3 a, Int3 b, Int3 c) {
			return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) == 0;
		}

		/** Returns if the points are colinear (lie on a straight line) */
		public static bool IsColinearXZ (Vector3 a, Vector3 b, Vector3 c) {
			float v = (b.x-a.x)*(c.z-a.z)-(c.x-a.x)*(b.z-a.z);

			// Epsilon not chosen with much though, just that float.Epsilon was a bit too small.
			return v <= 0.0000001f && v >= -0.0000001f;
		}

		/** Returns if the points are colinear (lie on a straight line) */
		public static bool IsColinearAlmostXZ (Int3 a, Int3 b, Int3 c) {
			long v = (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);

			return v > -1 && v < 1;
		}

		/** Returns if the line segment \a start2 - \a end2 intersects the line segment \a start1 - \a end1.
		 * If only the endpoints coincide, the result is undefined (may be true or false).
		 */
		public static bool SegmentsIntersect (Int2 start1, Int2 end1, Int2 start2, Int2 end2) {
			return RightOrColinear(start1, end1, start2) != RightOrColinear(start1, end1, end2) && RightOrColinear(start2, end2, start1) != RightOrColinear(start2, end2, end1);
		}

		/** Returns if the line segment \a start2 - \a end2 intersects the line segment \a start1 - \a end1.
		 * If only the endpoints coincide, the result is undefined (may be true or false).
		 *
		 * \note XZ space
		 */
		public static bool SegmentsIntersectXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			return RightOrColinearXZ(start1, end1, start2) != RightOrColinearXZ(start1, end1, end2) && RightOrColinearXZ(start2, end2, start1) != RightOrColinearXZ(start2, end2, end1);
		}

		/** Returns if the two line segments intersects. The lines are NOT treated as infinite (just for clarification)
		 * \see IntersectionPoint
		 */
		public static bool SegmentsIntersectXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return false;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);
			float u = nom/den;
			float u2 = nom2/den;

			if (u < 0F || u > 1F || u2 < 0F || u2 > 1F) {
				return false;
			}

			return true;
		}

		/** Intersection point between two infinite lines.
		 * Note that start points and directions are taken as parameters instead of start and end points.
		 * Lines are treated as infinite. If the lines are parallel 'start1' will be returned.
		 * Intersections are calculated on the XZ plane.
		 *
		 * \see LineIntersectionPointXZ
		 */
		public static Vector3 LineDirIntersectionPointXZ (Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2) {
			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			return start1 + dir1*u;
		}

		/** Intersection point between two infinite lines.
		 * Note that start points and directions are taken as parameters instead of start and end points.
		 * Lines are treated as infinite. If the lines are parallel 'start1' will be returned.
		 * Intersections are calculated on the XZ plane.
		 *
		 * \see LineIntersectionPointXZ
		 */
		public static Vector3 LineDirIntersectionPointXZ (Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2, out bool intersects) {
			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/** Returns if the ray (start1, end1) intersects the segment (start2, end2).
		 * false is returned if the lines are parallel.
		 * Only the XZ coordinates are used.
		 * \todo Double check that this actually works
		 */
		public static bool RaySegmentIntersectXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			long den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return false;
			}

			long nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			long nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			//factor1 < 0
			// If both have the same sign, then nom/den < 0 and thus the segment cuts the ray before the ray starts
			if (!(nom < 0 ^ den < 0)) {
				return false;
			}

			//factor2 < 0
			if (!(nom2 < 0 ^ den < 0)) {
				return false;
			}

			if ((den >= 0 && nom2 > den) || (den < 0 && nom2 <= den)) {
				return false;
			}

			return true;
		}

		/** Returns the intersection factors for line 1 and line 2. The intersection factors is a distance along the line \a start - \a end where the other line intersects it.\n
		 * \code intersectionPoint = start1 + factor1 * (end1-start1) \endcode
		 * \code intersectionPoint2 = start2 + factor2 * (end2-start2) \endcode
		 * Lines are treated as infinite.\n
		 * false is returned if the lines are parallel and true if they are not.
		 * Only the XZ coordinates are used.
		 */
		public static bool LineIntersectionFactorXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2, out float factor1, out float factor2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			long den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				factor1 = 0;
				factor2 = 0;
				return false;
			}

			long nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			long nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			factor1 = (float)nom/den;
			factor2 = (float)nom2/den;

			return true;
		}

		/** Returns the intersection factors for line 1 and line 2. The intersection factors is a distance along the line \a start - \a end where the other line intersects it.\n
		 * \code intersectionPoint = start1 + factor1 * (end1-start1) \endcode
		 * \code intersectionPoint2 = start2 + factor2 * (end2-start2) \endcode
		 * Lines are treated as infinite.\n
		 * false is returned if the lines are parallel and true if they are not.
		 * Only the XZ coordinates are used.
		 */
		public static bool LineIntersectionFactorXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out float factor1, out float factor2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den <= 0.00001f && den >= -0.00001f) {
				factor1 = 0;
				factor2 = 0;
				return false;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			float u = nom/den;
			float u2 = nom2/den;

			factor1 = u;
			factor2 = u2;

			return true;
		}

		/** Returns the intersection factor for line 1 with ray 2.
		 * The intersection factors is a factor distance along the line \a start - \a end where the other line intersects it.\n
		 * \code intersectionPoint = start1 + factor * (end1-start1) \endcode
		 * Lines are treated as infinite.\n
		 *
		 * The second "line" is treated as a ray, meaning only matches on start2 or forwards towards end2 (and beyond) will be returned
		 * If the point lies on the wrong side of the ray start, Nan will be returned.
		 *
		 * NaN is returned if the lines are parallel. */
		public static float LineRayIntersectionFactorXZ (Int3 start1, Int3 end1, Int3 start2, Int3 end2) {
			Int3 dir1 = end1-start1;
			Int3 dir2 = end2-start2;

			int den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return float.NaN;
			}

			int nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			int nom2 = dir1.x*(start1.z-start2.z) - dir1.z * (start1.x - start2.x);

			if ((float)nom2/den < 0) {
				return float.NaN;
			}
			return (float)nom/den;
		}

		/** Returns the intersection factor for line 1 with line 2.
		 * The intersection factor is a distance along the line \a start1 - \a end1 where the line \a start2 - \a end2 intersects it.\n
		 * \code intersectionPoint = start1 + intersectionFactor * (end1-start1) \endcode.
		 * Lines are treated as infinite.\n
		 * -1 is returned if the lines are parallel (note that this is a valid return value if they are not parallel too) */
		public static float LineIntersectionFactorXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				return -1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float u = nom/den;

			return u;
		}

		/** Returns the intersection point between the two lines. Lines are treated as infinite. \a start1 is returned if the lines are parallel */
		public static Vector3 LineIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2) {
			bool s;

			return LineIntersectionPointXZ(start1, end1, start2, end2, out s);
		}

		/** Returns the intersection point between the two lines. Lines are treated as infinite. \a start1 is returned if the lines are parallel */
		public static Vector3 LineIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z*dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);

			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/** Returns the intersection point between the two lines. Lines are treated as infinite. \a start1 is returned if the lines are parallel */
		public static Vector2 LineIntersectionPoint (Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2) {
			bool s;

			return LineIntersectionPoint(start1, end1, start2, end2, out s);
		}

		/** Returns the intersection point between the two lines. Lines are treated as infinite. \a start1 is returned if the lines are parallel */
		public static Vector2 LineIntersectionPoint (Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out bool intersects) {
			Vector2 dir1 = end1-start1;
			Vector2 dir2 = end2-start2;

			float den = dir2.y*dir1.x - dir2.x * dir1.y;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.y-start2.y)- dir2.y*(start1.x-start2.x);

			float u = nom/den;

			intersects = true;
			return start1 + dir1*u;
		}

		/** Returns the intersection point between the two line segments in XZ space.
		 * Lines are NOT treated as infinite. \a start1 is returned if the line segments do not intersect
		 * The point will be returned along the line [start1, end1] (this matters only for the y coordinate).
		 */
		public static Vector3 SegmentIntersectionPointXZ (Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects) {
			Vector3 dir1 = end1-start1;
			Vector3 dir2 = end2-start2;

			float den = dir2.z * dir1.x - dir2.x * dir1.z;

			if (den == 0) {
				intersects = false;
				return start1;
			}

			float nom = dir2.x*(start1.z-start2.z)- dir2.z*(start1.x-start2.x);
			float nom2 = dir1.x*(start1.z-start2.z) - dir1.z*(start1.x-start2.x);
			float u = nom/den;
			float u2 = nom2/den;

			if (u < 0F || u > 1F || u2 < 0F || u2 > 1F) {
				intersects = false;
				return start1;
			}

			intersects = true;
			return start1 + dir1*u;
		}

		/** Does the line segment intersect the bounding box.
		 * The line is NOT treated as infinite.
		 * \author Slightly modified code from http://www.3dkingdoms.com/weekly/weekly.php?a=21
		 */
		public static bool SegmentIntersectsBounds (Bounds bounds, Vector3 a, Vector3 b) {
			// Put segment in box space
			a -= bounds.center;
			b -= bounds.center;

			// Get line midpoint and extent
			var LMid = (a + b) * 0.5F;
			var L = (a - LMid);
			var LExt = new Vector3(Math.Abs(L.x), Math.Abs(L.y), Math.Abs(L.z));

			Vector3 extent = bounds.extents;

			// Use Separating Axis Test
			// Separation vector from box center to segment center is LMid, since the line is in box space
			if (Math.Abs(LMid.x) > extent.x + LExt.x) return false;
			if (Math.Abs(LMid.y) > extent.y + LExt.y) return false;
			if (Math.Abs(LMid.z) > extent.z + LExt.z) return false;
			// Crossproducts of line and each axis
			if (Math.Abs(LMid.y * L.z - LMid.z * L.y) > (extent.y * LExt.z + extent.z * LExt.y)) return false;
			if (Math.Abs(LMid.x * L.z - LMid.z * L.x) > (extent.x * LExt.z + extent.z * LExt.x)) return false;
			if (Math.Abs(LMid.x * L.y - LMid.y * L.x) > (extent.x * LExt.y + extent.y * LExt.x)) return false;
			// No separating axis, the line intersects
			return true;
		}

		/** Intersection of a line and a circle.
		 * Returns the greatest t such that segmentStart+t*(segmentEnd-segmentStart) lies on the circle.
		 *
		 * In case the line does not intersect with the circle, the closest point on the line
		 * to the circle will be returned.
		 *
		 * \note Works for line and sphere in 3D space as well.
		 *
		 * \see http://mathworld.wolfram.com/Circle-LineIntersection.html
		 * \see https://en.wikipedia.org/wiki/Intersection_(Euclidean_geometry)#A_line_and_a_circle
		 */
		public static float LineCircleIntersectionFactor (Vector3 circleCenter, Vector3 linePoint1, Vector3 linePoint2, float radius) {
			float segmentLength;
			var normalizedDirection = Normalize(linePoint2 - linePoint1, out segmentLength);
			var dirToStart = linePoint1 - circleCenter;

			var dot = Vector3.Dot(dirToStart, normalizedDirection);
			var discriminant = dot * dot - (dirToStart.sqrMagnitude - radius*radius);

			if (discriminant < 0) {
				// No intersection, pick closest point on segment
				discriminant = 0;
			}

			var t = -dot + Mathf.Sqrt(discriminant);
			return segmentLength > 0.00001f ? t / segmentLength : 0;
		}

		/** True if the matrix will reverse orientations of faces.
		 *
		 * Scaling by a negative value along an odd number of axes will reverse
		 * the orientation of e.g faces on a mesh. This must be counter adjusted
		 * by for example the recast rasterization system to be able to handle
		 * meshes with negative scales properly.
		 *
		 * We can find out if they are flipped by finding out how the signed
		 * volume of a unit cube is transformed when applying the matrix
		 *
		 * If the (signed) volume turns out to be negative
		 * that also means that the orientation of it has been reversed.
		 *
		 * \see https://en.wikipedia.org/wiki/Normal_(geometry)
		 * \see https://en.wikipedia.org/wiki/Parallelepiped
		 */
		public static bool ReversesFaceOrientations (Matrix4x4 matrix) {
			var dX = matrix.MultiplyVector(new Vector3(1, 0, 0));
			var dY = matrix.MultiplyVector(new Vector3(0, 1, 0));
			var dZ = matrix.MultiplyVector(new Vector3(0, 0, 1));

			// Calculate the signed volume of the parallelepiped
			var volume = Vector3.Dot(Vector3.Cross(dX, dY), dZ);

			return volume < 0;
		}

		/** True if the matrix will reverse orientations of faces in the XZ plane.
		 * Almost the same as ReversesFaceOrientations, but this method assumes
		 * that scaling a face with a negative scale along the Y axis does not
		 * reverse the orientation of the face.
		 *
		 * This is used for navmesh cuts.
		 *
		 * Scaling by a negative value along one axis or rotating
		 * it so that it is upside down will reverse
		 * the orientation of the cut, so we need to be reverse
		 * it again as a countermeasure.
		 * However if it is flipped along two axes it does not need to
		 * be reversed.
		 * We can handle all these cases by finding out how a unit square formed
		 * by our forward axis and our rightward axis is transformed in XZ space
		 * when applying the local to world matrix.
		 * If the (signed) area of the unit square turns out to be negative
		 * that also means that the orientation of it has been reversed.
		 * The signed area is calculated using a cross product of the vectors.
		 */
		public static bool ReversesFaceOrientationsXZ (Matrix4x4 matrix) {
			var dX = matrix.MultiplyVector(new Vector3(1, 0, 0));
			var dZ = matrix.MultiplyVector(new Vector3(0, 0, 1));

			// Take the cross product of the vectors projected onto the XZ plane
			var cross = (dX.x*dZ.z - dZ.x*dX.z);

			return cross < 0;
		}

		/** Normalize vector and also return the magnitude.
		 * This is more efficient than calculating the magnitude and normalizing separately
		 */
		public static Vector3 Normalize (Vector3 v, out float magnitude) {
			magnitude = v.magnitude;
			// This is the same constant that Unity uses
			if (magnitude > 1E-05f) {
				return v / magnitude;
			} else {
				return Vector3.zero;
			}
		}

		/** Normalize vector and also return the magnitude.
		 * This is more efficient than calculating the magnitude and normalizing separately
		 */
		public static Vector2 Normalize (Vector2 v, out float magnitude) {
			magnitude = v.magnitude;
			// This is the same constant that Unity uses
			if (magnitude > 1E-05f) {
				return v / magnitude;
			} else {
				return Vector2.zero;
			}
		}

		/* Clamp magnitude along the X and Z axes.
		 * The y component will not be changed.
		 */
		public static Vector3 ClampMagnitudeXZ (Vector3 v, float maxMagnitude) {
			float squaredMagnitudeXZ = v.x*v.x + v.z*v.z;

			if (squaredMagnitudeXZ > maxMagnitude*maxMagnitude && maxMagnitude > 0) {
				var factor = maxMagnitude / Mathf.Sqrt(squaredMagnitudeXZ);
				v.x *= factor;
				v.z *= factor;
			}
			return v;
		}

		/* Magnitude in the XZ plane */
		public static float MagnitudeXZ (Vector3 v) {
			return Mathf.Sqrt(v.x*v.x + v.z*v.z);
		}
	}

	/** Utility functions for working with numbers and strings.
	 * \ingroup utils
	 * \see Polygon
	 * \see VectorMath
	 */
	public static class AstarMath {
		/** Returns the closest point on the line. The line is treated as infinite.
		 * \see NearestPointStrict
		 */
		[System.Obsolete("Use VectorMath.ClosestPointOnLine instead")]
		public static Vector3 NearestPoint (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			return VectorMath.ClosestPointOnLine(lineStart, lineEnd, point);
		}

		[System.Obsolete("Use VectorMath.ClosestPointOnLineFactor instead")]
		public static float NearestPointFactor (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			return VectorMath.ClosestPointOnLineFactor(lineStart, lineEnd, point);
		}

		/** Factor of the nearest point on the segment.
		 * Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		 * The closest point can be got by (end-start)*factor + start;
		 */
		[System.Obsolete("Use VectorMath.ClosestPointOnLineFactor instead")]
		public static float NearestPointFactor (Int3 lineStart, Int3 lineEnd, Int3 point) {
			return VectorMath.ClosestPointOnLineFactor(lineStart, lineEnd, point);
		}

		/** Factor of the nearest point on the segment.
		 * Returned value is in the range [0,1] if the point lies on the segment otherwise it just lies on the line.
		 * The closest point can be got by (end-start)*factor + start;
		 */
		[System.Obsolete("Use VectorMath.ClosestPointOnLineFactor instead")]
		public static float NearestPointFactor (Int2 lineStart, Int2 lineEnd, Int2 point) {
			return VectorMath.ClosestPointOnLineFactor(lineStart, lineEnd, point);
		}

		/** Returns the closest point on the line segment. The line is NOT treated as infinite.
		 * \see NearestPoint
		 */
		[System.Obsolete("Use VectorMath.ClosestPointOnSegment instead")]
		public static Vector3 NearestPointStrict (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			return VectorMath.ClosestPointOnSegment(lineStart, lineEnd, point);
		}

		/** Returns the closest point on the line segment on the XZ plane. The line is NOT treated as infinite.
		 * \see NearestPoint
		 */
		[System.Obsolete("Use VectorMath.ClosestPointOnSegmentXZ instead")]
		public static Vector3 NearestPointStrictXZ (Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			return VectorMath.ClosestPointOnSegmentXZ(lineStart, lineEnd, point);
		}

		/** Returns the approximate shortest squared distance between x,z and the line p-q.
		 * The line is considered infinite.
		 * This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		 */
		[System.Obsolete("Use VectorMath.SqrDistancePointSegmentApproximate instead")]
		public static float DistancePointSegment (int x, int z, int px, int pz, int qx, int qz) {
			return VectorMath.SqrDistancePointSegmentApproximate(x, z, px, pz, qx, qz);
		}

		/** Returns the approximate shortest squared distance between x,z and the line p-q.
		 * The line is considered infinite.
		 * This function is not entirely exact, but it is about twice as fast as DistancePointSegment2.
		 */
		[System.Obsolete("Use VectorMath.SqrDistancePointSegmentApproximate instead")]
		public static float DistancePointSegment (Int3 a, Int3 b, Int3 p) {
			return VectorMath.SqrDistancePointSegmentApproximate(a, b, p);
		}

		/** Returns the squared distance between c and the line a-b. The line is not considered infinite. */
		[System.Obsolete("Use VectorMath.SqrDistancePointSegment instead")]
		public static float DistancePointSegmentStrict (Vector3 a, Vector3 b, Vector3 p) {
			return VectorMath.SqrDistancePointSegment(a, b, p);
		}
        public static long DistancePointSegmentStrict(Int3 a, Int3 b, Int3 p)
        {
            Int3 lhs = AstarMath.NearestPointStrict(ref a, ref b, ref p);
            return (lhs - p).sqrMagnitudeLong;
        }

        public static Int3 NearestPointStrict(ref Int3 lineStart, ref Int3 lineEnd, ref Int3 point)
        {
            Int3 vInt = lineEnd - lineStart;
            long sqrMagnitudeLong = vInt.sqrMagnitudeLong;
            if (sqrMagnitudeLong == 0L)
            {
                return lineStart;
            }
            long num = Int3.DotLong(point - lineStart, vInt);
            num = IntMath.Clamp(num, 0L, sqrMagnitudeLong);
            return IntMath.Divide(vInt, num, sqrMagnitudeLong) + lineStart;
        }

        /** Returns a point on a cubic bezier curve. \a t is clamped between 0 and 1 */
        [System.Obsolete("Use AstarSplines.CubicBezier instead")]
		public static Vector3 CubicBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			return AstarSplines.CubicBezier(p0, p1, p2, p3, t);
		}

		/** Maps a value between startMin and startMax to be between 0 and 1 */
		public static float MapTo (float startMin, float startMax, float value) {
			if (startMax != startMin) {
				value -= startMin;
				value /= (startMax - startMin);
				value = Mathf.Clamp01(value);
			} else {
				value = 0;
			}
			return value;
		}

		/** Maps a value between startMin and startMax to be between targetMin and targetMax */
		public static float MapTo (float startMin, float startMax, float targetMin, float targetMax, float value) {
			value -= startMin;
			value /= (startMax-startMin);
			value = Mathf.Clamp01(value);
			value *= (targetMax-targetMin);
			value += targetMin;
			return value;
		}

		/** Returns a nicely formatted string for the number of bytes (KiB, MiB, GiB etc). Uses decimal names (KB, Mb - 1000) but calculates using binary values (KiB, MiB - 1024) */
		public static string FormatBytesBinary (int bytes) {
			double sign = bytes >= 0 ? 1D : -1D;

			bytes = bytes >= 0 ? bytes : -bytes;

			if (bytes < 1024) {
				return (bytes*sign)+" bytes";
			}
			if (bytes < 1024*1024) {
				return ((bytes/1024D)*sign).ToString("0.0") + " kb";
			}
			if (bytes < 1024*1024*1024) {
				return ((bytes/(1024D*1024D))*sign).ToString("0.0") +" mb";
			}
			return ((bytes/(1024D*1024D*1024D))*sign).ToString("0.0") +" gb";
		}

		/** Returns bit number \a b from int \a a. The bit number is zero based. Relevant \a b values are from 0 to 31\n
		 * Equals to (a >> b) & 1
		 */
		static int Bit (int a, int b) {
			return (a >> b) & 1;
			//return (a & (1 << b)) >> b; //Original code, one extra shift operation required
		}

		/** Returns a nice color from int \a i with alpha \a a. Got code from the open-source Recast project, works really well\n
		 * Seems like there are only 64 possible colors from studying the code
		 */
		public static Color IntToColor (int i, float a) {
			int r = Bit(i, 1) + Bit(i, 3) * 2 + 1;
			int g = Bit(i, 2) + Bit(i, 4) * 2 + 1;
			int b = Bit(i, 0) + Bit(i, 5) * 2 + 1;

			return new Color(r*0.25F, g*0.25F, b*0.25F, a);
		}

		/**
		 * Converts an HSV color to an RGB color.
		 * According to the algorithm described at http://en.wikipedia.org/wiki/HSL_and_HSV
		 *
		 * @author Wikipedia
		 * @return the RGB representation of the color.
		 */
		public static Color HSVToRGB (float h, float s, float v) {
			float Min;
			float Chroma;
			float Hdash;
			float X;
			float r = 0, g = 0, b = 0;

			Chroma = s * v;
			Hdash = h / 60.0f;
			X = Chroma * (1.0f - System.Math.Abs((Hdash % 2.0f) - 1.0f));

			if (Hdash < 1.0f) {
				r = Chroma;
				g = X;
			} else if (Hdash < 2.0f) {
				r = X;
				g = Chroma;
			} else if (Hdash < 3.0f) {
				g = Chroma;
				b = X;
			} else if (Hdash < 4.0f) {
				g = X;
				b = Chroma;
			} else if (Hdash < 5.0f) {
				r = X;
				b = Chroma;
			} else if (Hdash < 6.0f) {
				r = Chroma;
				b = X;
			}

			Min = v - Chroma;

			r += Min;
			g += Min;
			b += Min;

			return new Color(r, g, b);
		}

		/** Squared distance between two points on the XZ plane */
		[System.Obsolete("Use VectorMath.SqrDistanceXZ instead")]
		public static float SqrMagnitudeXZ (Vector3 a, Vector3 b) {
			return VectorMath.SqrDistanceXZ(a, b);
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static float DistancePointSegment2 (int x, int z, int px, int pz, int qx, int qz) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static float DistancePointSegment2 (Vector3 a, Vector3 b, Vector3 p) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Int3.GetHashCode instead */
		[System.Obsolete("Use Int3.GetHashCode instead", true)]
		public static int ComputeVertexHash (int x, int y, int z) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static float Hermite (float start, float end, float value) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static float MapToRange (float targetMin, float targetMax, float value) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static string FormatBytes (int bytes) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static float MagnitudeXZ (Vector3 a, Vector3 b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Obsolete */
		[System.Obsolete("Obsolete", true)]
		public static int Repeat (int i, int n) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Abs instead */
		[System.Obsolete("Use Mathf.Abs instead", true)]
		public static float Abs (float a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Abs instead */
		[System.Obsolete("Use Mathf.Abs instead", true)]
		public static int Abs (int a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Min instead */
		[System.Obsolete("Use Mathf.Min instead", true)]
		public static float Min (float a, float b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Min instead */
		[System.Obsolete("Use Mathf.Min instead", true)]
		public static int Min (int a, int b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Min instead */
		[System.Obsolete("Use Mathf.Min instead", true)]
		public static uint Min (uint a, uint b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Max instead */
		[System.Obsolete("Use Mathf.Max instead", true)]
		public static float Max (float a, float b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Max instead */
		[System.Obsolete("Use Mathf.Max instead", true)]
		public static int Max (int a, int b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Max instead */
		[System.Obsolete("Use Mathf.Max instead", true)]
		public static uint Max (uint a, uint b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Max instead */
		[System.Obsolete("Use Mathf.Max instead", true)]
		public static ushort Max (ushort a, ushort b) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Sign instead */
		[System.Obsolete("Use Mathf.Sign instead", true)]
		public static float Sign (float a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Sign instead */
		[System.Obsolete("Use Mathf.Sign instead", true)]
		public static int Sign (int a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Clamp instead */
		[System.Obsolete("Use Mathf.Clamp instead", true)]
		public static float Clamp (float a, float b, float c) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Clamp instead */
		[System.Obsolete("Use Mathf.Clamp instead", true)]
		public static int Clamp (int a, int b, int c) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Clamp01 instead */
		[System.Obsolete("Use Mathf.Clamp01 instead", true)]
		public static float Clamp01 (float a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Clamp01 instead */
		[System.Obsolete("Use Mathf.Clamp01 instead", true)]
		public static int Clamp01 (int a) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.Lerp instead */
		[System.Obsolete("Use Mathf.Lerp instead", true)]
		public static float Lerp (float a, float b, float t) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.RoundToInt instead */
		[System.Obsolete("Use Mathf.RoundToInt instead", true)]
		public static int RoundToInt (float v) {
			throw new System.NotImplementedException("Obsolete");
		}

		/** \deprecated Use Mathf.RoundToInt instead */
		[System.Obsolete("Use Mathf.RoundToInt instead", true)]
		public static int RoundToInt (double v) {
			throw new System.NotImplementedException("Obsolete");
		}
	}

    /** Utility functions for working with polygons, lines, and other vector math.
	 * All functions which accepts Vector3s but work in 2D space uses the XZ space if nothing else is said.
	 *
	 * \version A lot of functions in this class have been moved to the VectorMath class
	 * the names have changed slightly and everything now consistently assumes a left handed
	 * coordinate system now instead of sometimes using a left handed one and sometimes
	 * using a right handed one. This is why the 'Left' methods redirect to methods
	 * named 'Right'. The functionality is exactly the same.
	 *
	 * \ingroup utils
	 */
    public class Polygon
    {
        public static List<Vector3> hullCache = new List<Vector3>();

        public static long TriangleArea2(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
        }

        public static float TriangleArea2(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
        }

        public static long TriangleArea(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
        }

        public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
        }

        public static bool ContainsPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            return Polygon.IsClockwiseMargin(a, b, p) && Polygon.IsClockwiseMargin(b, c, p) && Polygon.IsClockwiseMargin(c, a, p);
        }

        public static bool ContainsPoint(Int2 a, Int2 b, Int2 c, Int2 p)
        {
            return Polygon.IsClockwiseMargin(a, b, p) && Polygon.IsClockwiseMargin(b, c, p) && Polygon.IsClockwiseMargin(c, a, p);
        }

        public static bool ContainsPoint(Int3 a, Int3 b, Int3 c, Int3 p)
        {
            return Polygon.IsClockwiseMargin(a, b, p) && Polygon.IsClockwiseMargin(b, c, p) && Polygon.IsClockwiseMargin(c, a, p);
        }

        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            int num = polyPoints.Length - 1;
            bool flag = false;
            int i = 0;
            while (i < polyPoints.Length)
            {
                if (((polyPoints[i].y <= p.y && p.y < polyPoints[num].y) || (polyPoints[num].y <= p.y && p.y < polyPoints[i].y)) && p.x < (polyPoints[num].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[num].y - polyPoints[i].y) + polyPoints[i].x)
                {
                    flag = !flag;
                }
                num = i++;
            }
            return flag;
        }

        public static bool ContainsPoint(Vector3[] polyPoints, Vector3 p)
        {
            int num = polyPoints.Length - 1;
            bool flag = false;
            int i = 0;
            while (i < polyPoints.Length)
            {
                if (((polyPoints[i].z <= p.z && p.z < polyPoints[num].z) || (polyPoints[num].z <= p.z && p.z < polyPoints[i].z)) && p.x < (polyPoints[num].x - polyPoints[i].x) * (p.z - polyPoints[i].z) / (polyPoints[num].z - polyPoints[i].z) + polyPoints[i].x)
                {
                    flag = !flag;
                }
                num = i++;
            }
            return flag;
        }

        public static bool LeftNotColinear(Vector3 a, Vector3 b, Vector3 p)
        {
            return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) < -1.401298E-45f;
        }

        public static bool Left(Vector3 a, Vector3 b, Vector3 p)
        {
            return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0f;
        }

        public static bool Left(Vector2 a, Vector2 b, Vector2 p)
        {
            return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) <= 0f;
        }

        public static bool Left(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) <= 0L;
        }

        public static bool LeftNotColinear(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) < 0L;
        }

        public static bool Left(Int2 a, Int2 b, Int2 c)
        {
            return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y) <= 0L;
        }

        public static bool IsClockwiseMargin(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) <= 1.401298E-45f;
        }

        public static bool IsClockwise(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) < 0f;
        }

        public static bool IsClockwise(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) < 0L;
        }

        public static bool IsClockwiseMargin(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) <= 0L;
        }

        public static bool IsClockwiseMargin(Int2 a, Int2 b, Int2 c)
        {
            return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y) <= 0L;
        }

        public static bool IsColinear(Int3 a, Int3 b, Int3 c)
        {
            return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) == 0L;
        }

        public static bool IsColinearAlmost(Int3 a, Int3 b, Int3 c)
        {
            long num = (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
            return num > -1L && num < 1L;
        }

        public static bool IsColinear(Vector3 a, Vector3 b, Vector3 c)
        {
            float num = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
            return num <= 1E-07f && num >= -1E-07f;
        }

        public static bool IntersectsUnclamped(Vector3 a, Vector3 b, Vector3 a2, Vector3 b2)
        {
            return Polygon.Left(a, b, a2) != Polygon.Left(a, b, b2);
        }

        public static bool Intersects(Int2 a, Int2 b, Int2 a2, Int2 b2)
        {
            return Polygon.Left(a, b, a2) != Polygon.Left(a, b, b2) && Polygon.Left(a2, b2, a) != Polygon.Left(a2, b2, b);
        }

        public static bool Intersects(Int3 a, Int3 b, Int3 a2, Int3 b2)
        {
            return Polygon.Left(a, b, a2) != Polygon.Left(a, b, b2) && Polygon.Left(a2, b2, a) != Polygon.Left(a2, b2, b);
        }

        public static bool Intersects(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
        {
            Vector3 vector = end1 - start1;
            Vector3 vector2 = end2 - start2;
            float num = vector2.z * vector.x - vector2.x * vector.z;
            if (num == 0f)
            {
                return false;
            }
            float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
            float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
            float num4 = num2 / num;
            float num5 = num3 / num;
            return num4 >= 0f && num4 <= 1f && num5 >= 0f && num5 <= 1f;
        }

        public static Vector3 IntersectionPointOptimized(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2)
        {
            float num = dir2.z * dir1.x - dir2.x * dir1.z;
            if (num == 0f)
            {
                return start1;
            }
            float num2 = dir2.x * (start1.z - start2.z) - dir2.z * (start1.x - start2.x);
            float num3 = num2 / num;
            return start1 + dir1 * num3;
        }

        public static Vector3 IntersectionPointOptimized(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2, out bool intersects)
        {
            float num = dir2.z * dir1.x - dir2.x * dir1.z;
            if (num == 0f)
            {
                intersects = false;
                return start1;
            }
            float num2 = dir2.x * (start1.z - start2.z) - dir2.z * (start1.x - start2.x);
            float num3 = num2 / num;
            intersects = true;
            return start1 + dir1 * num3;
        }

        public static bool IntersectionFactorRaySegment(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
        {
            Int3 vInt = end1 - start1;
            Int3 vInt2 = end2 - start2;
            long num = (long)(vInt2.z * vInt.x - vInt2.x * vInt.z);
            if (num == 0L)
            {
                return false;
            }
            long num2 = (long)(vInt2.x * (start1.z - start2.z) - vInt2.z * (start1.x - start2.x));
            long num3 = (long)(vInt.x * (start1.z - start2.z) - vInt.z * (start1.x - start2.x));
            return (num2 < 0L ^ num < 0L) && (num3 < 0L ^ num < 0L) && (num < 0L || num3 <= num) && (num >= 0L || num3 > num);
        }

        public static bool IntersectionFactor(Int3 start1, Int3 end1, Int3 start2, Int3 end2, out float factor1, out float factor2)
        {
            Int3 vInt = end1 - start1;
            Int3 vInt2 = end2 - start2;
            long num = (long)(vInt2.z * vInt.x - vInt2.x * vInt.z);
            if (num == 0L)
            {
                factor1 = 0f;
                factor2 = 0f;
                return false;
            }
            long num2 = (long)(vInt2.x * (start1.z - start2.z) - vInt2.z * (start1.x - start2.x));
            long num3 = (long)(vInt.x * (start1.z - start2.z) - vInt.z * (start1.x - start2.x));
            factor1 = (float)num2 / (float)num;
            factor2 = (float)num3 / (float)num;
            return true;
        }

        public static bool IntersectionFactor(Int3 start1, Int3 end1, Int3 start2, Int3 end2, out IntFactor factor1, out IntFactor factor2)
        {
            Int3 vInt = end1 - start1;
            Int3 vInt2 = end2 - start2;
            long num = (long)(vInt2.z * vInt.x - vInt2.x * vInt.z);
            if (num == 0L)
            {
                factor1 = IntFactor.zero;
                factor2 = IntFactor.zero;
                return false;
            }
            long nom = (long)(vInt2.x * (start1.z - start2.z) - vInt2.z * (start1.x - start2.x));
            long nom2 = (long)(vInt.x * (start1.z - start2.z) - vInt.z * (start1.x - start2.x));
            factor1 = default(IntFactor);
            IntFactor vFactor = factor1;
            vFactor.numerator = nom;
            vFactor.denominator = num;
            factor1 = vFactor;
            factor2 = default(IntFactor);
            vFactor = factor2;
            vFactor.numerator = nom2;
            vFactor.denominator = num;
            factor2 = vFactor;
            return true;
        }

        public static bool IntersectionFactor(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out float factor1, out float factor2)
        {
            Vector3 vector = end1 - start1;
            Vector3 vector2 = end2 - start2;
            float num = vector2.z * vector.x - vector2.x * vector.z;
            if (num <= 1E-05f && num >= -1E-05f)
            {
                factor1 = 0f;
                factor2 = 0f;
                return false;
            }
            float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
            float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
            float num4 = num2 / num;
            float num5 = num3 / num;
            factor1 = num4;
            factor2 = num5;
            return true;
        }

        public static float IntersectionFactorRay(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
        {
            Int3 vInt = end1 - start1;
            Int3 vInt2 = end2 - start2;
            int num = vInt2.z * vInt.x - vInt2.x * vInt.z;
            if (num == 0)
            {
                return float.NaN;
            }
            int num2 = vInt2.x * (start1.z - start2.z) - vInt2.z * (start1.x - start2.x);
            int num3 = vInt.x * (start1.z - start2.z) - vInt.z * (start1.x - start2.x);
            if ((float)num3 / (float)num < 0f)
            {
                return float.NaN;
            }
            return (float)num2 / (float)num;
        }

        public static float IntersectionFactor(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
        {
            Vector3 vector = end1 - start1;
            Vector3 vector2 = end2 - start2;
            float num = vector2.z * vector.x - vector2.x * vector.z;
            if (num == 0f)
            {
                return -1f;
            }
            float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
            return num2 / num;
        }

        public static Vector3 IntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
        {
            bool flag;
            return Polygon.IntersectionPoint(start1, end1, start2, end2, out flag);
        }

        public static Vector3 IntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
        {
            Vector3 vector = end1 - start1;
            Vector3 vector2 = end2 - start2;
            float num = vector2.z * vector.x - vector2.x * vector.z;
            if (num == 0f)
            {
                intersects = false;
                return start1;
            }
            float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
            float num3 = num2 / num;
            intersects = true;
            return start1 + vector * num3;
        }

        public static Int3 IntersectionPoint(ref Int3 start1, ref Int3 end1, ref Int3 start2, ref Int3 end2, out bool intersects)
        {
            Int3 a = end1 - start1;
            Int3 vInt = end2 - start2;
            long num = (long)vInt.z * (long)a.x - (long)vInt.x * (long)a.z;
            if (num == 0L)
            {
                intersects = false;
                return start1;
            }
            long m = (long)vInt.x * ((long)start1.z - (long)start2.z) - (long)vInt.z * ((long)start1.x - (long)start2.x);
            intersects = true;
            Int3 lhs = IntMath.Divide(a, m, num);
            return lhs + start1;
        }

        public static Vector2 IntersectionPoint(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
        {
            bool flag;
            return Polygon.IntersectionPoint(start1, end1, start2, end2, out flag);
        }

        public static Vector2 IntersectionPoint(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out bool intersects)
        {
            Vector2 vector = end1 - start1;
            Vector2 vector2 = end2 - start2;
            float num = vector2.y * vector.x - vector2.x * vector.y;
            if (num == 0f)
            {
                intersects = false;
                return start1;
            }
            float num2 = vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x);
            float num3 = num2 / num;
            intersects = true;
            return start1 + vector * num3;
        }

        public static Vector3 SegmentIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
        {
            Vector3 vector = end1 - start1;
            Vector3 vector2 = end2 - start2;
            float num = vector2.z * vector.x - vector2.x * vector.z;
            if (num == 0f)
            {
                intersects = false;
                return start1;
            }
            float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
            float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
            float num4 = num2 / num;
            float num5 = num3 / num;
            if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
            {
                intersects = false;
                return start1;
            }
            intersects = true;
            return start1 + vector * num4;
        }

        public static Int3 SegmentIntersectionPoint(Int3 start1, Int3 end1, Int3 start2, Int3 end2, out bool intersects)
        {
            Int3 a = end1 - start1;
            Int3 vInt = end2 - start2;
            long num = (long)(vInt.z * a.x - vInt.x * a.z);
            if (num == 0L)
            {
                intersects = false;
                return start1;
            }
            long num2 = (long)(vInt.x * (start1.z - start2.z) - vInt.z * (start1.x - start2.x));
            long num3 = (long)(a.x * (start1.z - start2.z) - a.z * (start1.x - start2.x));
            IntFactor vFactor = new IntFactor
            {
                numerator = num2 * 1000L,
                denominator = num
            };
            IntFactor vFactor2 = default(IntFactor);
            vFactor2.numerator = num3 * 1000L;
            vFactor2.denominator = num;
            int integer = vFactor.integer;
            int integer2 = vFactor.integer;
            if (integer < 0 || integer > 1000 || integer2 < 0 || integer2 > 1000)
            {
                intersects = false;
                return start1;
            }
            intersects = true;
            return start1 + IntMath.Divide(a, (long)integer, 1000L);
        }

        public static Vector3[] ConvexHull(Vector3[] points)
        {
            if (points.Length == 0)
            {
                return new Vector3[0];
            }
            List<Vector3> list = Polygon.hullCache;
            list.Clear();
            int num = 0;
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].x < points[num].x)
                {
                    num = i;
                }
            }
            int num2 = num;
            int num3 = 0;
            while (true)
            {
                list.Add(points[num]);
                int num4 = 0;
                for (int j = 0; j < points.Length; j++)
                {
                    if (num4 == num || !Polygon.Left(points[num], points[num4], points[j]))
                    {
                        num4 = j;
                    }
                }
                num = num4;
                num3++;
                if (num3 > 10000)
                {
                    break;
                }
                if (num == num2)
                {
                    goto IL_E9;
                }
            }
            Debug.LogWarning("Infinite Loop in Convex Hull Calculation");
        IL_E9:
            return list.ToArray();
        }

        public static bool LineIntersectsBounds(Bounds bounds, Vector3 a, Vector3 b)
        {
            a -= bounds.center;
            b -= bounds.center;
            Vector3 vector = (a + b) * 0.5f;
            Vector3 vector2 = a - vector;
            Vector3 vector3 = new Vector3(Math.Abs(vector2.x), Math.Abs(vector2.y), Math.Abs(vector2.z));
            Vector3 extents = bounds.extents;
            return Math.Abs(vector.x) <= extents.x + vector3.x && Math.Abs(vector.y) <= extents.y + vector3.y && Math.Abs(vector.z) <= extents.z + vector3.z && Math.Abs(vector.y * vector2.z - vector.z * vector2.y) <= extents.y * vector3.z + extents.z * vector3.y && Math.Abs(vector.x * vector2.z - vector.z * vector2.x) <= extents.x * vector3.z + extents.z * vector3.x && Math.Abs(vector.x * vector2.y - vector.y * vector2.x) <= extents.x * vector3.y + extents.y * vector3.x;
        }

        public static Vector3[] Subdivide(Vector3[] path, int subdivisions)
        {
            subdivisions = ((subdivisions >= 0) ? subdivisions : 0);
            if (subdivisions == 0)
            {
                return path;
            }
            Vector3[] array = new Vector3[(path.Length - 1) * (int)Mathf.Pow(2f, (float)subdivisions) + 1];
            int num = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                float num2 = 1f / Mathf.Pow(2f, (float)subdivisions);
                for (float num3 = 0f; num3 < 1f; num3 += num2)
                {
                    array[num] = Vector3.Lerp(path[i], path[i + 1], Mathf.SmoothStep(0f, 1f, num3));
                    num++;
                }
            }
            array[num] = path[path.Length - 1];
            return array;
        }

        public static Vector3 ClosestPointOnTriangle(Vector3[] triangle, Vector3 point)
        {
            return Polygon.ClosestPointOnTriangle(triangle[0], triangle[1], triangle[2], point);
        }

        public static Vector3 ClosestPointOnTriangle(Vector3 tr0, Vector3 tr1, Vector3 tr2, Vector3 point)
        {
            Vector3 vector = tr0 - point;
            Vector3 vector2 = tr1 - tr0;
            Vector3 vector3 = tr2 - tr0;
            float sqrMagnitude = vector2.sqrMagnitude;
            float num = Vector3.Dot(vector2, vector3);
            float sqrMagnitude2 = vector3.sqrMagnitude;
            float num2 = Vector3.Dot(vector, vector2);
            float num3 = Vector3.Dot(vector, vector3);
            float num4 = sqrMagnitude * sqrMagnitude2 - num * num;
            float num5 = num * num3 - sqrMagnitude2 * num2;
            float num6 = num * num2 - sqrMagnitude * num3;
            if (num5 + num6 <= num4)
            {
                if (num5 < 0f)
                {
                    if (num6 < 0f)
                    {
                        if (num2 < 0f)
                        {
                            num6 = 0f;
                            if (-num2 >= sqrMagnitude)
                            {
                                num5 = 1f;
                            }
                            else
                            {
                                num5 = -num2 / sqrMagnitude;
                            }
                        }
                        else
                        {
                            num5 = 0f;
                            if (num3 >= 0f)
                            {
                                num6 = 0f;
                            }
                            else if (-num3 >= sqrMagnitude2)
                            {
                                num6 = 1f;
                            }
                            else
                            {
                                num6 = -num3 / sqrMagnitude2;
                            }
                        }
                    }
                    else
                    {
                        num5 = 0f;
                        if (num3 >= 0f)
                        {
                            num6 = 0f;
                        }
                        else if (-num3 >= sqrMagnitude2)
                        {
                            num6 = 1f;
                        }
                        else
                        {
                            num6 = -num3 / sqrMagnitude2;
                        }
                    }
                }
                else if (num6 < 0f)
                {
                    num6 = 0f;
                    if (num2 >= 0f)
                    {
                        num5 = 0f;
                    }
                    else if (-num2 >= sqrMagnitude)
                    {
                        num5 = 1f;
                    }
                    else
                    {
                        num5 = -num2 / sqrMagnitude;
                    }
                }
                else
                {
                    float num7 = 1f / num4;
                    num5 *= num7;
                    num6 *= num7;
                }
            }
            else if (num5 < 0f)
            {
                float num8 = num + num2;
                float num9 = sqrMagnitude2 + num3;
                if (num9 > num8)
                {
                    float num10 = num9 - num8;
                    float num11 = sqrMagnitude - 2f * num + sqrMagnitude2;
                    if (num10 >= num11)
                    {
                        num5 = 1f;
                        num6 = 0f;
                    }
                    else
                    {
                        num5 = num10 / num11;
                        num6 = 1f - num5;
                    }
                }
                else
                {
                    num5 = 0f;
                    if (num9 <= 0f)
                    {
                        num6 = 1f;
                    }
                    else if (num3 >= 0f)
                    {
                        num6 = 0f;
                    }
                    else
                    {
                        num6 = -num3 / sqrMagnitude2;
                    }
                }
            }
            else if (num6 < 0f)
            {
                float num8 = num + num3;
                float num9 = sqrMagnitude + num2;
                if (num9 > num8)
                {
                    float num10 = num9 - num8;
                    float num11 = sqrMagnitude - 2f * num + sqrMagnitude2;
                    if (num10 >= num11)
                    {
                        num6 = 1f;
                        num5 = 0f;
                    }
                    else
                    {
                        num6 = num10 / num11;
                        num5 = 1f - num6;
                    }
                }
                else
                {
                    num6 = 0f;
                    if (num9 <= 0f)
                    {
                        num5 = 1f;
                    }
                    else if (num2 >= 0f)
                    {
                        num5 = 0f;
                    }
                    else
                    {
                        num5 = -num2 / sqrMagnitude;
                    }
                }
            }
            else
            {
                float num10 = sqrMagnitude2 + num3 - num - num2;
                if (num10 <= 0f)
                {
                    num5 = 0f;
                    num6 = 1f;
                }
                else
                {
                    float num11 = sqrMagnitude - 2f * num + sqrMagnitude2;
                    if (num10 >= num11)
                    {
                        num5 = 1f;
                        num6 = 0f;
                    }
                    else
                    {
                        num5 = num10 / num11;
                        num6 = 1f - num5;
                    }
                }
            }
            return tr0 + num5 * vector2 + num6 * vector3;
        }

        public static float DistanceSegmentSegment3D(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
        {
            Vector3 vector = e1 - s1;
            Vector3 vector2 = e2 - s2;
            Vector3 vector3 = s1 - s2;
            float num = Vector3.Dot(vector, vector);
            float num2 = Vector3.Dot(vector, vector2);
            float num3 = Vector3.Dot(vector2, vector2);
            float num4 = Vector3.Dot(vector, vector3);
            float num5 = Vector3.Dot(vector2, vector3);
            float num6 = num * num3 - num2 * num2;
            float num7 = num6;
            float num8 = num6;
            float num9;
            float num10;
            if (num6 < 1E-06f)
            {
                num9 = 0f;
                num7 = 1f;
                num10 = num5;
                num8 = num3;
            }
            else
            {
                num9 = num2 * num5 - num3 * num4;
                num10 = num * num5 - num2 * num4;
                if (num9 < 0f)
                {
                    num9 = 0f;
                    num10 = num5;
                    num8 = num3;
                }
                else if (num9 > num7)
                {
                    num9 = num7;
                    num10 = num5 + num2;
                    num8 = num3;
                }
            }
            if (num10 < 0f)
            {
                num10 = 0f;
                if (-num4 < 0f)
                {
                    num9 = 0f;
                }
                else if (-num4 > num)
                {
                    num9 = num7;
                }
                else
                {
                    num9 = -num4;
                    num7 = num;
                }
            }
            else if (num10 > num8)
            {
                num10 = num8;
                if (-num4 + num2 < 0f)
                {
                    num9 = 0f;
                }
                else if (-num4 + num2 > num)
                {
                    num9 = num7;
                }
                else
                {
                    num9 = -num4 + num2;
                    num7 = num;
                }
            }
            float num11 = (Math.Abs(num9) >= 1E-06f) ? (num9 / num7) : 0f;
            float num12 = (Math.Abs(num10) >= 1E-06f) ? (num10 / num8) : 0f;
            return (vector3 + num11 * vector - num12 * vector2).sqrMagnitude;
        }
    }
}
