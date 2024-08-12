using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace RMPAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomogeneityTestingController : ControllerBase
    {
        [HttpPost("HomogeneityTesting")]
        public async Task<IActionResult> HomogeneityTesting(IFormFile jsonfile, [FromForm] double confidenceLevel, double umethod)
        {
            try
            {
                if (jsonfile == null || jsonfile.Length == 0)
                {
                    return BadRequest("Invalid file found.");
                }

                using var stream = new StreamReader(jsonfile.OpenReadStream());
                string json = await stream.ReadToEndAsync();

                var data = JsonConvert.DeserializeObject<List<HomogeneityData>>(json);

                if (data == null || !data.Any())
                {
                    return BadRequest("No data available in the JSON file.");
                }

                if (data.Count < 10 || data.Count(item => item.Attribute1 != 0) < 10 || data.Count(item => item.Attribute2 != 0) < 10 || data.Count(item => item.Attribute3 != 0) < 10)
                {
                    return BadRequest("Insufficient Data (There must be at least 10 data points for each attribute to proceed).");
                }

                var meanA1 = data.Average(item => item.Attribute1);
                var meanA2 = data.Average(item => item.Attribute2);
                var meanA3 = data.Average(item => item.Attribute3);

                var stdDevA1 = CalculateStandardDeviation(data.Select(item => item.Attribute1).ToList());
                var stdDevA2 = CalculateStandardDeviation(data.Select(item => item.Attribute2).ToList());
                var stdDevA3 = CalculateStandardDeviation(data.Select(item => item.Attribute3).ToList());

                var lowerLimitA1 = meanA1 - 2 * stdDevA1;
                var upperLimitA1 = meanA1 + 2 * stdDevA1;

                var lowerLimitA2 = meanA2 - 2 * stdDevA2;
                var upperLimitA2 = meanA2 + 2 * stdDevA2;

                var lowerLimitA3 = meanA3 - 2 * stdDevA3;
                var upperLimitA3 = meanA3 + 2 * stdDevA3;

                var outliers = data.Where(item =>
                    item.Attribute1 < lowerLimitA1 || item.Attribute1 > upperLimitA1 ||
                    item.Attribute2 < lowerLimitA2 || item.Attribute2 > upperLimitA2 ||
                    item.Attribute3 < lowerLimitA3 || item.Attribute3 > upperLimitA3
                ).ToList();

                var dataWithoutOutliers = data.Except(outliers).ToList();

                var dataTable = ToDataTable(dataWithoutOutliers);

                var sumOfRows = dataTable.AsEnumerable().Select(row => row.ItemArray.Sum(item => Convert.ToDouble(item))).ToList();
                var sumOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col => dataTable.AsEnumerable().Sum(row => Convert.ToDouble(row[col]))).ToList();

                var meanOfRows = dataTable.AsEnumerable().Select(row => row.ItemArray.Average(item => Convert.ToDouble(item))).ToList();
                var meanOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col => dataTable.AsEnumerable().Average(row => Convert.ToDouble(row[col]))).ToList();

                var varianceOfRows = dataTable.AsEnumerable().Select(row =>
                {
                    var values = row.ItemArray.Select(item => Convert.ToDouble(item)).ToList();
                    return CalculateVariance(values);
                }).ToList();

                var varianceOfColumns = dataTable.Columns.Cast<DataColumn>().Select(col =>
                {
                    var values = dataTable.AsEnumerable().Select(row => Convert.ToDouble(row[col])).ToList();
                    return CalculateVariance(values);
                }).ToList();

                var grandMean = dataTable.AsEnumerable().SelectMany(row => row.ItemArray.Select(item => Convert.ToDouble(item))).Average();

                var SSB = dataTable.Columns.Count * meanOfRows.Sum(mean => Math.Pow(mean - grandMean, 2));

                var deviations = dataTable.AsEnumerable().Select(row => row.ItemArray.Select((item, index) => Convert.ToDouble(item) - row.ItemArray.Average(cell => Convert.ToDouble(cell))).ToArray()).ToList();
                double SSW = deviations.Select(row => row.Select(deviation => Math.Pow(deviation, 2)).Sum()).Sum();

                var dfRows = dataTable.Rows.Count - 1;
                var dfColumns = dataTable.Columns.Count - 1;
                var dfWithinRows = dfRows * dfColumns;

                var MSB = SSB / dfRows;
                var MSW = SSW / dfWithinRows;

                var SSC = dataTable.Rows.Count * meanOfColumns.Sum(mean => Math.Pow(mean - grandMean, 2));
                var SSE = SSW - SSC;
                var MSE = SSE / dfWithinRows;
                var MSC = SSC / dfColumns;

                var FRows = MSB / MSE;
                var FColumns = MSC / MSE;

                var pValueRows = 1 - FisherSnedecor.CDF(dfRows, dfWithinRows, FRows);
                var pValueColumns = 1 - FisherSnedecor.CDF(dfColumns, dfWithinRows, FColumns);


                var degreeFreedomRow = dfRows;
                var degreeFreedomColumn = dfColumns;
                var degreeFreedomWithin = dfWithinRows;
                var dof = (dfWithinRows + dfColumns);

                double levelOfSignificance = MapConfidenceToSignificance(confidenceLevel);

                var fCritAt95 = FisherSnedecor.InvCDF(dfRows, dfWithinRows+dfColumns, 1 - levelOfSignificance);
                var fCritRows = FisherSnedecor.InvCDF(dfRows, dfWithinRows, 1 - levelOfSignificance);
                var fCritColumns = FisherSnedecor.InvCDF(dfColumns, dfWithinRows, 1 - levelOfSignificance);

                var FValue = MSB / MSE;
                var homogeneityTest = (FValue > fCritAt95) ? "FAIL HOMOGENEITY TEST" : "PASS HOMOGENEITY TEST";

                var numberOfReplicates = 3.0; 
                var uHomogeneity = Math.Max((MSB - MSE) / numberOfReplicates, 0);
                var U2bb = uHomogeneity;
                var RelativeHomogeneity = uHomogeneity / stdDevA1 * 100;
                var Ubb1 = Math.Sqrt(uHomogeneity);
                var ubb2 = (Math.Sqrt(MSE / numberOfReplicates)) * Math.Pow(2 / dfWithinRows, 0.25);
                var SWithin = Math.Sqrt(MSE);
                var UWithin = SWithin / Math.Sqrt(numberOfReplicates); ;

                var Uhom1 = Math.Sqrt(Math.Pow(Ubb1, 2) + Math.Pow(UWithin, 2));
                var Uhom2 = Math.Sqrt(Math.Pow(Ubb1, 2) + Math.Pow(UWithin, 2) + Math.Pow(umethod, 2)); 

                var result = new HomogeneityTestingResult
                {
                    StandardDeviationTable = new StandardDeviationTable
                    {
                        StandardDeviationA1 = stdDevA1,
                        StandardDeviationA2 = stdDevA2,
                        StandardDeviationA3 = stdDevA3,
                        LowerLimitA1 = lowerLimitA1,
                        UpperLimitA1 = upperLimitA1,
                        LowerLimitA2 = lowerLimitA2,
                        UpperLimitA2 = upperLimitA2,
                        LowerLimitA3 = lowerLimitA3,
                        UpperLimitA3 = upperLimitA3
                    },
                    Outliers = outliers,
                    DataWithoutOutliers = dataWithoutOutliers,
                    ResultsTable = new ResultsTable
                    {
                        SumOfRows = sumOfRows,
                        SumOfColumns = sumOfColumns,
                        MeanOfRows = meanOfRows,
                        MeanOfColumns = meanOfColumns,
                        VarianceOfRows = varianceOfRows,
                        VarianceOfColumns = varianceOfColumns
                    },
                    ANOVA = new ANOVA
                    {
                        Deviations = deviations,
                        SSB = SSB,
                        SSC = SSC,
                        SSW = SSW,
                        SSE = SSE,
                        DegreeFreedomRow = degreeFreedomRow,
                        DegreeFreedomColumn = degreeFreedomColumn,
                        DegreeFreedomWithin = degreeFreedomWithin,
                        MSB = MSB,
                        MSC = MSC,
                        MSW = MSW,
                        MSE = MSE,
                        FRows = FRows,
                        FColumns = FColumns,
                        PValueRows = pValueRows,
                        PValueColumns = pValueColumns,
                        FCritRows = fCritRows,
                        FCritColumns = fCritColumns,
                        FValue = FValue,
                        FCriticalAt95 = fCritAt95,
                        HomogeneityTest = homogeneityTest
                    },
                    UHomogeneityTable = new UHomogeneityTable
                    {
                        U2bb = U2bb,
                        RelativeHomogeneity = RelativeHomogeneity,
                        Ubb1 = Ubb1,
                        Ubb2 = ubb2,
                        SWithin = SWithin,
                        UWithin = UWithin
                    },
                    Uhom1 = Uhom1,
                    Uhom2 = Uhom2
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private static double CalculateStandardDeviation(List<double> values)
        {
            var avg = values.Average();
            return Math.Sqrt(values.Sum(v => Math.Pow(v - avg, 2)) / (values.Count - 1));
        }

        private static double CalculateVariance(List<double> values)
        {
            var avg = values.Average();
            return values.Sum(v => Math.Pow(v - avg, 2)) / (values.Count - 1);
        }

        private static double FDistribution(double f, double d1, double d2)
        {
            return 1.0 - FisherSnedecor.CDF(d1, d2, f);
        }

        private static double FDistributionCriticalValue(double p, double d1, double d2)
        {
            return 1.0 - FisherSnedecor.InvCDF(d1, d2, p);
        }

        private static double MapConfidenceToSignificance(double confidenceLevel)
        {
            return confidenceLevel switch
            {
                60.0 => 0.40,
                70.0 => 0.30,
                80.0 => 0.20,
                85.0 => 0.15,
                90.0 => 0.10,
                95.0 => 0.05,
                98.0 => 0.02,
                99.0 => 0.01,
                99.8 => 0.002,
                99.9 => 0.001,
                _ => throw new ArgumentException("Invalid confidence level")
            };
        }

        private static DataTable ToDataTable<T>(IList<T> data)
        {
            var properties = typeof(T).GetProperties();
            var dataTable = new DataTable();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in data)
            {
                var values = new object[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}
