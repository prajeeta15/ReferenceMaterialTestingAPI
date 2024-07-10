using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace RMPAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class STSController : ControllerBase
    {
        private readonly ZScoreMap zScoreMap = new ZScoreMap();

        [HttpPost("ShortTermStability")]
        public async Task<IActionResult> Analyze(IFormFile jsonfile, [FromForm] double stsConfidenceInterval)
        {
            try
            {
                if (jsonfile == null || jsonfile.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var zScore = zScoreMap.GetZScore(stsConfidenceInterval);

                // Read the uploaded file
                using var reader = new StreamReader(jsonfile.OpenReadStream());
                string json = await reader.ReadToEndAsync();

                // Deserialize JSON data
                var regjson = JsonConvert.DeserializeObject<List<STSData>>(json);

                if (regjson == null || !regjson.Any())
                {
                    return BadRequest("Invalid or empty data in the uploaded file");
                }
                if (regjson.Count < 16 || regjson.Count(item => item.Density != 0) < 16 || regjson.Any(item => item.Week > 4))
                {
                    return BadRequest("Insufficient Data (There must be at least 16 data points for Density and Week values should not exceed 4).");
                }

                // Perform Linear Regression
                var periods = regjson.Select(d => (double)d.Week).ToArray();
                var densities = regjson.Select(d => d.Density).ToArray();
                var regression = SimpleRegression.Fit(periods, densities);
                var predictions = periods.Select(x => regression.Item1 + regression.Item2 * x).ToArray();
                var rSquare = GoodnessOfFit.RSquared(predictions, densities);

                var avgDensity = densities.Average();

                // Check the valid observations
                var observations = densities.Length;
                if (observations < 4)
                {
                    return BadRequest("Insufficient data points for regression analysis (minimum 3 required).");
                }

                // Calculate other statistics
                var mean = densities.Average();
                var sumOfSquares = densities.Sum(d => Math.Pow(d - mean, 2));
                var sumOfResiduals = densities.Zip(predictions, (d, p) => d - p).Sum(r => r * r);
                var standardError = Math.Sqrt(sumOfResiduals / (observations - 2));
                var adjustedRSquare = 1 - ((1 - rSquare) * (observations - 1) / (observations - 2));
                var multipleR = Math.Sqrt(rSquare);

                var regressionStatistics = new
                {
                    MultipleR = multipleR,
                    RSquare = rSquare,
                    AdjustedRSquare = adjustedRSquare,
                    StandardError = standardError,
                    Observations = observations
                };

                // Perform ANOVA
                var ssTotal = sumOfSquares;
                var ssRegression = predictions.Sum(p => Math.Pow(p - mean, 2));
                var ssResidual = sumOfResiduals;
                var dfTotal = observations - 1;
                var dfRegression = 1;
                var dfResidual = observations - 2;

                var msRegression = ssRegression / dfRegression;
                var msResidual = ssResidual / dfResidual;
                var fValue = msRegression / msResidual;
                var significanceF = 1 - MathNet.Numerics.Distributions.FisherSnedecor.CDF(dfRegression, dfResidual, fValue);

                var anovaStatistics = new
                {
                    DfRegression = dfRegression,
                    DfResidual = dfResidual,
                    DfTotal = dfTotal,
                    SsRegression = ssRegression,
                    SsResidual = ssResidual,
                    MsRegression = msRegression,
                    MsResidual = msResidual,
                    FValue = fValue,
                    SignificanceF = significanceF
                };

                // Calculate coefficients and other statistics
                var intercept = regression.Item1;
                var slope = regression.Item2;

                // Calculate standard error of intercept and slope
                var seIntercept = Math.Sqrt(standardError * standardError * periods.Sum(x => x * x) / (observations * periods.Sum(x => x * x) - Math.Pow(periods.Sum(), 2)));
                var seSlope = Math.Sqrt(standardError * standardError * observations / (observations * periods.Sum(x => x * x) - Math.Pow(periods.Sum(), 2)));

                var tStatIntercept = intercept / seIntercept;
                var tStatSlope = slope / seSlope;
                var pValueIntercept = 2 * (1 - MathNet.Numerics.Distributions.StudentT.CDF(0, 1, dfResidual, Math.Abs(tStatIntercept)));
                var pValueSlope = 2 * (1 - MathNet.Numerics.Distributions.StudentT.CDF(0, 1, dfResidual, Math.Abs(tStatSlope)));

                // Calculating Margin of Error
                var marginOfErrorIntercept = zScore * seIntercept;
                var marginOfErrorSlope = zScore * seSlope;

                // Calculate Confidence Intervals
                var lowerConfidenceIntercept = intercept - marginOfErrorIntercept;
                var upperConfidenceIntercept = intercept + marginOfErrorIntercept;
                var lowerConfidenceSlope = slope - marginOfErrorSlope;
                var upperConfidenceSlope = slope + marginOfErrorSlope;

                // Calculating Relative STS%
                int[] months = { 1, 2, 6, 12, 24 };
                double[] deltaValues = new double[months.Length];
                double[] U_deltaValues = new double[months.Length];
                double[] U_LTS = new double[months.Length];

                for (int i = 0; i < months.Length; i++)
                {
                    double delta = slope * months[i];
                    deltaValues[i] = delta;

                    double deltaU = seSlope * months[i];
                    U_deltaValues[i] = deltaU;

                    double U_LTS_value = Math.Sqrt(delta * delta + deltaU * deltaU);
                    U_LTS[i] = U_LTS_value;
                }

                var ULTS24 = U_LTS[4];
                var RelativeSTS = Math.Round((ULTS24 * 100) / avgDensity, 2);

                var analysis = new
                {
                    Intercept = intercept,
                    Slope = slope,
                    StandardErrorIntercept = seIntercept,
                    StandardErrorSlope = seSlope,
                    TStatIntercept = tStatIntercept,
                    TStatSlope = tStatSlope,
                    PValueIntercept = pValueIntercept,
                    PValueSlope = pValueSlope,
                    LowerConfidenceIntercept = lowerConfidenceIntercept,
                    UpperConfidenceIntercept = upperConfidenceIntercept,
                    LowerConfidenceSlope = lowerConfidenceSlope,
                    UpperConfidenceSlope = upperConfidenceSlope
                };

                var pValueSignificance = pValueSlope < 0.05 ? "Since P-value is less than 0.05 there is a significant trend." : "Since P-value is greater than 0.05 there is no significant trend.";

                var result = new
                {
                    RegressionStatistics = regressionStatistics,
                    AnovaStatistics = anovaStatistics,
                    Analysis = analysis,
                    PValueSignificance = pValueSignificance,
                    DeltaValues = deltaValues,
                    U_DeltaValues = U_deltaValues,
                    ULTS = U_LTS,
                    Relative_STS_percentage = RelativeSTS
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
