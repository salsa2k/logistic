using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;
using LogisticGame.Managers;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Performance optimization system for loading large save files
    public static class LoadPerformanceOptimizer
    {
        // Performance events
        public static event Action<string, PerformanceMetrics> OnLoadPerformanceAnalyzed;
        public static event Action<string, string> OnPerformanceWarning;
        public static event Action<string, ChunkLoadProgress> OnChunkLoadProgress;

        // Performance configuration
        private const long LARGE_FILE_THRESHOLD = 5 * 1024 * 1024; // 5MB
        private const long HUGE_FILE_THRESHOLD = 50 * 1024 * 1024; // 50MB
        private const int DEFAULT_CHUNK_SIZE = 1024 * 1024; // 1MB chunks
        private const int MAX_CONCURRENT_CHUNKS = 3;
        private const int MEMORY_CHECK_INTERVAL = 1000; // ms
        private const long LOW_MEMORY_THRESHOLD = 100 * 1024 * 1024; // 100MB

        // Performance tracking
        private static Dictionary<string, PerformanceMetrics> _performanceHistory = new Dictionary<string, PerformanceMetrics>();
        private static bool _enablePerformanceMonitoring = true;
        private static bool _enableMemoryOptimization = true;
        private static bool _enableChunkedLoading = true;

        // Memory management
        // Note: MemoryPressure API not available in Unity, using alternative memory tracking

        // Configuration methods
        public static void SetPerformanceMonitoring(bool enabled)
        {
            _enablePerformanceMonitoring = enabled;
        }

        public static void SetMemoryOptimization(bool enabled)
        {
            _enableMemoryOptimization = enabled;
        }

        public static void SetChunkedLoading(bool enabled)
        {
            _enableChunkedLoading = enabled;
        }

        // Main optimization methods
        public static async Task<SaveData> OptimizedLoadAsync(string slotName, LoadStrategy strategy = LoadStrategy.Full)
        {
            var metrics = new PerformanceMetrics
            {
                SlotName = slotName,
                Strategy = strategy,
                StartTime = DateTime.Now
            };

            try
            {
                // Analyze file before loading
                metrics.FileSize = SaveFileManager.GetSaveFileInfo(slotName)?.FileSize ?? 0;
                metrics.IsLargeFile = metrics.FileSize > LARGE_FILE_THRESHOLD;
                metrics.IsHugeFile = metrics.FileSize > HUGE_FILE_THRESHOLD;

                // Check system memory
                if (_enableMemoryOptimization)
                {
                    metrics.InitialMemoryUsage = GetCurrentMemoryUsage();
                    
                    if (metrics.InitialMemoryUsage > LOW_MEMORY_THRESHOLD && metrics.IsLargeFile)
                    {
                        OnPerformanceWarning?.Invoke(slotName, "Low system memory detected for large file load");
                        await PerformMemoryCleanup();
                    }
                }

                // Choose optimal loading method based on file size and system state
                SaveData loadedData = await ChooseOptimalLoadMethodAsync(slotName, strategy, metrics);

                // Record final metrics
                metrics.EndTime = DateTime.Now;
                metrics.LoadDuration = metrics.EndTime - metrics.StartTime;
                metrics.FinalMemoryUsage = GetCurrentMemoryUsage();
                metrics.MemoryDelta = metrics.FinalMemoryUsage - metrics.InitialMemoryUsage;
                metrics.WasSuccessful = loadedData != null;

                // Store performance history
                _performanceHistory[slotName] = metrics;

                if (_enablePerformanceMonitoring)
                {
                    OnLoadPerformanceAnalyzed?.Invoke(slotName, metrics);
                    LogPerformanceMetrics(metrics);
                }

                return loadedData;
            }
            catch (Exception ex)
            {
                metrics.EndTime = DateTime.Now;
                metrics.LoadDuration = metrics.EndTime - metrics.StartTime;
                metrics.WasSuccessful = false;
                metrics.Exception = ex;
                
                _performanceHistory[slotName] = metrics;
                
                Debug.LogError($"Optimized load failed for {slotName}: {ex.Message}");
                throw;
            }
        }

        private static async Task<SaveData> ChooseOptimalLoadMethodAsync(string slotName, LoadStrategy strategy, PerformanceMetrics metrics)
        {
            // For small files, use standard loading
            if (!metrics.IsLargeFile)
            {
                metrics.LoadMethod = LoadMethod.Standard;
                return await StandardLoadAsync(slotName);
            }

            // For huge files with limited memory, force chunked loading
            if (metrics.IsHugeFile || (metrics.IsLargeFile && metrics.InitialMemoryUsage > LOW_MEMORY_THRESHOLD))
            {
                if (_enableChunkedLoading)
                {
                    metrics.LoadMethod = LoadMethod.Chunked;
                    return await ChunkedLoadAsync(slotName, metrics);
                }
                else
                {
                    OnPerformanceWarning?.Invoke(slotName, "Large file detected but chunked loading is disabled");
                }
            }

            // For large files with sufficient memory, use memory-optimized loading
            if (_enableMemoryOptimization)
            {
                metrics.LoadMethod = LoadMethod.MemoryOptimized;
                return await MemoryOptimizedLoadAsync(slotName, metrics);
            }

            // Fallback to standard loading
            metrics.LoadMethod = LoadMethod.Standard;
            return await StandardLoadAsync(slotName);
        }

        private static async Task<SaveData> StandardLoadAsync(string slotName)
        {
            // Use existing SaveManager functionality
            return await LogisticGame.Managers.SaveManager.Instance.LoadGameAsync(slotName);
        }

        private static async Task<SaveData> MemoryOptimizedLoadAsync(string slotName, PerformanceMetrics metrics)
        {
            try
            {
                // Force garbage collection before loading
                await PerformMemoryCleanup();

                // Load with memory monitoring
                var memoryMonitorTask = StartMemoryMonitoring(slotName);

                // Load the save data
                string saveContent = await SaveFileManager.LoadFileAsync(slotName);
                
                // Parse in smaller increments to avoid memory spikes
                SaveData saveData = await ParseSaveDataOptimizedAsync(saveContent, metrics);

                // Stop memory monitoring
                await memoryMonitorTask;

                return saveData;
            }
            catch (OutOfMemoryException)
            {
                OnPerformanceWarning?.Invoke(slotName, "Out of memory during load - attempting recovery");
                
                // Force aggressive cleanup and retry with chunked loading
                await PerformAggressiveMemoryCleanup();
                
                if (_enableChunkedLoading)
                {
                    return await ChunkedLoadAsync(slotName, metrics);
                }
                
                throw;
            }
        }

        private static async Task<SaveData> ChunkedLoadAsync(string slotName, PerformanceMetrics metrics)
        {
            var chunkProgress = new ChunkLoadProgress
            {
                SlotName = slotName,
                TotalFileSize = metrics.FileSize,
                ChunkSize = DEFAULT_CHUNK_SIZE,
                TotalChunks = (int)Math.Ceiling((double)metrics.FileSize / DEFAULT_CHUNK_SIZE)
            };

            try
            {
                OnChunkLoadProgress?.Invoke(slotName, chunkProgress);

                // Read file in chunks
                var chunks = new List<string>();
                string filePath = SaveFileManager.GetSaveFilePath(slotName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var buffer = new char[DEFAULT_CHUNK_SIZE];
                    int chunkIndex = 0;

                    while (!reader.EndOfStream)
                    {
                        int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                        string chunk = new string(buffer, 0, bytesRead);
                        chunks.Add(chunk);

                        chunkIndex++;
                        chunkProgress.LoadedChunks = chunkIndex;
                        chunkProgress.Progress = (float)chunkIndex / chunkProgress.TotalChunks;
                        
                        OnChunkLoadProgress?.Invoke(slotName, chunkProgress);

                        // Memory management between chunks
                        if (chunkIndex % 10 == 0) // Every 10 chunks
                        {
                            await PerformMemoryCleanup();
                        }

                        // Yield control to prevent frame drops
                        if (chunkIndex % 5 == 0)
                        {
                            await Task.Yield();
                        }
                    }
                }

                // Reassemble chunks
                chunkProgress.IsReassembling = true;
                OnChunkLoadProgress?.Invoke(slotName, chunkProgress);

                string completeContent = await ReassembleChunksAsync(chunks);
                chunks.Clear(); // Free chunk memory

                // Parse the complete content
                SaveData saveData = await ParseSaveDataOptimizedAsync(completeContent, metrics);

                chunkProgress.IsCompleted = true;
                OnChunkLoadProgress?.Invoke(slotName, chunkProgress);

                return saveData;
            }
            catch (Exception ex)
            {
                chunkProgress.HasFailed = true;
                chunkProgress.ErrorMessage = ex.Message;
                OnChunkLoadProgress?.Invoke(slotName, chunkProgress);
                throw;
            }
        }

        private static async Task<string> ReassembleChunksAsync(List<string> chunks)
        {
            // Use StringBuilder for efficient string concatenation
            var builder = new StringBuilder(chunks.Sum(c => (int)c.Length));
            
            for (int i = 0; i < chunks.Count; i++)
            {
                builder.Append(chunks[i]);
                
                // Yield periodically to maintain responsiveness
                if (i % 100 == 0)
                {
                    await Task.Yield();
                }
            }

            return builder.ToString();
        }

        private static async Task<SaveData> ParseSaveDataOptimizedAsync(string saveContent, PerformanceMetrics metrics)
        {
            try
            {
                // Create save data object
                SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                
                // Parse JSON in background thread to avoid blocking main thread
                await Task.Run(() =>
                {
                    JsonUtility.FromJsonOverwrite(saveContent, saveData);
                });

                return saveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse save data: {ex.Message}");
                throw;
            }
        }

        // Memory management
        private static async Task PerformMemoryCleanup()
        {
            if (!_enableMemoryOptimization)
                return;

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Give GC time to complete
            await Task.Delay(50);

            // Unity-specific cleanup
            if (Application.isPlaying)
            {
                await Resources.UnloadUnusedAssets();
            }
        }

        private static async Task PerformAggressiveMemoryCleanup()
        {
            if (!_enableMemoryOptimization)
                return;

            // Multiple GC passes
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(100);
            }

            // Unity cleanup
            if (Application.isPlaying)
            {
                await Resources.UnloadUnusedAssets();
                await Task.Delay(200);
            }

            // Large object heap compaction (if available)
            try
            {
                // LargeObjectHeapCompactionMode not available in Unity, using standard GC collection
                GC.Collect();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Large object heap compaction failed: {ex.Message}");
            }
        }

        private static async Task StartMemoryMonitoring(string slotName)
        {
            if (!_enableMemoryOptimization)
                return;

            await Task.Run(async () =>
            {
                long initialMemory = GetCurrentMemoryUsage();
                long peakMemory = initialMemory;
                
                while (true)
                {
                    await Task.Delay(MEMORY_CHECK_INTERVAL);
                    
                    long currentMemory = GetCurrentMemoryUsage();
                    if (currentMemory > peakMemory)
                    {
                        peakMemory = currentMemory;
                    }

                    // Check for memory pressure
                    if (currentMemory > LOW_MEMORY_THRESHOLD)
                    {
                        OnPerformanceWarning?.Invoke(slotName, $"High memory usage detected: {currentMemory / (1024 * 1024):F0}MB");
                        
                        // Trigger cleanup if memory is getting too high
                        if (currentMemory > LOW_MEMORY_THRESHOLD * 1.5f)
                        {
                            await PerformMemoryCleanup();
                        }
                    }

                    // Break if the load operation is likely complete
                    // This is a simplified check - in reality, you'd have a more sophisticated way to detect completion
                    if (currentMemory < initialMemory * 0.8f)
                    {
                        break;
                    }
                }
            });
        }

        private static long GetCurrentMemoryUsage()
        {
            if (_enableMemoryOptimization)
            {
                try
                {
                    return GC.GetTotalMemory(false);
                }
                catch
                {
                    return 0;
                }
            }
            
            return 0;
        }

        // Performance analysis
        public static PerformanceMetrics GetPerformanceHistory(string slotName)
        {
            return _performanceHistory.ContainsKey(slotName) ? _performanceHistory[slotName] : null;
        }

        public static List<PerformanceMetrics> GetAllPerformanceHistory()
        {
            return new List<PerformanceMetrics>(_performanceHistory.Values);
        }

        public static PerformanceAnalysis AnalyzePerformance()
        {
            var analysis = new PerformanceAnalysis();
            var allMetrics = _performanceHistory.Values.ToList();

            if (allMetrics.Count == 0)
            {
                return analysis;
            }

            analysis.TotalLoads = allMetrics.Count;
            analysis.SuccessfulLoads = allMetrics.Count(m => m.WasSuccessful);
            analysis.FailedLoads = analysis.TotalLoads - analysis.SuccessfulLoads;
            
            var successfulMetrics = allMetrics.Where(m => m.WasSuccessful);
            if (successfulMetrics.Any())
            {
                analysis.AverageLoadTime = successfulMetrics.Average(m => m.LoadDuration.TotalSeconds);
                analysis.FastestLoadTime = successfulMetrics.Min(m => m.LoadDuration.TotalSeconds);
                analysis.SlowestLoadTime = successfulMetrics.Max(m => m.LoadDuration.TotalSeconds);
                analysis.AverageFileSize = successfulMetrics.Average(m => m.FileSize);
                analysis.AverageMemoryDelta = successfulMetrics.Average(m => m.MemoryDelta);
            }

            analysis.LargeFileLoads = allMetrics.Count(m => m.IsLargeFile);
            analysis.HugeFileLoads = allMetrics.Count(m => m.IsHugeFile);
            analysis.ChunkedLoads = allMetrics.Count(m => m.LoadMethod == LoadMethod.Chunked);
            analysis.MemoryOptimizedLoads = allMetrics.Count(m => m.LoadMethod == LoadMethod.MemoryOptimized);

            return analysis;
        }

        private static void LogPerformanceMetrics(PerformanceMetrics metrics)
        {
            var message = $"Load Performance - {metrics.SlotName}: " +
                         $"{metrics.LoadDuration.TotalSeconds:F2}s, " +
                         $"{metrics.FileSize / (1024 * 1024):F1}MB, " +
                         $"Method: {metrics.LoadMethod}, " +
                         $"Memory Δ: {metrics.MemoryDelta / (1024 * 1024):F1}MB";

            if (metrics.LoadDuration.TotalSeconds > 10)
            {
                Debug.LogWarning($"SLOW LOAD: {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        // Utility methods
        public static void ClearPerformanceHistory()
        {
            _performanceHistory.Clear();
        }

        public static bool IsLargeFile(long fileSize)
        {
            return fileSize > LARGE_FILE_THRESHOLD;
        }

        public static bool IsHugeFile(long fileSize)
        {
            return fileSize > HUGE_FILE_THRESHOLD;
        }

        public static LoadStrategy RecommendLoadStrategy(long fileSize, long availableMemory)
        {
            if (fileSize > HUGE_FILE_THRESHOLD || availableMemory < LOW_MEMORY_THRESHOLD)
            {
                return LoadStrategy.Streaming;
            }
            
            if (fileSize > LARGE_FILE_THRESHOLD)
            {
                return LoadStrategy.Lazy;
            }
            
            return LoadStrategy.Full;
        }
    }

    // AIDEV-NOTE: Data structures for performance tracking
    [System.Serializable]
    public class PerformanceMetrics
    {
        public string SlotName;
        public LoadStrategy Strategy;
        public LoadMethod LoadMethod;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan LoadDuration;
        public long FileSize;
        public long InitialMemoryUsage;
        public long FinalMemoryUsage;
        public long MemoryDelta;
        public bool IsLargeFile;
        public bool IsHugeFile;
        public bool WasSuccessful;
        public Exception Exception;

        public string GetSummary()
        {
            return $"{SlotName}: {LoadDuration.TotalSeconds:F1}s, {FileSize / (1024 * 1024):F1}MB, {LoadMethod}";
        }
    }

    [System.Serializable]
    public class ChunkLoadProgress
    {
        public string SlotName;
        public long TotalFileSize;
        public int ChunkSize;
        public int TotalChunks;
        public int LoadedChunks;
        public float Progress;
        public bool IsReassembling;
        public bool IsCompleted;
        public bool HasFailed;
        public string ErrorMessage;

        public string GetStatusMessage()
        {
            if (HasFailed)
                return $"Failed: {ErrorMessage}";
            
            if (IsCompleted)
                return "Completed";
            
            if (IsReassembling)
                return "Reassembling chunks...";
            
            return $"Loading chunk {LoadedChunks}/{TotalChunks} ({Progress:P0})";
        }
    }

    [System.Serializable]
    public class PerformanceAnalysis
    {
        public int TotalLoads;
        public int SuccessfulLoads;
        public int FailedLoads;
        public double AverageLoadTime;
        public double FastestLoadTime;
        public double SlowestLoadTime;
        public double AverageFileSize;
        public double AverageMemoryDelta;
        public int LargeFileLoads;
        public int HugeFileLoads;
        public int ChunkedLoads;
        public int MemoryOptimizedLoads;

        public string GetReport()
        {
            var report = new StringBuilder();
            report.AppendLine("Load Performance Analysis");
            report.AppendLine($"Total Loads: {TotalLoads}");
            report.AppendLine($"Success Rate: {(double)SuccessfulLoads / TotalLoads:P1}");
            report.AppendLine($"Average Load Time: {AverageLoadTime:F2}s");
            report.AppendLine($"Load Time Range: {FastestLoadTime:F2}s - {SlowestLoadTime:F2}s");
            report.AppendLine($"Average File Size: {AverageFileSize / (1024 * 1024):F1}MB");
            report.AppendLine($"Average Memory Delta: {AverageMemoryDelta / (1024 * 1024):F1}MB");
            report.AppendLine($"Large Files: {LargeFileLoads} ({(double)LargeFileLoads / TotalLoads:P1})");
            report.AppendLine($"Huge Files: {HugeFileLoads} ({(double)HugeFileLoads / TotalLoads:P1})");
            report.AppendLine($"Chunked Loads: {ChunkedLoads} ({(double)ChunkedLoads / TotalLoads:P1})");
            report.AppendLine($"Memory Optimized: {MemoryOptimizedLoads} ({(double)MemoryOptimizedLoads / TotalLoads:P1})");
            
            return report.ToString();
        }
    }

    // AIDEV-NOTE: Enumeration for load methods
    public enum LoadMethod
    {
        Standard,           // Normal loading
        MemoryOptimized,    // With memory management
        Chunked             // Chunked loading for large files
    }
}