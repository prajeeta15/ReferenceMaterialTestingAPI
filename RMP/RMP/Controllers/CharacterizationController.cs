using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CharacterizationController : ControllerBase
{
    [HttpPost("Characterization")]
    public async Task<IActionResult> Characterization(IFormFile jsonfile)
    {
        try
        {
            if (jsonfile == null || jsonfile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Read the JSON file
            using (var stream = new StreamReader(jsonfile.OpenReadStream()))
            {
                string json = await stream.ReadToEndAsync();

                var labDataList = JsonConvert.DeserializeObject<List<CharacterizationData>>(json);

                if (labDataList == null || labDataList.Count == 0)
                {
                    return BadRequest("Invalid JSON data or empty array.");
                }
            
                var results = new List<LabResult>();
                var allValues = new List<double>();

                foreach (var labData in labDataList)
                {
                    var values = new List<double> { labData.Result1, labData.Result2, labData.Result3 };
                    var sortedValues = values.OrderBy(v => v).ToList();
                    var min = sortedValues.First();
                    var max = sortedValues.Last();
                    var median = sortedValues[1]; // The second item in the sorted list of 3 items
                    allValues.AddRange(values);

                    results.Add(new LabResult
                    {
                        LabName = labData.LabName,
                        LabID = labData.LabID,
                        Minimum = min,
                        Maximum = max,
                        Median = median
                    });
                }

                // Calculate the median of all medians
                var medians = results.Select(r => r.Median).OrderBy(m => m).ToList();
                double medianOfMedians = medians.Count % 2 == 0
                    ? (medians[medians.Count / 2 - 1] + medians[medians.Count / 2]) / 2
                    : medians[medians.Count / 2];

                // Calculate the absolute difference for each lab
                foreach (var result in results)
                {
                    result.AbsoluteValue = Math.Abs(result.Median - medianOfMedians);
                }

                // Calculate MAD (Median of Absolute Deviations)
                var absoluteDifferences = results.Select(r => r.AbsoluteValue).OrderBy(ad => ad).ToList();
                double MAD = absoluteDifferences.Count % 2 == 0
                    ? (absoluteDifferences[absoluteDifferences.Count / 2 - 1] + absoluteDifferences[absoluteDifferences.Count / 2]) / 2
                    : absoluteDifferences[absoluteDifferences.Count / 2];

                // Calculate MADe (MAD / 0.674)
                double MADe = MAD / 0.674;

                // Calculate Uchar (mg/l)
                double numOfLabs = results.Count;
                double ucharMgL = CalculateUcharMgL(MAD, numOfLabs);

                // Calculate Uchar, PPM (Sdv) - standard deviation of all values
                double mean = allValues.Average();
                double variance = allValues.Average(v => Math.Pow(v - mean, 2));
                double ucharPPM = Math.Sqrt(variance);

                var summary = new ResultSummary
                {
                    MedianOfMedians = medianOfMedians,
                    MAD = MAD,
                    MADe = MADe,
                    UcharMgL = ucharMgL,
                    UcharPPM = ucharPPM,
                    LabResults = results
                };

                return Ok(summary);
            }

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

    private double CalculateUcharMgL(double MAD, double numOfLabs)
    {
        double MADe = MAD / 0.674;
        return MADe * Math.Sqrt(Math.PI / 2) / Math.Sqrt(numOfLabs);
    }
}
