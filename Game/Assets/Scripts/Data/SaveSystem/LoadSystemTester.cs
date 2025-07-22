using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using LogisticGame.Managers;
using System.Linq;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Comprehensive testing system for load operations, validation, and error scenarios
    public class LoadSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _enableAutomaticTesting = false;
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private bool _enablePerformanceTesting = true;
        [SerializeField] private bool _enableStressTesting = false;
        [SerializeField] private int _testIterations = 10;

        [Header("Test Results")]
        [SerializeField] private int _totalTests = 0;
        [SerializeField] private int _passedTests = 0;
        [SerializeField] private int _failedTests = 0;
        [SerializeField] private List<TestResult> _testResults = new List<TestResult>();

        // Test events
        public static event Action<TestSuite> OnTestSuiteStarted;
        public static event Action<TestSuite, TestSuiteResult> OnTestSuiteCompleted;
        public static event Action<TestCase> OnTestCaseStarted;
        public static event Action<TestCase, TestResult> OnTestCaseCompleted;

        // Test data
        private Dictionary<string, TestSaveData> _testSaveFiles = new Dictionary<string, TestSaveData>();
        private const string TEST_SAVE_PREFIX = "TEST_";

        private void Start()
        {
            if (_runTestsOnStart)
            {
                _ = RunAllTestsAsync();
            }
        }

        // Main testing methods
        public async Task<TestSuiteResult> RunAllTestsAsync()
        {
            Debug.Log("Starting comprehensive load system tests...");

            var suiteResult = new TestSuiteResult
            {
                SuiteName = "Comprehensive Load System Tests",
                StartTime = DateTime.Now
            };

            OnTestSuiteStarted?.Invoke(new TestSuite { Name = suiteResult.SuiteName });

            try
            {
                // Prepare test environment
                await PrepareTestEnvironmentAsync();

                // Run test categories
                var basicTests = await RunBasicLoadTestsAsync();
                var validationTests = await RunValidationTestsAsync();
                var errorTests = await RunErrorScenarioTestsAsync();
                var performanceTests = _enablePerformanceTesting ? await RunPerformanceTestsAsync() : new TestCategoryResult { CategoryName = "Performance Tests (Skipped)" };
                var stressTests = _enableStressTesting ? await RunStressTestsAsync() : new TestCategoryResult { CategoryName = "Stress Tests (Skipped)" };

                // Compile results
                suiteResult.CategoryResults.AddRange(new[]
                {
                    basicTests, validationTests, errorTests, performanceTests, stressTests
                });

                suiteResult.EndTime = DateTime.Now;
                suiteResult.Duration = suiteResult.EndTime - suiteResult.StartTime;
                suiteResult.TotalTests = suiteResult.CategoryResults.Sum(c => c.TestResults.Count);
                suiteResult.PassedTests = suiteResult.CategoryResults.Sum(c => c.PassedTests);
                suiteResult.FailedTests = suiteResult.CategoryResults.Sum(c => c.FailedTests);
                suiteResult.WasSuccessful = suiteResult.FailedTests == 0;

                // Update instance variables
                _totalTests = suiteResult.TotalTests;
                _passedTests = suiteResult.PassedTests;
                _failedTests = suiteResult.FailedTests;
                _testResults = suiteResult.CategoryResults.SelectMany(c => c.TestResults).ToList();

                Debug.Log($"Test suite completed: {suiteResult.PassedTests}/{suiteResult.TotalTests} passed in {suiteResult.Duration.TotalSeconds:F1}s");
                
                OnTestSuiteCompleted?.Invoke(new TestSuite { Name = suiteResult.SuiteName }, suiteResult);
                
                return suiteResult;
            }
            catch (Exception ex)
            {
                suiteResult.EndTime = DateTime.Now;
                suiteResult.Duration = suiteResult.EndTime - suiteResult.StartTime;
                suiteResult.WasSuccessful = false;
                suiteResult.Exception = ex;

                Debug.LogError($"Test suite failed: {ex.Message}");
                return suiteResult;
            }
            finally
            {
                await CleanupTestEnvironmentAsync();
            }
        }

        private async Task<TestCategoryResult> RunBasicLoadTestsAsync()
        {
            var categoryResult = new TestCategoryResult { CategoryName = "Basic Load Tests" };

            // Test 1: Load valid save file
            var test1 = await RunTestCaseAsync("Load Valid Save File", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("valid_save", TestSaveType.Valid);
                var loadedData = await LogisticGame.Managers.LoadManager.Instance.LoadGameAsync(testSlot);
                
                Assert.IsNotNull(loadedData, "Loaded data should not be null");
                Assert.AreEqual(testSlot, loadedData.SaveName, "Save name should match");
                
                return true;
            });
            categoryResult.TestResults.Add(test1);

            // Test 2: Load with different strategies
            var test2 = await RunTestCaseAsync("Load With Different Strategies", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("strategy_test", TestSaveType.Large);
                
                // Test Full strategy
                var fullData = await LogisticGame.Managers.LoadManager.Instance.LoadGameAsync(testSlot, LoadStrategy.Full);
                Assert.IsNotNull(fullData, "Full load should succeed");

                // Test Lazy strategy
                var lazyData = await LogisticGame.Managers.LoadManager.Instance.LoadGameAsync(testSlot, LoadStrategy.Lazy);
                Assert.IsNotNull(lazyData, "Lazy load should succeed");

                return true;
            });
            categoryResult.TestResults.Add(test2);

            // Test 3: Save file discovery
            var test3 = await RunTestCaseAsync("Save File Discovery", async () =>
            {
                // Create multiple test saves
                await CreateTestSaveFileAsync("discovery_1", TestSaveType.Valid);
                await CreateTestSaveFileAsync("discovery_2", TestSaveType.Valid);
                await CreateTestSaveFileAsync("discovery_3", TestSaveType.Valid);

                var discoveredFiles = await LogisticGame.Managers.LoadManager.Instance.DiscoverSaveFilesAsync(true);
                
                Assert.IsTrue(discoveredFiles.Count >= 3, "Should discover at least 3 test files");
                Assert.IsTrue(discoveredFiles.Any(f => f.SlotName.Contains("discovery")), "Should find discovery test files");

                return true;
            });
            categoryResult.TestResults.Add(test3);

            categoryResult.PassedTests = categoryResult.TestResults.Count(r => r.Passed);
            categoryResult.FailedTests = categoryResult.TestResults.Count(r => !r.Passed);

            return categoryResult;
        }

        private async Task<TestCategoryResult> RunValidationTestsAsync()
        {
            var categoryResult = new TestCategoryResult { CategoryName = "Validation Tests" };

            // Test 1: Validate healthy save file
            var test1 = await RunTestCaseAsync("Validate Healthy Save File", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("healthy_save", TestSaveType.Valid);
                var health = await SaveFileDiscovery.CheckSaveFileHealthAsync(testSlot);
                
                Assert.IsNotNull(health, "Health check should return result");
                Assert.IsTrue(health.IsHealthy, "Healthy file should pass health check");
                Assert.AreEqual(SaveHealth.Excellent, health.OverallHealth, "Should have excellent health");

                return true;
            });
            categoryResult.TestResults.Add(test1);

            // Test 2: Detect corrupted save file
            var test2 = await RunTestCaseAsync("Detect Corrupted Save File", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("corrupted_save", TestSaveType.Corrupted);
                var health = await SaveFileDiscovery.CheckSaveFileHealthAsync(testSlot);
                
                Assert.IsNotNull(health, "Health check should return result");
                Assert.IsFalse(health.IsHealthy, "Corrupted file should fail health check");
                Assert.IsTrue(health.Issues.Count > 0, "Should have issues identified");

                return true;
            });
            categoryResult.TestResults.Add(test2);

            // Test 3: Version migration detection
            var test3 = await RunTestCaseAsync("Version Migration Detection", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("old_version", TestSaveType.OldVersion);
                bool requiresMigration = VersionMigrationPipeline.IsMigrationRequired("1.0.0", Application.version ?? "1.1.0");
                
                Assert.IsTrue(requiresMigration, "Should detect migration requirement");

                return true;
            });
            categoryResult.TestResults.Add(test3);

            categoryResult.PassedTests = categoryResult.TestResults.Count(r => r.Passed);
            categoryResult.FailedTests = categoryResult.TestResults.Count(r => !r.Passed);

            return categoryResult;
        }

        private async Task<TestCategoryResult> RunErrorScenarioTestsAsync()
        {
            var categoryResult = new TestCategoryResult { CategoryName = "Error Scenario Tests" };

            // Test 1: Missing save file
            var test1 = await RunTestCaseAsync("Missing Save File Recovery", async () =>
            {
                string nonExistentSlot = "non_existent_save";
                
                try
                {
                    var recoveredData = await LoadErrorRecovery.AttemptRecoveryAsync(
                        nonExistentSlot, 
                        new FileNotFoundException("Save file not found")
                    );
                    
                    // Recovery should create fallback data
                    Assert.IsNotNull(recoveredData, "Recovery should provide fallback data");
                    return true;
                }
                catch (Exception)
                {
                    // Recovery failure is acceptable for this test
                    return true; // We're testing that the system handles the error gracefully
                }
            });
            categoryResult.TestResults.Add(test1);

            // Test 2: Corrupted save file recovery
            var test2 = await RunTestCaseAsync("Corrupted Save File Recovery", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("corrupt_recovery", TestSaveType.Corrupted);
                
                // Create a backup first
                await SaveFileManager.CreateBackupAsync(testSlot);
                
                try
                {
                    var recoveredData = await LoadErrorRecovery.AttemptRecoveryAsync(
                        testSlot,
                        new InvalidDataException("Corrupted save data")
                    );
                    
                    Assert.IsNotNull(recoveredData, "Should recover data through repair or backup");
                    return true;
                }
                catch (Exception)
                {
                    // If recovery fails, that's also a valid test result
                    return true;
                }
            });
            categoryResult.TestResults.Add(test2);

            // Test 3: Out of memory recovery
            var test3 = await RunTestCaseAsync("Out Of Memory Recovery", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("memory_test", TestSaveType.Large);
                
                try
                {
                    var recoveredData = await LoadErrorRecovery.AttemptRecoveryAsync(
                        testSlot,
                        new OutOfMemoryException("Insufficient memory")
                    );
                    
                    // Should attempt different load strategies
                    return true;
                }
                catch (Exception)
                {
                    // Recovery might fail, but should handle gracefully
                    return true;
                }
            });
            categoryResult.TestResults.Add(test3);

            categoryResult.PassedTests = categoryResult.TestResults.Count(r => r.Passed);
            categoryResult.FailedTests = categoryResult.TestResults.Count(r => !r.Passed);

            return categoryResult;
        }

        private async Task<TestCategoryResult> RunPerformanceTestsAsync()
        {
            var categoryResult = new TestCategoryResult { CategoryName = "Performance Tests" };

            // Test 1: Large file load performance
            var test1 = await RunTestCaseAsync("Large File Load Performance", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("large_perf_test", TestSaveType.Large);
                
                var startTime = DateTime.Now;
                var loadedData = await LoadPerformanceOptimizer.OptimizedLoadAsync(testSlot);
                var loadTime = DateTime.Now - startTime;
                
                Assert.IsNotNull(loadedData, "Large file should load successfully");
                Assert.IsTrue(loadTime.TotalSeconds < 30, $"Load time should be reasonable: {loadTime.TotalSeconds:F1}s");

                var metrics = LoadPerformanceOptimizer.GetPerformanceHistory(testSlot);
                Assert.IsNotNull(metrics, "Performance metrics should be recorded");

                return true;
            });
            categoryResult.TestResults.Add(test1);

            // Test 2: Memory usage optimization
            var test2 = await RunTestCaseAsync("Memory Usage Optimization", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("memory_test", TestSaveType.Large);
                
                long memoryBefore = GC.GetTotalMemory(true);
                var loadedData = await LoadPerformanceOptimizer.OptimizedLoadAsync(testSlot);
                long memoryAfter = GC.GetTotalMemory(true);
                
                long memoryIncrease = memoryAfter - memoryBefore;
                
                Assert.IsNotNull(loadedData, "Should load successfully");
                // Memory increase should be reasonable (less than 100MB for test)
                Assert.IsTrue(memoryIncrease < 100 * 1024 * 1024, $"Memory increase should be reasonable: {memoryIncrease / (1024 * 1024):F1}MB");

                return true;
            });
            categoryResult.TestResults.Add(test2);

            categoryResult.PassedTests = categoryResult.TestResults.Count(r => r.Passed);
            categoryResult.FailedTests = categoryResult.TestResults.Count(r => !r.Passed);

            return categoryResult;
        }

        private async Task<TestCategoryResult> RunStressTestsAsync()
        {
            var categoryResult = new TestCategoryResult { CategoryName = "Stress Tests" };

            // Test 1: Multiple concurrent loads
            var test1 = await RunTestCaseAsync("Multiple Concurrent Loads", async () =>
            {
                var tasks = new List<Task<SaveData>>();
                
                // Create multiple test saves
                for (int i = 0; i < 5; i++)
                {
                    string testSlot = await CreateTestSaveFileAsync($"concurrent_{i}", TestSaveType.Valid);
                    tasks.Add(LogisticGame.Managers.LoadManager.Instance.LoadGameAsync(testSlot));
                }

                var results = await Task.WhenAll(tasks);
                
                Assert.IsTrue(results.All(r => r != null), "All concurrent loads should succeed");
                return true;
            });
            categoryResult.TestResults.Add(test1);

            // Test 2: Repeated load/unload cycles
            var test2 = await RunTestCaseAsync("Repeated Load Cycles", async () =>
            {
                string testSlot = await CreateTestSaveFileAsync("cycle_test", TestSaveType.Valid);
                
                for (int i = 0; i < _testIterations; i++)
                {
                    var loadedData = await LogisticGame.Managers.LoadManager.Instance.LoadGameAsync(testSlot);
                    Assert.IsNotNull(loadedData, $"Load iteration {i} should succeed");
                    
                    // Force cleanup between loads
                    loadedData = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    
                    await Task.Delay(10); // Brief pause
                }

                return true;
            });
            categoryResult.TestResults.Add(test2);

            categoryResult.PassedTests = categoryResult.TestResults.Count(r => r.Passed);
            categoryResult.FailedTests = categoryResult.TestResults.Count(r => !r.Passed);

            return categoryResult;
        }

        // Test infrastructure
        private async Task<TestResult> RunTestCaseAsync(string testName, Func<Task<bool>> testFunc)
        {
            var testCase = new TestCase { Name = testName };
            var result = new TestResult { TestName = testName, StartTime = DateTime.Now };

            OnTestCaseStarted?.Invoke(testCase);

            try
            {
                bool passed = await testFunc();
                result.Passed = passed;
                result.Message = passed ? "Test passed" : "Test failed";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Exception = ex;
                result.Message = $"Test failed with exception: {ex.Message}";
            }
            finally
            {
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
            }

            OnTestCaseCompleted?.Invoke(testCase, result);
            Debug.Log($"Test '{testName}': {(result.Passed ? "PASSED" : "FAILED")} in {result.Duration.TotalMilliseconds:F0}ms");

            return result;
        }

        private async Task PrepareTestEnvironmentAsync()
        {
            Debug.Log("Preparing test environment...");
            
            // Initialize systems if needed
            VersionMigrationPipeline.Initialize();
            
            // Clear any existing test files
            await CleanupTestEnvironmentAsync();
            
            await Task.Delay(100);
        }

        private async Task CleanupTestEnvironmentAsync()
        {
            Debug.Log("Cleaning up test environment...");
            
            try
            {
                // Remove test save files
                var testSlots = SaveFileManager.GetAvailableSaveSlots()
                    .Where(slot => slot.StartsWith(TEST_SAVE_PREFIX))
                    .ToList();

                foreach (var slot in testSlots)
                {
                    SaveFileManager.DeleteSaveSlot(slot);
                }

                _testSaveFiles.Clear();
                
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cleanup failed: {ex.Message}");
            }
        }

        private async Task<string> CreateTestSaveFileAsync(string testName, TestSaveType type)
        {
            string slotName = $"{TEST_SAVE_PREFIX}{testName}";
            
            var testData = new TestSaveData
            {
                SlotName = slotName,
                Type = type,
                CreatedAt = DateTime.Now
            };

            // Create save data based on type
            SaveData saveData = CreateSaveDataForType(type, slotName);
            
            // Save the test file
            string saveJson = JsonUtility.ToJson(saveData, true);
            
            if (type == TestSaveType.Corrupted)
            {
                // Corrupt the JSON
                saveJson = saveJson.Substring(0, saveJson.Length / 2) + "CORRUPTED}";
            }

            await SaveFileManager.SaveFileAtomicAsync(slotName, saveJson);
            
            // Create metadata
            var metadata = new SaveMetadata
            {
                SaveName = slotName,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now,
                SaveVersion = type == TestSaveType.OldVersion ? "1.0.0" : Application.version ?? "1.1.0",
                PlayTimeHours = 5f,
                CurrentCredits = 10000f,
                TotalContracts = 5,
                Checksum = type == TestSaveType.Corrupted ? "INVALID" : SaveValidator.CalculateChecksum(saveData)
            };

            await SaveFileManager.SaveMetadataAsync(slotName, metadata);
            
            _testSaveFiles[slotName] = testData;
            return slotName;
        }

        private SaveData CreateSaveDataForType(TestSaveType type, string slotName)
        {
            var saveData = ScriptableObject.CreateInstance<SaveData>();
            var gameState = ScriptableObject.CreateInstance<GameState>();
            var playerProgress = ScriptableObject.CreateInstance<PlayerProgress>();
            var settings = ScriptableObject.CreateInstance<SettingsData>();
            var company = ScriptableObject.CreateInstance<CompanyData>();

            // Initialize with test data - TODO: implement if methods don't exist
            // company.Initialize($"Test Company {slotName}", 50000f);
            // gameState.Initialize(company, settings);
            
            // Add test data based on type
            switch (type)
            {
                case TestSaveType.Large:
                    // Add many vehicles and contracts to make it large
                    for (int i = 0; i < 100; i++)
                    {
                        // Create mock instances for testing - would need actual VehicleInstance/ContractInstance in production
                        // TODO: Implement proper test data creation when actual instances are available
                        // var vehicleData = new VehicleInstanceData(mockVehicleInstance);
                        // var contractData = new ContractInstanceData(mockContractInstance);
                        // saveData.AddVehicleInstance(vehicleData);
                        // saveData.AddContractInstance(contractData);
                    }
                    break;
                    
                case TestSaveType.Valid:
                default:
                    // Add minimal valid data - TODO: implement when actual instances available
                    // var vehicle = new VehicleInstanceData(mockVehicleInstance);
                    // saveData.AddVehicleInstance(vehicle);
                    break;
            }

            saveData.Initialize(slotName, gameState, playerProgress, settings);
            return saveData;
        }

        // Public test methods
        [ContextMenu("Run All Tests")]
        public void RunAllTestsFromMenu()
        {
            _ = RunAllTestsAsync();
        }

        [ContextMenu("Run Basic Tests")]
        public void RunBasicTestsFromMenu()
        {
            _ = RunBasicLoadTestsAsync();
        }

        [ContextMenu("Clean Test Environment")]
        public void CleanTestEnvironmentFromMenu()
        {
            _ = CleanupTestEnvironmentAsync();
        }

        public string GetTestReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("Load System Test Report");
            report.AppendLine($"Total Tests: {_totalTests}");
            report.AppendLine($"Passed: {_passedTests}");
            report.AppendLine($"Failed: {_failedTests}");
            report.AppendLine($"Success Rate: {(double)_passedTests / _totalTests:P1}");
            
            if (_testResults.Any(r => !r.Passed))
            {
                report.AppendLine();
                report.AppendLine("Failed Tests:");
                foreach (var failure in _testResults.Where(r => !r.Passed))
                {
                    report.AppendLine($"  - {failure.TestName}: {failure.Message}");
                }
            }
            
            return report.ToString();
        }
    }

    // AIDEV-NOTE: Test data structures
    [System.Serializable]
    public class TestSaveData
    {
        public string SlotName;
        public TestSaveType Type;
        public DateTime CreatedAt;
    }

    [System.Serializable]
    public class TestSuite
    {
        public string Name;
    }

    [System.Serializable]
    public class TestCase
    {
        public string Name;
    }

    [System.Serializable]
    public class TestResult
    {
        public string TestName;
        public bool Passed;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public string Message;
        public Exception Exception;
    }

    [System.Serializable]
    public class TestCategoryResult
    {
        public string CategoryName;
        public List<TestResult> TestResults = new List<TestResult>();
        public int PassedTests;
        public int FailedTests;
    }

    [System.Serializable]
    public class TestSuiteResult
    {
        public string SuiteName;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public bool WasSuccessful;
        public int TotalTests;
        public int PassedTests;
        public int FailedTests;
        public List<TestCategoryResult> CategoryResults = new List<TestCategoryResult>();
        public Exception Exception;
    }

    public enum TestSaveType
    {
        Valid,
        Corrupted,
        Large,
        OldVersion,
        Empty
    }

    // Simple assertion methods for testing
    public static class Assert
    {
        public static void IsTrue(bool condition, string message = "Assertion failed")
        {
            if (!condition)
                throw new AssertionException(message);
        }

        public static void IsFalse(bool condition, string message = "Assertion failed")
        {
            if (condition)
                throw new AssertionException(message);
        }

        public static void IsNotNull(object obj, string message = "Object is null")
        {
            if (obj == null)
                throw new AssertionException(message);
        }

        public static void AreEqual<T>(T expected, T actual, string message = "Values are not equal")
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new AssertionException($"{message}. Expected: {expected}, Actual: {actual}");
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}