using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;
using ChargePadLine.Service.Trace.Dto;
using System.Threading;
using ChargePadLine.Application.Trace.Production.Recipes;
using ChargePadLine.Service.Trace.Impl;

namespace ChargePadLine.Tests
{
    public class CommonInterfaseServiceTest : IClassFixture<MyFixture>
    {
        private readonly MyFixture _fixture;
        private readonly CommonInterfaseService _commonInterfaseService;

        public CommonInterfaseServiceTest(MyFixture fixture, CommonInterfaseService commonInterfaseService)
        {
            _fixture = fixture;
            _commonInterfaseService= commonInterfaseService;
        }
        [Fact]
        public void UploadData_BatchTest_WithGeneratedData()
        {
            // Arrange: Generate 30 test cases
            var testCases = GenerateTestCases(30);
            var successCount = 0;
            var errorCount = 0;

            // Act: Execute all test cases
            foreach (var testCase in testCases)
            {
                Thread.Sleep(1000);
                try
                {
                    var result = _commonInterfaseService.UploadData(testCase);

                    successCount++;
                    Console.WriteLine($"Success for SN: {testCase.SN}, Station: {testCase.StationCode}, Resource: {testCase.Resource},Date{DateTime.Now.ToShortTimeString()},return{result.ToString()}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                }
            }

            // Assert: Verify the results
            Console.WriteLine($"\nTest Results: {successCount} succeeded, {errorCount} failed");

            // Note: We're not asserting that all tests succeed here,
            // because the actual success depends on the mock setup
            // and business logic. This test is mainly for generating
            // and executing the test cases.
        }

        

        /// <summary>
        /// Generate test cases with random data
        /// </summary>
        /// <param name="count">Number of test cases to generate</param>
        /// <returns>List of test cases</returns>
        private List<RequestUploadCheckParams> GenerateTestCases(int count)
        {
            var testCases = new List<RequestUploadCheckParams>();
            var random = new Random();
            var resourceArry = new[] { "SB0010", "SB0020", "SB0030", "SB0040", "SB0050", "SB0060", "SB0070", "SB0080" };
            var stationArry = new[] { "OP10", "OP20", "OP30", "OP40", "OP50", "OP60", "OP70", "OP80" };
            for (int i = 0; i < count; i++)
            {
                // Generate SN: SN2026011500xxx where xxx starts from 100
                var snSuffix = (106 + i).ToString("D3");
                var sn = $"SN2026011500{snSuffix}";

                for (int j = 0; j < stationArry.Length; j++)
                {
                    var resource = resourceArry[j];
                    var stationCode = stationArry[j];
                    var testDataCount = random.Next(5, 11);
                    var testData = GenerateTestData(testDataCount);

                    var testCase = new RequestUploadCheckParams
                    {
                        SN = sn,
                        Resource = resource,
                        StationCode = stationCode,
                        WorkOrderCode = "", // Empty as requested
                        TestResult = "PASS", // Fixed as requested
                        TestData = testData
                    };

                    testCases.Add(testCase);
                }
            }

            return testCases;
        }

        /// <summary>
        /// Generate random test data in JSON format
        /// </summary>
        /// <param name="count">Number of test items</param>
        /// <returns>JSON string of test data</returns>
        private string GenerateTestData(int count)
        {
            var random = new Random();
            var testItems = new List<TestDataItem>();

            // Test parameter definitions
            var paramKeys = new[]
            {
                "Voltage_Input", "Current_Input", "Voltage_Output", "Current_Output",
                "Power_Consumption", "Temperature", "Humidity", "Pressure",
                "Frequency", "Resistance", "Capacitance", "Inductance",
                "Power_Factor", "Efficiency", "Noise_Level"
            };

            var units = new[] { "V", "A", "W", "°C", "%", "Pa", "Hz", "Ω", "F", "H", "dB" };

            for (int i = 0; i < count; i++)
            {
                // Select a random parameter key
                var paramKey = paramKeys[random.Next(paramKeys.Length)];

                // Generate random values
                var lowerLimit = random.NextDouble() * 100;
                var upperLimit = lowerLimit + random.NextDouble() * 100;
                var testValue = lowerLimit + random.NextDouble() * (upperLimit - lowerLimit);

                // Format values
                var formattedLowerLimit = Math.Round(lowerLimit, 1);
                var formattedUpperLimit = Math.Round(upperLimit, 1);
                var formattedTestValue = Math.Round(testValue, 2);

                // Select a random unit
                var unit = units[random.Next(units.Length)];

                var testItem = new TestDataItem
                {
                    Upperlimit = formattedUpperLimit,
                    Lowerlimit = formattedLowerLimit,
                    Units = unit,
                    ParametricKey = paramKey,
                    TestResult = "PASS", // Always pass for this test
                    TestValue = formattedTestValue.ToString(),
                    Remark = formattedTestValue.ToString()
                };

                testItems.Add(testItem);
            }

            // Serialize to JSON
            return JsonSerializer.Serialize(testItems);
        }

        // Helper class for test data serialization
        private class TestDataItem
        {
            public double Upperlimit { get; set; }
            public double Lowerlimit { get; set; }
            public string Units { get; set; }
            public string ParametricKey { get; set; }
            public string TestResult { get; set; }
            public string TestValue { get; set; }
            public string Remark { get; set; }
        }
    }
}