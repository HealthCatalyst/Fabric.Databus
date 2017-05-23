// ReSharper disable InconsistentNaming
namespace Fabric.FHIR.DataModels
{
    public class Condition
    {
        public string clinicalStatus { get; set; }
        public string verificationStatus { get; set; }

        public ConditionCategory[] category { get; set; }

        public ConditionCode[] code { get; set; }
    }

    public class ConditionCode  
    {
        public string coding { get; set; }
        public string text { get; set; }
    }

    public class ConditionCategory
    {
        public string system { get; set; }
        public string code { get; set; }
        public string text { get; set; }
    }
}