using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace RMPAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomogeneityTestingController : ControllerBase
    {
        // POST api/homogeneitytesting/homogeneitytesting
        [HttpPost("HomogeneityTesting")]
        public async Task<IActionResult> HomogeneityTesting(IFormFile jsonfile, [FromForm] double probability, double umethod)
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
                var dCol = dataTable.Columns.Count - 1;
                var dfWithinRows = dfRows * dCol;

                var MSB = SSB / dfRows;
                var MSW = SSW / dfWithinRows;

                var SSC = dataTable.Rows.Count * meanOfColumns.Sum(mean => Math.Pow(mean - grandMean, 2));
                var SSE = SSW - SSC;
                var MSE = SSE / dfWithinRows;
                var MSC = SSC / dCol;

                var FRows = MSB / MSE;
                var FColumns = MSC / MSE;

                var pValueRows = 1 - FDistribution(FRows, dfRows, dfWithinRows);
                var pValueColumns = 1 - FDistribution(FColumns, dCol, dfWithinRows);

                probability = probability / 100;
                var degree_freedom_row = dfRows;
                var degree_freedom_column = dCol;
                var degree_freedom_within = dfWithinRows;
                var dof = (dfWithinRows + dCol);


                var fCritRows = FDistributionCriticalValue(1 - probability, dfRows, dfWithinRows);
                var fCritColumns = FDistributionCriticalValue(1 - probability, dataTable.Columns.Count - 1, dfWithinRows);

                var fCritat95 = FisherSnedecor.InvCDF(dfRows, dof, 1 - probability);
                var F_value = MSB / MSE;
                var homogeneityTest = (F_value > fCritat95) ? "FAIL HOMOGENEITY TEST" : "PASS HOMOGENEITY TEST";

                var U2bb = Math.Max(0, (MSB - MSE) / (dataTable.Columns.Count));

                var Ubb1 = Math.Sqrt(U2bb);

                var RelativeHomogeneity = (Ubb1 / grandMean) * 100;

                var ubb2 = Math.Sqrt((MSE / (dataTable.Columns.Count)) * Math.Pow(2 / degree_freedom_within, 0.25));

                var SWithin = Math.Sqrt(MSE);

                var UWithin = SWithin / Math.Sqrt(dataTable.Columns.Count);

                var Uhom1 = CalculateUhom1(Ubb1, UWithin);
                var Uhom2 = CalculateUhom2(Ubb1, UWithin, umethod);

                var resultsTable = new
                {
                    SumOfRows = sumOfRows,
                    SumOfColumns = sumOfColumns,
                    MeanOfRows = meanOfRows,
                    MeanOfColumns = meanOfColumns,
                    VarianceOfRows = varianceOfRows,
                    VarianceOfColumns = varianceOfColumns
                };

                var result = new
                {
                    StandardDeviationTable = new
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
                    ResultsTable = resultsTable,
                    ANOVA = new
                    {
                        deviations,
                        SSB,
                        SSC,
                        SSW,
                        SSE,
                        degree_freedom_row,
                        degree_freedom_column,
                        degree_freedom_within,
                        MSB,
                        MSC,
                        MSW,
                        MSE,
                        FRows,
                        FColumns,
                        pValueRows,
                        pValueColumns,
                        fCritRows,
                        fCritColumns,
                        Fvalue = F_value,
                        FcriticalAt95 = fCritat95,
                        HomogeneityTest = homogeneityTest
                    },
                    AdditionalCalculations = new
                    {
                        U2bb = U2bb,
                        RelativeHomogeneity = RelativeHomogeneity,
                        Ubb1 = Ubb1,
                        ubb2 = ubb2,
                        SWithin = SWithin,
                        UWithin = UWithin
                    },
                    Uhom1 = new
                    {
                        Uhom1
                    },
                    Uhom2 = new
                    {
                        Uhom2
                    }
                };

                return Ok(result);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            var mean = values.Average();
            var variance = values.Average(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(variance);
        }

        private double CalculateVariance(List<double> values)
        {
            var mean = values.Average();
            return values.Average(v => Math.Pow(v - mean, 2));
        }

        private DataTable ToDataTable(List<HomogeneityData> data)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Attribute1", typeof(double));
            dataTable.Columns.Add("Attribute2", typeof(double));
            dataTable.Columns.Add("Attribute3", typeof(double));

            foreach (var item in data)
            {
                dataTable.Rows.Add(item.Attribute1, item.Attribute2, item.Attribute3);
            }

            return dataTable;
        }

        private double FDistribution(double value, int dfn, int dfd)
        {
            return new FisherSnedecor(dfn, dfd).CumulativeDistribution(value);
        }

        private double FDistributionCriticalValue(double probability, int dfn, int dfd)
        {
            return new FisherSnedecor(dfn, dfd).InverseCumulativeDistribution(probability);
        }

        private double CalculateUhom1(double Ubb1, double UWithin)
        {
            return Math.Sqrt(Math.Pow(Ubb1, 2) + Math.Pow(UWithin, 2));
        }

        private double CalculateUhom2(double Ubb1, double UWithin, double umethod)
        {
            return Math.Sqrt(Math.Pow(Ubb1, 2) + Math.Pow(UWithin, 2) + Math.Pow(umethod, 2));
        }
    }
}
