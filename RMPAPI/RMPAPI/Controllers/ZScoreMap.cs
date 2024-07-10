public class ZScoreMap
{
    private Dictionary<double, double> map = new Dictionary<double, double>();

    public ZScoreMap()
    {
        map.Add(80.0, 1.282);
        map.Add(85.0, 1.440);
        map.Add(90.0, 1.645);
        map.Add(95.0, 1.960);
        map.Add(99.0, 2.576);
        map.Add(99.5, 2.807);
        map.Add(99.9, 3.291);
    }

    public double GetZScore(double confidenceInterval)
    {
        if (map.TryGetValue(confidenceInterval, out double zScore))
        {
            return zScore;
        }
        else
        {
            throw new ArgumentException("Invalid confidence interval provided.");
        }
    }
}
