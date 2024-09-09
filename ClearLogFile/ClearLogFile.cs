using ClearLogFile.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearLogFile
{
    internal class ClearLogFile : BackgroundService
    {
        private ILogger _logger;
        private ConfigSettings _config;
        private System.Threading.Timer timerSportEvent = null;
        public ClearLogFile(ILogger logger, ConfigSettings config) 
        {
            _logger = logger;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //await Task.Delay(10, stoppingToken);
            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            timerSportEvent = new System.Threading.Timer(OnElapsedTime_SportEvent, null, 3000, System.Threading.Timeout.Infinite);
            return base.StartAsync(cancellationToken);
        }
        private void OnElapsedTime_SportEvent(object? state) 
        {
            try
            {
                List<ClearLogList>? clear_log_list = _config?.clear_log_list;
                if (clear_log_list != null) 
                {
                    foreach (var item in clear_log_list)
                    {
                        if (item != null) 
                        {
                            if (item.path != null && item.path.Count > 0) 
                            {
                                foreach (var item_path in item.path) 
                                {
                                    if (!string.IsNullOrEmpty(item_path))
                                    {
                                        if (Directory.Exists(item_path)) 
                                        {
                                            var files = Directory.EnumerateFiles(item_path, "*.*", SearchOption.AllDirectories)
                                            .Where
                                            (
                                                file => 
                                                {
                                                    List<string?>? extention_list = item.extention_list;
                                                    return extention_list.Any
                                                    (
                                                        ext => !string.IsNullOrEmpty(ext) 
                                                        && file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
                                                    ) 
                                                    &&
                                                    (DateTime.Now - File.GetLastWriteTime(file)).TotalDays > (item?.keep_last_n_days ?? 30);
                                                }
                                            )
                                            .ToList();

                                            foreach (var file in files) 
                                            {
                                                if ((_config?.run_from ?? 2) <= DateTime.Now.Hour && DateTime.Now.Hour <= (_config?.run_to ?? 3))
                                                {
#if DEBUG
                                                    Console.WriteLine($"Will be Deleted: {file}");
#else
                                                    // Attempt to open the file with exclusive access
                                                    try
                                                    {
                                                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                                        {
                                                            // If successful, close the stream and delete the file
                                                            fs.Close();
                                                        }

                                                        // Delete the file
                                                        File.Delete(file);
                                                        Console.WriteLine($"Deleted: {file}");
                                                    }
                                                    catch (IOException)
                                                    {
                                                        // The file is in use by another process
                                                        Console.WriteLine($"File is in use and cannot be deleted: {file}");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // Handle other exceptions
                                                        Console.WriteLine($"An error occurred with file {file}: {ex.Message}");
                                                    }
#endif
                                                }
                                            }
                                            Console.ReadKey();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: Access to the path '{ex.Message}' is denied.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"OnElapsedTime_SportEvent exception: ", ex);
                throw;
            }
            finally 
            {
                timerSportEvent.Change(3000, System.Threading.Timeout.Infinite);
            }
        }
    }
}
