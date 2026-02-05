using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Noteflow.Models;

namespace Noteflow.Services
{
    public class CardSetManagement
    {
        private string _filePath;

        public CardSetManagement(string path)
        {
            _filePath = path;
            Console.WriteLine($"JSON-Datei wird verwendet: {_filePath}");
        }

        public List<CardSet> LoadSets()
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine("Die Datei existiert nicht. Erstelle eine neue JSON-Datei.");
                    File.WriteAllText(_filePath, "[]");
                    return new List<CardSet>();
                }

                var jsonData = File.ReadAllText(_filePath);
                var sets = JsonSerializer.Deserialize<List<CardSet>>(jsonData) ?? new List<CardSet>();
                Console.WriteLine($"{sets.Count} Sets geladen.");
                return sets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Sets: {ex.Message}");
                return new List<CardSet>();
            }
        }

        public void SaveSets(List<CardSet> sets)
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var jsonData = JsonSerializer.Serialize(sets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, jsonData);
                Console.WriteLine("Sets erfolgreich gespeichert.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Sets: {ex.Message}");
            }
        }

        public void ReindexSets(List<CardSet> sets)
        {
            for (int i = 0; i < sets.Count; i++)
            {
                sets[i].Id = i + 1;
                Console.WriteLine($"Set {sets[i].Id}: {sets[i].Name}");
            }
        }
    }
}
