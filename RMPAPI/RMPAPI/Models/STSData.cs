namespace RMPAPI.Models
{
    public class STSData
    {
        public int Week { get; set; }
        public double Density { get; set; }
    }

    public class STSRegressionStatistics
    {
        public double MultipleR { get; set; }
        public double RSquare { get; set; }
        public double AdjustedRSquare { get; set; }
        public double StandardError { get; set; }
        public int Observations { get; set; }
    }

    public class STSAnovaStatistics
    {
        public int DfRegression { get; set; }
        public int DfResidual { get; set; }
        public int DfTotal { get; set; }
        public double SsRegression { get; set; }
        public double SsResidual { get; set; }
        public double MsRegression { get; set; }
        public double MsResidual { get; set; }
        public double FValue { get; set; }
        public double SignificanceF { get; set; }
    }

    public class STSAnalysis
    {
        public double Intercept { get; set; }
        public double Slope { get; set; }
        public double StandardErrorIntercept { get; set; }
        public double StandardErrorSlope { get; set; }
        public double TStatIntercept { get; set; }
        public double TStatSlope { get; set; }
        public double PValueIntercept { get; set; }
        public double PValueSlope { get; set; }
        public double LowerConfidenceIntercept { get; set; }
        public double UpperConfidenceIntercept { get; set; }
        public double LowerConfidenceSlope { get; set; }
        public double UpperConfidenceSlope { get; set; }
    }
}
