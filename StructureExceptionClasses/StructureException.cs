using System;

namespace StructureExceptionClasses
{
    public class StructureException : Exception
    {
        public string StructureID { get; protected set; }
        public StructureException(string message, string structureID) : base(message)
        {
            StructureID = structureID;
        }
        public StructureException(string message, string structureID, Exception innerException) : base(message, innerException)
        {
            StructureID = structureID;
        }

        //   Commented out unused constructors
        //public StructureException() : base() { }
        //public StructureException(string message) : base(message) { }
        //public StructureException(string message, Exception innerException) : base(message, innerException) { }
        //public StructureException(string message, string structureID, Exception innerException) : base(message, innerException)
        //{
        //     StructureID = structureID;
        //}
    }
}
