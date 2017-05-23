using System;
// ReSharper disable InconsistentNaming

namespace Fabric.FHIR.DataModels
{
    public class Patient
    {
        public string EDWPatientID { get; set; }
        public string gender { get; set; }
        public DateTime birthDate { get; set; }

        public Identifier[] identifier { get; set; }

        public Name[] name { get; set; }

        public Condition[] condition { get; set; }
    }
}