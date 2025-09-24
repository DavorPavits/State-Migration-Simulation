using System.Dynamic;
using System.Text.Json;

namespace StateMigrationApp
{
    //Application State
    public class AppState
    {
        public int Counter { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastUpdate { get; set; }
        public string ContainerName { get; set; } = "";
        public List<string> ProcessedTasks { get; set; } = new List<string>();
     }

    class Program
    {
        private static readonly string StateFile = "/app/snapshots/app_state.json";
        private static readonly string ContainerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "unknown";
        private static AppState _state = new AppState();
        private static bool _running = true;

        static async Task Main(String[] args)
        {
            Console.WriteLine($"---Starting Application on {ContainerName}---");

            
            //Try to load existing state
            LoadState();

            //Update container name
            _state.ContainerName = ContainerName;

            //Handle shutdown graefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nShutdown signal received. Saving State...");
                SaveState();
                _running = false;
            };

            //Main application loop
            await RunApp();
        }
        static async Task RunApp()
        {
            Console.WriteLine($"Application resumed on {ContainerName}");
            Console.WriteLine($"Current counter: {_state.Counter}");
            Console.WriteLine($"Tasks processed so far: {_state.ProcessedTasks.Count}");

            while (_running)
            {
                //Simulate some work
                _state.Counter++;
                _state.LastUpdate = DateTime.Now;

                string task = $"Task_{_state.Counter}_{DateTime.Now:HH:mm:ss}";
                _state.ProcessedTasks.Add(task);

                Console.WriteLine($"[{ContainerName}] Processing {task} (Counter: {_state.Counter})");

                //Save state every 10 iterations (in case of unexpected shutdown)
                if (_state.Counter % 10 == 0)
                {
                    SaveState();
                    Console.WriteLine($"[{ContainerName}] State checkpoint saved");
                }

                //Simulate work time
                await Task.Delay(3000);
            }

            SaveState();
            Console.WriteLine($"[{ContainerName}] Application stopped gracefully");
            
        }

        static void SaveState()
        {
            try
            {
                //Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(StateFile) ?? "/shared");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(_state, options);
                File.WriteAllText(StateFile, json);

                Console.WriteLine($"[{ContainerName}] State savedto {StateFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ContainerName}] Failed to save state: {ex.Message}");
            }
            
        }

        static void LoadState()
        {
            try
            {
                if (File.Exists(StateFile))
                {
                    string json = File.ReadAllText(StateFile);
                    var loadedState = JsonSerializer.Deserialize<AppState>(json);

                    if (loadedState != null)
                    {
                        _state = loadedState;
                        Console.WriteLine($"[{ContainerName}] State loaded from {StateFile}");
                        Console.WriteLine($"[{ContainerName}] Previous contaner: {_state.ContainerName}");
                        Console.WriteLine($"[{ContainerName}] Resuming from counter: {_state.Counter}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ContainerName}] Could not load state: {ex.Message}");
            }

            //Initialize new state if loading failed
            Console.WriteLine($"[{ContainerName}] Starting with fresh state");
            _state = new AppState
            {
                StartTime = DateTime.Now,
                Counter = 0,
                ContainerName = ContainerName
            };
        }

    }
}