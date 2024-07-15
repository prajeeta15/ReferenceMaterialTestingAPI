# ReferenceMaterialTesting
Bharat Petroleum Internship Project

![image](https://github.com/user-attachments/assets/792553c1-3909-4e86-b5ae-847ff4fe2da0)

TwoWayAnovaApi-HomogeneityTesting Documentation
By Prajeeta Pal (Intern)
Made for Bharat Petroleum Corporation

Overview
This API performs TwoWay ANOVA analysis on a given dataset to assess the effects of two factors and their interaction on a response variable. The API reads a JSON file containing data points, conducts TwoWay ANOVA, and returns various statistical metrics and insights. The API check the homogeneity of the Reference Materials. 

Framework
 Framework: ASP.NET Core
 Version: .NET 8.0
 Environment: Visual Studio Professional 2022
 Language: C#

 Packages Used
 MathNet.Numerics: A library for advanced numerical computations.
 Newtonsoft.Json: A library for JSON serialization and deserialization.
 Swashbuckle.AspNetCore: For API documentation generation.

Objective
The TwoWay ANOVA API is designed to analyse datasets to understand the impact of two independent variables on a dependent variable. This helps in determining if there is a significant interaction between the two factors affecting the outcome.
TwoWay ANOVA allows us to:
1. Determine the main effects of each independent variable.
2. Identify any interaction effect between the independent variables.
  (Fig 1.0)
![image](https://github.com/user-attachments/assets/003e5411-9fee-4166-9e03-5e1942a41c9d)

TwoWay ANOVA Components: ANOVA Table
o	Degrees of Freedom (df): Number of values in the final calculation of a statistic that are free to vary.
o	Sum of Squares (SS): Measures the total variability in the data.
o	Mean Square (MS): Average of the sum of squares (calculated by dividing SS by df).
o	Fvalue: Ratio of the variance between the group means to the variance within the groups.
o	Pvalue: Probability value that helps determine the significance of the results.
  (Fig 1.1)
![image](https://github.com/user-attachments/assets/117658cb-1962-4fc1-879d-977413c40879)

In the second part of the test, we find the F-critical value at 95% or any other value that the user inputs and based on that to calculate of the Reference Material passes the homogeneity test or not.

Randomized Complete Block Design (RCBD)
In experimental design, RCBD is used to control for the variability among experimental units. Each block represents a homogenous grouping of experimental units, reducing the error variance and increasing the precision of the experiment.
RCBD Components:
o	Blocks: Groups of experimental units with similar characteristics.
o	Treatments: Different conditions or treatments applied to the experimental units within each block.
o	Randomization: Ensures that treatments are randomly assigned within each block to avoid systematic bias.



Formulas 
 ![image](https://github.com/user-attachments/assets/dbc76d5f-6044-4e28-89aa-56ecded31f33)

 Short-Term Stability and Long-Term Stability API (Regression)
By Prajeeta Pal (Intern)
made for Bharat Petroleum Corporation
Overview
The Short-Term Stability API aims to performing regression analysis on a given dataset and calculating the short-term stability over the period of 4 weeks and shelf life (1,2,6,12,24 months) of Reference Materials. The API reads a JSON file containing data points, conducts linear regression analysis, and returns various statistical metrics and insights. The Long-Term Stability API is the second stage after the STS calculation to find the Reference Material Stability for per month and their shelf life over 12 and 24 months.
•	Framework: ASP.NET Core 
•	Version: .NET 8.0
•	Environment: Visual Studio Professional 2022
•	Language: C#
•	Packages Used: MathNet.Numerics, NewtonSoft.Json, Swashbuckle.Microsoft.Core 

How Does BPCL calculate short-term stability and long-term stability of Reference Models?
BPCL calculates the short-term and long-term stability of reference models through a structured process after homogeneity testing. Here's how its approached:
Short-Term Stability Testing - 
1.	Sample Preparation: After confirming homogeneity, 8 homogenous bottles are selected for testing.
2.	Division and Storage: Each of these 8 bottles is divided into 2 parts, resulting in a total of 16 samples (8 bottles x 2 parts each).
3.	Testing Schedule: The 16 samples are distributed across 4 weeks, with 2 parts being tested each week.
4.	Temperature Control: Each part is stored at a specific temperature for one week.
5.	Density Measurement: After one week, the density of each sample is measured.
6.	Trend Analysis: This process generates 16 density observations over 4 weeks. These observations are analysed to detect any significant trends in density changes with respect to temperature. The analysis helps in assessing the short-term stability of the reference material and predicting its shelf life over the next 2 years.
Long-Term Stability Testing - 
1.	Extended Testing: After completing the short-term stability testing, long-term stability is assessed using 4 bottles.
1.	2.Sample Preparation: Each of the 4 bottles is divided into 2 parts, resulting in 8 samples.
2.	3.Testing Schedule: The testing is spread over 2 months. For the first month, 2 bottles (4 samples) are tested, followed by the next 2 bottles (4 samples) in the second month.
3.	Density Measurement and Analysis: Similar to the short-term testing, the density of each sample is measured, and the data is analysed to observe trends over the two-month period. This analysis helps in evaluating the long-term stability of the reference material.

Formula Table: 
![image](https://github.com/user-attachments/assets/b75fcb1d-4ccb-45ac-8f64-75d357e5423a)
![image](https://github.com/user-attachments/assets/aaf0c755-c2b0-434e-b33b-61124dd1623f)


Linear Regression in Excel 

Regression Statistics: 
![image](https://github.com/user-attachments/assets/e7ce0d38-aab5-4d7d-8e60-7c16318f5292)

This section tells us how well the calculated linear regression equation fits our source data. 
 
Multiple R:  It is the Correlation Coefficient that measures the strength of a linear relationship between two variables. The correlation coefficient can be any value between 1 and 1, and its absolute value indicates the relationship strength. The larger the absolute value, the stronger the relationship:
o	1 means a strong positive relationship
o	1 means a strong negative relationship
o	0 means no relationship at all

R Square: It is the Coefficient of Determination, which is used as an indicator of the goodness of fit. It shows how many points fall on the regression line. The R2 value is calculated from the total sum of squares, more precisely, it is the sum of the squared deviations of the original data from the mean.

Adjusted R Square: It is the R square adjusted for the number of independent variables in the model. Us will want to use this value instead of R square for multiple regression analysis.

Standard Error: It is another goodnessoffit measure that shows the precision of usr regression analysis the smaller the number, the more certain us can be about usr regression equation. While R2 represents the percentage of the dependent variables variance that is explained by the model, Standard Error is an absolute measure that shows the average distance that the data points fall from the regression line.

Observations: It is simply the number of observations in usr model.

ANOVA:
![image](https://github.com/user-attachments/assets/270d1a87-3932-4f1c-a3c6-689b037ef7f0)

The second section of the output is Analysis of Variance (ANOVA).
 
In statistics, oneway analysis of variance is a technique to compare whether two or more samples' means are significantly different. This analysis of variance technique requires a numeric response variable "Y" (densities) and a single explanatory variable "X" (week number), hence "oneway".
It splits the sum of squares into individual components that give information about the levels of variability within our regression model.
o	df is the number of degrees of freedom associated with the sources of variance.
o	SS is the sum of squares. The smaller the Residual SS compared with the Total SS, the better usr model fits the data.
o	MS is the mean square.
o	F is the F statistic or Ftest for the null hypothesis. It is used to test the overall significance of the model.
o	Significance F is the Pvalue of F.

Coefficients:
![image](https://github.com/user-attachments/assets/e76184f5-9eea-464f-98cb-8e1726440d05)

This section provides specific information about the components of our analysis. 
 
The most useful component in this section is coefficients. It enables us to build a linear regression equation in Excel: y = bx + a 
o	Coefficient: Gives us the least squares estimate.
o	Standard Error: the least squares estimate of the standard error.
o	T Statistic: The T Statistic for the null hypothesis vs. the alternate hypothesis. 
o	P Value: Gives us the pvalue for the hypothesis test.
o	Lower 95%: The lower boundary for the confidence interval.
o	Upper 95%: The upper boundary for the confidence interval.
*Confidence Interval is taken standard at 95%.

Characterization API 
By Prajeeta Pal (Intern)
made for Bharat Petroleum Corporation
Overview
The API aims to take input in a json file with 3 lab results each of 10 external labs for homogeneity, then find the minimum, maximum and median of the three and perform further calculations to verify the homogeneity of the Reference Material with the external labs and internal labs to find uchar, based on which a certification final will be generated to pass the Reference Material to be distributed. 
•	Framework: ASP.NET Core 
•	Version: .NET 8.0
•	Environment: Visual Studio Professional 2022
•	Language: C#
•	Packages Used: MathNet.Numerics, NewtonSoft.Json, Swashbuckle.Microsoft.Core 
Formula Table:
![image](https://github.com/user-attachments/assets/ac1c3677-1059-4579-8e9b-674f2ca01a39)
The code will return uchar in two units – mg/L and Sdv and the user can choose which one they would want to consider for the next stage of calculation – Final Certification. 





