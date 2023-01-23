using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using VMS.TPS.Common.Model.API;

using Esapi;

namespace StructureExporters
{
    public class StructuresToXML : IExporter
    {

        #region Variables

        private readonly PlanSetup plan;
        private readonly XmlDocument document = null;
        private readonly Image image = null;

        #endregion

        #region Constructors

        public StructuresToXML(Series series, PlanSetup plan)
        {
            this.plan = plan;
            document = InitXmlDocument();
            image = series.Images.First(x => x.Id == plan.Id);
        }
        public StructuresToXML(Patient patient, PlanSetup plan)
        {
            this.plan = plan;
            document = InitXmlDocument();

            try
            {
                List<Image> images = null;
                Study study = patient.Studies.First();
                Series series = study.Series.First(x => x.Images.Count() > 0);
                images = series.Images.ToList();
                image = images.First(x => x.Id == plan.Id);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Private Functions

        private static XmlDocument InitXmlDocument()
        {
            XmlDocument document = new XmlDocument();

            XmlNode dec = document.CreateXmlDeclaration("1.0", "utf-8", "yes");
            document.AppendChild(dec);
            XmlNode root = document.CreateElement("StructureSet");
            XmlNode version = document.CreateElement("version");
            root.AppendChild(version);
            version.InnerText = "1";
            document.AppendChild(root);

            return document;
        }
        private XmlNode StructureToXml(EsapiStructure structure)
        {
            XmlNode structureNode = document.CreateElement("Structure");
            XmlAttribute attr = document.CreateAttribute("Id");
            attr.Value = structure.Id;
            _ = structureNode.Attributes.Append(attr);

            try
            {
                foreach (Slice slice in structure)
                {
                    XmlNode sliceNode = document.CreateElement("Slice");
                    _ = structureNode.AppendChild(sliceNode);
                    attr = document.CreateAttribute("z");
                    attr.Value = slice[0][0].z.ToString("0.00");
                    _ = sliceNode.Attributes.Append(attr);
                    attr = document.CreateAttribute("nContours");
                    attr.Value = slice.Count.ToString();
                    _ = sliceNode.Attributes.Append(attr);
                    foreach (Contour contour in slice)
                    {
                        XmlNode contourNode = document.CreateElement("Contour");
                        attr = document.CreateAttribute("nPoints");
                        attr.Value = contour.Count.ToString();
                        contourNode.Attributes.Append(attr);
                        _ = sliceNode.AppendChild(contourNode);
                        contourNode.InnerText = contour.ToString();
                    }
                }
            }
            catch
            {
                throw;
            }

            return structureNode;
        }

        #endregion

        #region Public Functions

        #region Export Structures

        public void ExportStructure(Structure structure)
        {
            EsapiStructure s = new EsapiStructure(structure, plan.StructureSet);
            XmlNode root = document.DocumentElement;
            XmlAttribute attr = document.CreateAttribute("SetID");
            attr.Value = plan.StructureSet.Id;
            root.Attributes.Append(attr);
            try
            {
                _ = root.AppendChild(
                    StructureToXml(s)
                    );
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public void ExportStructures(StructureSet structureSet)
        {
            XmlNode root = document.DocumentElement;

            XmlAttribute attr = document.CreateAttribute("SetID");
            attr.Value = structureSet.Id;
            root.Attributes.Append(attr);
            try
            {
                foreach (Structure s in structureSet.Structures)
                {
                    EsapiStructure structure = new EsapiStructure(s, structureSet);
                    _ = root.AppendChild(
                        StructureToXml(structure)
                        );
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public void ExportStructures()
        {
            ExportStructures(plan.StructureSet);
        }

        #endregion

        public void Save(string path)
        {
            document.Save(path);
        }

        #endregion

    }
}
