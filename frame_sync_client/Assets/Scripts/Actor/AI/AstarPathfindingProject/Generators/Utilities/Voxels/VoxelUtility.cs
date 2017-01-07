using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.Voxels {
	using Pathfinding.Util;

	/** Various utilities for voxel rasterization.
	 * \astarpro
	 */
	public class Utility {

		private static int[] clipPolygonIntCache = new int[21];
        public static int ClipPolygon(Int3[] vIn, int n, Int3[] vOut, int multi, int offset, int axis)
        {
            int[] array = Utility.clipPolygonIntCache;
            for (int i = 0; i < n; i++)
            {
                array[i] = multi * vIn[i][axis] + offset;
            }
            int num = 0;
            int j = 0;
            int num2 = n - 1;
            while (j < n)
            {
                bool flag = array[num2] >= 0;
                bool flag2 = array[j] >= 0;
                if (flag != flag2)
                {
                    double rhs = (double)array[num2] / (double)(array[num2] - array[j]);
                    vOut[num] = vIn[num2] + (vIn[j] - vIn[num2]) * rhs;
                    num++;
                }
                if (flag2)
                {
                    vOut[num] = vIn[j];
                    num++;
                }
                num2 = j;
                j++;
            }
            return num;
        }


        public static float Min (float a, float b, float c) {
			a = a < b ? a : b;
			return a < c ? a : c;
		}

		public static float Max (float a, float b, float c) {
			a = a > b ? a : b;
			return a > c ? a : c;
		}

		public static int Max (int a, int b, int c, int d) {
			a = a > b ? a : b;
			a = a > c ? a : c;
			return a > d ? a : d;
		}

		public static int Min (int a, int b, int c, int d) {
			a = a < b ? a : b;
			a = a < c ? a : c;
			return a < d ? a : d;
		}

		public static float Max (float a, float b, float c, float d) {
			a = a > b ? a : b;
			a = a > c ? a : c;
			return a > d ? a : d;
		}

		public static float Min (float a, float b, float c, float d) {
			a = a < b ? a : b;
			a = a < c ? a : c;
			return a < d ? a : d;
		}

		public static void CopyVector (float[] a, int i, Vector3 v) {
			a[i] = v.x;
			a[i+1] = v.y;
			a[i+2] = v.z;
		}

		/** Swaps the variables a and b */
		public static void Swap (ref int a, ref int b) {
			int tmp = a;

			a = b;
			b = tmp;
		}

		/** Removes duplicate vertices from the array and updates the triangle array.
		 * \returns The new array of vertices
		 */
		public static Int3[] RemoveDuplicateVertices (Int3[] vertices, int[] triangles) {
			// Get a dictionary from an object pool to avoid allocating a new one
			var firstVerts = ObjectPoolSimple<Dictionary<Int3, int> >.Claim();

			firstVerts.Clear();

			// Remove duplicate vertices
			var compressedPointers = new int[vertices.Length];

			int count = 0;
			for (int i = 0; i < vertices.Length; i++) {
				if (!firstVerts.ContainsKey(vertices[i])) {
					firstVerts.Add(vertices[i], count);
					compressedPointers[i] = count;
					vertices[count] = vertices[i];
					count++;
				} else {
					// There are some cases, rare but still there, that vertices are identical
					compressedPointers[i] = firstVerts[vertices[i]];
				}
			}

			firstVerts.Clear();
			ObjectPoolSimple<Dictionary<Int3, int> >.Release(ref firstVerts);

			for (int i = 0; i < triangles.Length; i++) {
				triangles[i] = compressedPointers[triangles[i]];
			}

			var compressed = new Int3[count];
			for (int i = 0; i < count; i++) compressed[i] = vertices[i];
			return compressed;
		}
	}
}
