using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Noteflow.Models;

namespace Noteflow.Services
{
    public class CardBankManagement
    {
        private string filePath;

        public CardBankManagement(string path)
        {
            filePath = path;
            Console.WriteLine($"JSON-Datei wird verwendet: {filePath}");
        }

        // Lädt die Karten aus der JSON-Datei
        public List<IndexCard> LoadCards()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Die Datei existiert nicht. Erstelle eine neue JSON-Datei.");
                    return new List<IndexCard>();
                }

                var jsonData = File.ReadAllText(filePath);
                var cards = JsonSerializer.Deserialize<List<IndexCard>>(jsonData) ?? new List<IndexCard>();
                Console.WriteLine($"{cards.Count} Karten geladen."); // Debugging-Ausgabe
                return cards;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Karten: {ex.Message}");
                return new List<IndexCard>();
            }
        }

        // Speichert die Karten in der JSON-Datei
        public void SaveCards(List<IndexCard> cards)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(cards, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"JSON-Daten zum Speichern: {jsonData}"); // Debugging-Ausgabe
                File.WriteAllText(filePath, jsonData);
                Console.WriteLine("Karten erfolgreich gespeichert.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Karten: {ex.Message}");
            }
        }

        // Aktualisiert die IDs der Karten, sodass sie fortlaufend nummeriert sind
        public void ReindexCards(List<IndexCard> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Id = i + 1; // IDs beginnen bei 1
                Console.WriteLine($"Karte {cards[i].Id}: {cards[i].Front}"); // Debugging-Ausgabe
            }
            Console.WriteLine("Karten-IDs erfolgreich aktualisiert.");
        }
    }
}