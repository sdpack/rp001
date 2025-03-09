using hello_world_api.Models;
using System.Text.Json;

namespace hello_world_api.Services
{
    public class JournalService
    {
        private List<JournalEntry> _entries = new List<JournalEntry>();
        private static int _nextId = 1;
        private readonly string _filePath;
        private readonly ILogger<JournalService> _logger;

        public JournalService(IConfiguration configuration, ILogger<JournalService> logger)
        {
            // Get the data directory from configuration or use a default
            var dataDir = configuration["JournalSettings:DataDirectory"] ?? "App_Data";
            
            // Ensure directory exists
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            
            _filePath = Path.Combine(dataDir, "journal-entries.json");
            _logger = logger;
            
            // Load existing entries when service starts
            LoadEntries();
        }

        private void LoadEntries()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var entries = JsonSerializer.Deserialize<List<JournalEntry>>(json);
                    
                    if (entries != null && entries.Any())
                    {
                        _entries = entries;
                        _nextId = _entries.Max(e => e.Id) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading journal entries from file");
            }
        }

        private void SaveEntries()
        {
            try
            {
                var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving journal entries to file");
            }
        }

        public List<JournalEntry> GetEntries(int limit = 0)
        {
            var entries = _entries.OrderByDescending(e => e.Timestamp).ToList();
            
            if (limit > 0)
            {
                entries = entries.Take(limit).ToList();
            }
            
            return entries;
        }

        public List<JournalEntry> GetEntriesByDate(DateTime date)
        {
            return _entries
                .Where(e => e.Timestamp.Date == date.Date)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        public JournalEntry AddEntry(string content)
        {
            var entry = new JournalEntry
            {
                Id = _nextId++,
                Content = content,
                Timestamp = DateTime.UtcNow
            };
            
            _entries.Add(entry);
            
            // Save entries to file after adding a new one
            SaveEntries();
            
            return entry;
        }
    }
} 