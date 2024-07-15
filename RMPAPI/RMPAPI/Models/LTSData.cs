public class LTSData
{
    public int Month { get; set; }
    public double Density { get; set; }
}
public class RegressionStatistics
{
    public double MultipleR { get; set; }
    public double RSquare { get; set; }
    public double AdjustedRSquare { get; set; }
    public double StandardError { get; set; }
    public int Observations { get; set; }
}

public class AnovaStatistics
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

public class Analysis
{
    public double Intercept { get; set; }
    public double Slope { get; set; }
    public double StandardErrorIntercept { get; set; }
    public double StandardErrorSlope { get; set; }
    public double TStatIntercept { get; set; }
    public double TStatSlope { get; set; }
    public double PValueIntercept { get; set; }
    public double PValueSlope { get; set; }
    public double Lower95Intercept { get; set; }
    public double Upper95Intercept { get; set; }
    public double Lower95Slope { get; set; }
    public double Upper95Slope { get; set; }
}

public class LTSResult
{
    public RegressionStatistics LTSRegressionStatistics { get; set; }
    public AnovaStatistics LTSAnovaStatistics { get; set; }
    public Analysis LTSAnalysis { get; set; }
    public string LTSPValueSignificance { get; set; }
    public double[] ULTS { get; set; }
    public double RelativeLTSPercentage { get; set; }
}
