using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureExporters
{
    public interface IExporter
    {
        void ExportStructure(Structure structure);
        void ExportStructures();
        void ExportStructures(StructureSet structureSet);
        void Save(string path);
    }
}
