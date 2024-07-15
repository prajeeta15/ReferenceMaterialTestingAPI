public class HomogeneityData
{
    public double Attribute1 { get; set; }
    public double Attribute2 { get; set; }
    public double Attribute3 { get; set; }
}

public class StandardDeviationTable
{
    public double StandardDeviationA1 { get; set; }
    public double StandardDeviationA2 { get; set; }
    public double StandardDeviationA3 { get; set; }
    public double LowerLimitA1 { get; set; }
    public double UpperLimitA1 { get; set; }
    public double LowerLimitA2 { get; set; }
    public double UpperLimitA2 { get; set; }
    public double LowerLimitA3 { get; set; }
    public double UpperLimitA3 { get; set; }
}

public class ResultsTable
{
    public List<double> SumOfRows { get; set; }
    public List<double> SumOfColumns { get; set; }
    public List<double> MeanOfRows { get; set; }
    public List<double> MeanOfColumns { get; set; }
    public List<double> VarianceOfRows { get; set; }
    public List<double> VarianceOfColumns { get; set; }
}

public class ANOVA
{
    public List<double[]> Deviations { get; set; }
    public double SSB { get; set; }
    public double SSC { get; set; }
    public double SSW { get; set; }
    public double SSE { get; set; }
    public int DegreeFreedomRow { get; set; }
    public int DegreeFreedomColumn { get; set; }
    public int DegreeFreedomWithin { get; set; }
    public double MSB { get; set; }
    public double MSC { get; set; }
    public double MSW { get; set; }
    public double MSE { get; set; }
    public double FRows { get; set; }
    public double FColumns { get; set; }
    public double PValueRows { get; set; }
    public double PValueColumns { get; set; }
    public double FCritRows { get; set; }
    public double FCritColumns { get; set; }
    public double FValue { get; set; }
    public double FCriticalAt95 { get; set; }
    public string HomogeneityTest { get; set; }
}

public class UHomogeneityTable
{
    public double U2bb { get; set; }
    public double RelativeHomogeneity { get; set; }
    public double Ubb1 { get; set; }
    public double Ubb2 { get; set; }
    public double SWithin { get; set; }
    public double UWithin { get; set; }
}

public class HomogeneityTestingResult
{
    public StandardDeviationTable StandardDeviationTable { get; set; }
    public List<HomogeneityData> Outliers { get; set; }
    public List<HomogeneityData> DataWithoutOutliers { get; set; }
    public ResultsTable ResultsTable { get; set; }
    public ANOVA ANOVA { get; set; }
    public UHomogeneityTable UHomogeneityTable { get; set; }
    public double Uhom1 { get; set; }
    public double Uhom2 { get; set; }
}
