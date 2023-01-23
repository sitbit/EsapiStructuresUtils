using Esapi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureExporters
{
    public class StructuresToCSV// : IExporter
    {
        private readonly PlanSetup plan;
        private readonly Image image = null;
        private readonly StringBuilder sb = new StringBuilder();

        #region Constructors

        public StructuresToCSV(Series series, PlanSetup plan)
        {
            this.plan = plan;
            image = series.Images.First(x => x.Id == plan.Id);
        }
        public StructuresToCSV(Patient patient, PlanSetup plan)
        {
            this.plan = plan;
            try
            {
                //List<Image> images = null;
                Study study = patient.Studies.First();
                foreach (Series series in study.Series)
                {
                    if (series.Images.Count() > 0)
                    {
                        image = series.Images.First(x => x.Id == plan.Id);
                        break;
                    }
                }
                //image = images.First(x => x.Id == plan.Id);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        public void ExportStructure(Structure structure)
        {
            EsapiStructure s = new EsapiStructure(structure, plan.StructureSet);
            sb.AppendLine(s.ToString());
        }

        public void ExportStructures()
        {
            ExportStructures(plan.StructureSet);
        }
        public void ExportStructures(StructureSet structureSet)
        {
            sb.AppendLine($"Set ID,{structureSet.Id}");
            foreach (Structure structure in structureSet.Structures)
            {
                ExportStructure(structure);
            }
        }

        public void Save(string path)
        {
            StreamWriter writer = null;
            try
            {
                Stream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new StreamWriter(stream);
                writer.WriteLine(sb.ToString());
            }
            catch
            {
                throw;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }
    }
}
