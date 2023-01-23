using System;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;

using StructureExceptionClasses;

namespace EsapiUtilitiesClasses
{
    public class EsapiUtilities
    {

        #region Static Variables

        private static readonly StringBuilder messages = new StringBuilder();

        #endregion

        #region Static Properties

        public static string Messages => messages.ToString();

        #endregion

        #region AddStructure

        /// <summary>
        /// Adds a structure if it's possible to do so.
        ///
        /// </summary>
        /// <param name="SS">The structureset to which the structure will be added to.</param>
        /// <param name="structureType">The string of the type of the structure ot be created.</param>
        /// <param name="structureID">The name of the new structure.</param>
        /// <param name="overwrite" default="false">'true' if it's okay to overwrite the structure.</param>
        /// <param name="exceptionOnNull" default="false">If method should throw an exception when the resulting structure is null, set to 'true'.</param>
        /// <returns>The requested structure.</returns>
        /// 
        public static Structure AddStructure(
            StructureSet SS,
            string structureType,
            string structureID,
            bool overwrite = false,
            bool exceptionOnNull = false)
        {
            _ = messages.Clear();
            //  Check the inputs
            int maxIdLength = 16;
            if (structureID == "")
            {
                throw new ArgumentException($"Structure ID is an empty string!", "structureID");
            }
            else if (structureID.Length > maxIdLength)
            {
                throw new ArgumentException($"Structure ID, {structureID} is too long (length = {structureID.Length}; max = {maxIdLength})!", "structureID");
            }

            if (structureType == "")
            {
                throw new ArgumentException($"Structure type is an empty string!", "structureType");
            }

            try
            {
                Structure structure = SS.Structures.FirstOrDefault(x => x.Id.ToLower() == structureID.ToLower());
                if (structure != null)
                {
                    if (overwrite)
                    {
                        return structure;
                    }
                    else
                    {
                        StructureException exc = new StructureException($"{structureID} exists and can't be overwritten!", structureID);
                        exc.Data.Add("structureSet", SS.Id);
                        exc.Data.Add("structureType", structureType);
                        exc.Data.Add("structureID", structureID);
                        exc.Data.Add("overwrite", overwrite);
                        exc.Data.Add("exceptionOnNull", exceptionOnNull);
                        throw exc;
                    }
                }
                else
                {
                    try
                    {
                        structure = SS.AddStructure(structureType, structureID);
                        if (exceptionOnNull && structure == null)
                        {
                            StructureException exc = new StructureException(
                                $"Unable to add structure, {structureID} (structure is null)!",
                                structureID);
                            exc.Data.Add("structureSet", SS.Id);
                            exc.Data.Add("structureType", structureType);
                            exc.Data.Add("structureID", structureID);
                            exc.Data.Add("overwrite", overwrite);
                            exc.Data.Add("exceptionOnNull", exceptionOnNull);
                            throw exc;
                        }
                        bool exists = TestStructureExists(SS, structureID);
                        if (!exists)
                        {
                            if (exceptionOnNull)
                            {
                                StructureException exc = new StructureException(
                                    $"Unable to add structure, {structureID} (failed to find it in the structure set)!",
                                    structureID);
                                exc.Data.Add("structureSet", SS.Id);
                                exc.Data.Add("structureType", structureType);
                                exc.Data.Add("structureID", structureID);
                                exc.Data.Add("overwrite", overwrite);
                                exc.Data.Add("exceptionOnNull", exceptionOnNull);
                                throw exc;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        return structure;
                    }
                    catch (Exception ex)
                    {
                        StructureException exc = new StructureException($"Problem adding structure, {structureID}!", structureID, ex);
                        exc.Data.Add("structureSet", SS.Id);
                        exc.Data.Add("structureType", structureType);
                        exc.Data.Add("structureID", structureID);
                        exc.Data.Add("overwrite", overwrite);
                        exc.Data.Add("exceptionOnNull", exceptionOnNull);
                        throw exc;
                    }
                }
            }
            catch (Exception ex)
            {
                StructureException exc = new StructureException($"Problem retrieving/creating structure {structureID}", structureID, ex);
                exc.Data.Add("structureSet", SS.Id);
                exc.Data.Add("structureType", structureType);
                exc.Data.Add("structureID", structureID);
                exc.Data.Add("overwrite", overwrite);
                exc.Data.Add("exceptionOnNull", exceptionOnNull);
                throw exc;
            }
        }
        private static bool TestStructureExists(StructureSet SS, string structureID)
        {
            // checks to see if the structure was truely added - the structure above may not be null - but to make sure the structure
            // was truely added we "search for it". Sometimes if a structure already exists (ID) in a seperate structure set (not the
            // one loaded) but associated with the same image set, it causes issues (structures "can" be added, but they violate naming
            // allowance of having multiple structures with the same ID (Despite not being in the same structure set). To avoid this
            // (this really shouldn't be an issue in clinical settings, but just to be on the safe side) we go through a double check
            // of adding the structures, and searching for it, effectively twice.
            Structure test = SS.Structures.FirstOrDefault(x => x.Id == structureID);
            if (test == null)
            {
                _ = messages.AppendLine($"\n<*> Testing Structure was null when creating {structureID}");
            }
            else
            {
                _ = messages.AppendLine($"\n|Created/added {structureID}|");
            }
            return test != null;
        }

        #endregion

        public static Structure CreateNewStructure(StructureSet SS, string structureType, string structureID, bool overwrite = true, bool exceptionOnNull = true)
        {
            Structure newStructure = SS.Structures.FirstOrDefault(x => x.Id.ToUpper() == structureID.ToUpper());
            if (newStructure != null)
            {
                if (overwrite)
                {
                    _ = messages.AppendLine($"Found {newStructure.Id} in the structure set.");
                    return newStructure;
                }
                else
                {
                    _ = messages.AppendLine($"Found {newStructure.Id} in the structure set, but overwrite not allowed.");
                    return null;
                }
            }

            try
            {
                newStructure = AddStructure(SS, structureType, structureID, overwrite, exceptionOnNull);
                if (newStructure == null)
                {
                    string msg = $"Can't add {structureID}!";
                    _ = messages.AppendLine(msg);
                    if (exceptionOnNull)
                    {
                        StructureException exc = new StructureException(msg, structureID);
                        throw exc;
                    }
                    return null;
                }
                _ = messages.AppendLine($"Added the new structure {structureID}");
            }
            catch (StructureException ex)
            {
                Exception exc = new Exception($"Problem adding {ex.StructureID}!", ex);
                exc.Data.Add("structureSet", SS.Id);
                exc.Data.Add("structureType", structureType);
                exc.Data.Add("structureID", structureID);
                throw exc;
            }
            catch (Exception ex)
            {
                Exception exc = new Exception($"Can't add {structureID}!", ex);
                exc.Data.Add("structureSet", SS.Id);
                exc.Data.Add("structureType", structureType);
                exc.Data.Add("structureID", structureID);
                throw exc;
            }

            return newStructure;
        }

    }
}
