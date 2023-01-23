using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.Types;

namespace Esapi
{
    public class Contour : List<VVector>, ICloneable
    {
        public Contour() : base() { }
        public Contour(VVector[] vertices) : this()
        {
            foreach (VVector v in vertices)
            {
                Add(v);
            }
        }
        public double ZLocation => this[0].z;
        public override string ToString()
        {
            return ToString(" | ");
        }
        public string ToString(string delimiter)
        {
            List<string> points = new List<string>();
            foreach (VVector pt in this)
            {
                points.Add($"{pt.x:0.00}, {pt.y:0.00}");
            }
            return string.Join(delimiter, points);
        }

        /// <summary>
        /// Makes a deep copy of the contour as a VVector array
        /// </summary>
        /// <returns>VVector[]</returns>
        public VVector[] CopyToVVectorArray()
        {
            VVector[] vec = new VVector[Count];

            for (int i = 0; i < Count; ++i)
            {
                vec[i] = new VVector(this[i].x, this[i].y, this[i].z);
            }

            return vec;
        }
        /// <summary>
        /// Converts the contour to a VVector array (shallow copy)
        /// </summary>
        /// <returns>VVector[]</returns>
        public VVector[] ToVVectorArray()
        {
            return ToArray();
        }

        public object Clone()
        {
            return new Contour(ToVVectorArray());
        }
    }
}
