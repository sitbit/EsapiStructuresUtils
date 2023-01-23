using System;
using System.Collections.Generic;
using System.Text;
using VMS.TPS.Common.Model.Types;

namespace Esapi
{
    public class Slice : List<Contour>, ICloneable
    {
        public Slice() : base() { }
        public Slice(VVector[][] slice) : this()
        {
            ZLocation = slice[0][0].z;
            foreach (VVector[] ctr in slice)
            {
                Add(new Contour(ctr));
            }
        }
        public double ZLocation { get; protected set; }

        public object Clone()
        {
            Slice slice = new Slice();
            slice.ZLocation = ZLocation;

            foreach(Contour contour in this)
            {
                slice.Add((Contour)contour.Clone());
            }

            return slice;
        }
        public string ToString(string delimiter)
        {
            StringBuilder sb = new StringBuilder();

            _ = sb.AppendLine($"Z{delimiter}{ZLocation}");
            _ = sb.AppendLine($"nContours{delimiter}{Count}");
            foreach(Contour ctr in this)
            {
                _ = sb.AppendLine(ctr.ToString(delimiter));
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            return ToString(",");
        }
        public VVector[][] ToVVectorArray()
        {
            VVector[][] vectors = new VVector[Count][];
            int i = 0;
            foreach (Contour contour in this)
            {
                vectors[i] = contour.ToVVectorArray();
                ++i;
            }
            return vectors;
        }
    }
}
