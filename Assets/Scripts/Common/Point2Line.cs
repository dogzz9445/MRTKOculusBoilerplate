using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Common
{
    class Vector22Line
    {
        /// <summary>
        /// Uses the Douglas Peucker algorithm to reduce the number of Vector2s.
        /// </summary>
        /// <param name="Vector2s">The Vector2s.</param>
        /// <param name="Tolerance">The tolerance.</param>
        /// <returns></returns>
        public static List<Vector2> DouglasPeuckerReduction
            (List<Vector2> Vector2s, Double Tolerance)
        {
            if (Vector2s == null || Vector2s.Count < 3)
                return Vector2s;

            Int32 firstVector2 = 0;
            Int32 lastVector2 = Vector2s.Count - 1;
            List<Int32> Vector2IndexsToKeep = new List<Int32>();

            //Add the first and last index to the keepers
            Vector2IndexsToKeep.Add(firstVector2);
            Vector2IndexsToKeep.Add(lastVector2);

            //The first and the last Vector2 cannot be the same
            while (Vector2s[firstVector2].Equals(Vector2s[lastVector2]))
            {
                lastVector2--;
            }

            DouglasPeuckerReduction(Vector2s, firstVector2, lastVector2,
            Tolerance, ref Vector2IndexsToKeep);

            List<Vector2> returnVector2s = new List<Vector2>();
            Vector2IndexsToKeep.Sort();
            foreach (Int32 index in Vector2IndexsToKeep)
            {
                returnVector2s.Add(Vector2s[index]);
            }

            return returnVector2s;
        }

        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="Vector2s">The Vector2s.</param>
        /// <param name="firstVector2">The first Vector2.</param>
        /// <param name="lastVector2">The last Vector2.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="Vector2IndexsToKeep">The Vector2 index to keep.</param>
        private static void DouglasPeuckerReduction(List<Vector2>
            Vector2s, Int32 firstVector2, Int32 lastVector2, Double tolerance,
            ref List<Int32> Vector2IndexsToKeep)
        {
            Double maxDistance = 0;
            Int32 indexFarthest = 0;

            for (Int32 index = firstVector2; index < lastVector2; index++)
            {
                Double distance = PerpendicularDistance
                    (Vector2s[firstVector2], Vector2s[lastVector2], Vector2s[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest Vector2 that exceeds the tolerance
                Vector2IndexsToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(Vector2s, firstVector2,
                indexFarthest, tolerance, ref Vector2IndexsToKeep);
                DouglasPeuckerReduction(Vector2s, indexFarthest,
                lastVector2, tolerance, ref Vector2IndexsToKeep);
            }
        }

        /// <summary>
        /// The distance of a Vector2 from a line made from Vector21 and Vector22.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public static Double PerpendicularDistance
            (Vector2 Vector21, Vector2 Vector22, Vector2 Vector2)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            Double area = Math.Abs(.5 * (Vector21.x * Vector22.y + Vector22.x *
            Vector2.y + Vector2.x * Vector21.y - Vector22.x * Vector21.y - Vector2.x *
            Vector22.y - Vector21.x * Vector2.y));
            Double bottom = Math.Sqrt(Math.Pow(Vector21.x - Vector22.x, 2) +
            Math.Pow(Vector21.y - Vector22.y, 2));
            Double height = area / bottom * 2;

            return height;

            //Another option
            //Double A = Vector2.x - Vector21.x;
            //Double B = Vector2.Y - Vector21.Y;
            //Double C = Vector22.x - Vector21.x;
            //Double D = Vector22.Y - Vector21.Y;

            //Double dot = A * C + B * D;
            //Double len_sq = C * C + D * D;
            //Double param = dot / len_sq;

            //Double xx, yy;

            //if (param < 0)
            //{
            //    xx = Vector21.x;
            //    yy = Vector21.Y;
            //}
            //else if (param > 1)
            //{
            //    xx = Vector22.x;
            //    yy = Vector22.Y;
            //}
            //else
            //{
            //    xx = Vector21.x + param * C;
            //    yy = Vector21.Y + param * D;
            //}

            //Double d = DistanceBetweenOn2DPlane(Vector2, new Vector2(xx, yy));
        }
    }
}
