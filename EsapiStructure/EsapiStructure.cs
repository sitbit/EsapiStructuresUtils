using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using EsapiUtilitiesClasses;
using StructureExceptionClasses;

namespace Esapi
{
    public class EsapiStructure : List<Slice>, ICloneable
    {

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structure"></param>
        /// <param name="set"></param>
        public EsapiStructure(Structure structure, StructureSet set) : base()
        {
            Structure = structure;
            Parent = set;
            if (Structure.MeshGeometry == null || Parent.Image == null)
            {
                return;
            }
            for (int i = MinSlice; i <= MaxSlice; ++i)
            {
                VVector[][] vectors = structure.GetContoursOnImagePlane(i);
                if (vectors == null || vectors.Length == 0)
                {
                    continue;
                }
                Add(new Slice(vectors));
            }
        }

        #endregion

        #region Private Functions

        private static int GetSliceIndex(double z, StructureSet ss)
        {
            double Z0 = ss.Image.Origin.z;
            double res = ss.Image.ZRes;

            return (int)Math.Floor((z - Z0) / res + 0.5);
        }
        private int GetSliceIndex(double z)
        {
            return GetSliceIndex(z, Parent);
        }
        private void RemoveContourFromStructure(Contour ctr, int sliceIndex)
        {
            Structure.SubtractContourOnImagePlane(ctr.ToVVectorArray(), sliceIndex);
        }
        private void AddContourToStructure(Contour ctr, int sliceIndex)
        {
            Structure.AddContourOnImagePlane(ctr.ToVVectorArray(), sliceIndex);
        }

        #endregion

        #region Properties

        public bool Approved
        {
            get
            {
                StructureApprovalHistoryEntry entry = Structure.ApprovalHistory.Last();
                return entry.ApprovalStatus == StructureApprovalStatus.Approved;
            }
        }
        public Rect3D BoundingBox => Structure.MeshGeometry.Bounds;
        public VVector CenterPoint => Structure.CenterPoint;
        public string DicomType => Structure.DicomType;
        public bool HasSegments => Count > 0;
        public bool HasVolume => Structure.Volume > 0;
        public DateTime HistoryDateTime => Structure.HistoryDateTime;
        public string HistoryUserDisplayName => Structure.HistoryUserDisplayName;
        public string HistoryUserName => Structure.HistoryUserName;
        public string Id => Structure.Id;
        public bool IsEmpty => Structure.IsEmpty;
        public bool IsHighResolution => Structure.IsHighResolution;
        public MeshGeometry3D MeshGeometry => Structure.MeshGeometry;
        public string Name => Structure.Name;
        private StructureSet Parent { get; }
        public int ROINumber => Structure.ROINumber;
        public SegmentVolume SegmentVolume => Structure.SegmentVolume;
        public Structure Structure { get; }
        public double Volume => Structure.Volume;
        public Slice this[double z]
        {
            get
            {
                if (z < MinZ || z >MaxZ)
                {
                    throw new ArgumentOutOfRangeException("z", z, $"'z' ({z}) must be between{MinZ} and {MaxZ}.");
                }
                int index = GetSliceIndex(z) - MinSlice;
                return this[index];
            }
        }
        public double MaxZ { get { return BoundingBox.Z + BoundingBox.SizeZ; } }
        public double MinZ { get { return BoundingBox.Z; } }
        public int MinSlice { get { return GetSliceIndex(MinZ); } }
        public int MaxSlice { get { return GetSliceIndex(MaxZ); } }

        #endregion

        #region Public Functions

        #region Cloning Functions

        /// <summary>
        /// A static function that creates a new structure with the same contours as those in 'source'.
        /// </summary>
        /// <param name="source">The source of the contours to be copied.</param>
        /// <param name="newID">The ID for the new structure.</param>
        /// <param name="destination">The structure set to add the new structure to.</param>
        /// <returns></returns>
        /// <exception cref="StructureException"></exception>
        public static Structure CloneVarianStructure(Structure source, string newID, StructureSet destination)
        {
            if (!destination.CanAddStructure(source.DicomType, newID))
            {
                throw new StructureException($"Can't add structure {newID}.",newID);
            }
            try
            {
                Structure structure = EsapiUtilities.AddStructure(destination, source.DicomType, newID, true, true);
                EsapiStructure esapi = new EsapiStructure(source, destination);
                if (esapi.IsHighResolution)
                {
                    structure.ConvertToHighResolution();
                }
                foreach (Slice slice in esapi)
                {
                    int zindex = GetSliceIndex(slice.ZLocation, destination);// - mindex;
                    foreach (Contour ctr in slice)
                    {
                        structure.AddContourOnImagePlane(ctr.ToVVectorArray(), zindex);
                    }
                }

                return structure;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a new structure with the same contours as those in the
        /// encapsulated structure and add it to the destination structure set.
        /// </summary>
        /// <param name="newID">The ID for the new structure.</param>
        /// <param name="destination">The structure set to add the new structure to.</param>
        /// <returns></returns>
        /// <exception cref="StructureException"></exception>
        public Structure CloneVarianStructure(string newID, StructureSet destination)
        {
            if (!destination.CanAddStructure(Structure.DicomType, newID))
            {
                throw new StructureException($"Can't add structure {newID}.", newID);
            }
            try
            {
                Structure structure = EsapiUtilities.AddStructure(destination, Structure.DicomType, newID, true, true);
                EsapiStructure esapi = new EsapiStructure(Structure, destination);
                if (esapi.IsHighResolution)
                {
                    structure.ConvertToHighResolution();
                }
                foreach (Slice slice in esapi)
                {
                    int zindex = GetSliceIndex(slice.ZLocation, destination);
                    foreach (Contour ctr in slice)
                    {
                        structure.AddContourOnImagePlane(ctr.ToVVectorArray(), zindex);
                    }
                }

                return structure;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a new structure with the same contours as those in the
        /// encapsulated structure and add it to the parent structure set.
        /// </summary>
        /// <param name="newID">The ID for the new structure.</param>
        /// <param name="destination">The structure set to add the new structure to.</param>
        /// <returns></returns>
        /// <exception cref="StructureException"></exception>
        public Structure CloneVarianStructure(string newID)
        {
            try
            {
                return CloneVarianStructure(Structure, newID, Parent);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a copy of the EsapiStructure by creating a clone of the underlying
        /// Varian structure(!) and wrapping it in a new EsapiStructure object.  The new
        /// Varian Structure is added to the destination structure set.
        /// </summary>
        /// <param name="newID">The ID for the new structure.</param>
        /// <param name="destination">The structure set to add the new structure to.</param>
        /// <returns>A deep copy of this EsapiStructure</returns>
        /// <exception cref="StructureException"></exception>
        public EsapiStructure CloneEsapiStructure(string newID, StructureSet destination)
        {
            try
            {
                Structure structure = CloneVarianStructure(newID, destination);
                return new EsapiStructure(structure, destination);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a copy of the EsapiStructure by creating a clone of the underlying
        /// Varian structure(!) and wrapping it in a new EsapiStructure object.  The new
        /// Varian Structure is added to the structure set containing the encapsulated
        /// Structure!
        /// </summary>
        /// <param name="newID">The ID for the new structure.</param>
        /// <returns>A deep copy of this EsapiStructure</returns>
        /// <exception cref="StructureException"></exception>
        public EsapiStructure CloneEsapiStructure(string newID)
        {
            try
            {
                Structure structure = CloneVarianStructure(newID, Parent);
                return new EsapiStructure(structure, Parent);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a copy of the EsapiStructure by creating a clone of the underlying
        /// Varian structure(!) and wrapping it in a new EsapiStructure object.  The new
        /// Varian Structure is added to the structure set containing the encapsulated
        /// Structure with the new Id = {Id}_clone!
        /// </summary>
        /// <returns>A deep copy of this EsapiStructure</returns>
        /// <exception cref="StructureException"></exception>
        public object Clone()
        {
            try
            {
                string id = $"{Id}_clone";
                return CloneEsapiStructure(id);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        public void AddContourToSlice(Contour ctr, int sliceIndex)
        {
            this[sliceIndex].Add(ctr);
            AddContourToStructure(ctr, sliceIndex);
        }
        public void AddContourToSlice(Contour ctr, double z)
        {
            int sliceIndex = GetSliceIndex(z);
            AddContourToSlice(ctr, sliceIndex);
        }
        public Contour GetContour(int sliceIndex, int contourIndex)
        {
            Slice slice = this[sliceIndex];
            if (contourIndex >= 0 && contourIndex < slice.Count)
            {
                return slice[contourIndex];
            }
            return null;
        }
        public Contour GetContour(double z, int contourIndex)
        {
            int sliceIndex = GetSliceIndex(z) - MinSlice;
            return GetContour(sliceIndex, contourIndex);
        }
        public Contour RemoveContourFromSlice(int sliceIndex, int contourIndex)
        {
            Slice slice = this[sliceIndex];
            Contour ctr = slice[contourIndex];
            slice.Remove(ctr);
            RemoveContourFromStructure(ctr, sliceIndex + MinSlice);

            return ctr;
        }
        public Contour RemoveContourFromSlice(double z, int contourIndex)
        {
            int sliceIndex = GetSliceIndex(z);
            return RemoveContourFromSlice(sliceIndex, contourIndex);
        }

        public string ToString(string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            _ = sb.AppendLine($"Structure ID{delimiter}{Structure.Id}");
            foreach (Slice slice in this)
            {
                _ = sb.AppendLine(slice.ToString(delimiter));
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            return ToString(",");
        }

        #endregion

    }
}
