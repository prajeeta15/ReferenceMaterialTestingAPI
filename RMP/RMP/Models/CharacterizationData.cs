public class CharacterizationData
{
    public string LabName { get; set; }
    public string LabID { get; set; }
    public double Result1 { get; set; }
    public double Result2 { get; set; }
    public double Result3 { get; set; }
}

public class LabResult
{
    public string LabName { get; set; }
    public string LabID { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double Median { get; set; }
    public double AbsoluteValue { get; set; }
}

public class ResultSummary
{
    public double MedianOfMedians { get; set; }
    public double MAD { get; set; }
    public double MADe { get; set; }
    public double UcharMgL { get; set; }
    public double UcharPPM { get; set; }
    public List<LabResult> LabResults { get; set; }

}
