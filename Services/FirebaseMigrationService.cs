using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

public class FirebaseMigrationService
{
    private readonly FirestoreDb _db;
    private readonly string _collectionName = "languages";

    public FirebaseMigrationService(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"] ?? throw new ArgumentNullException("Firebase:ProjectId is not configured");
        _db = FirestoreDb.Create(projectId);
    }

    public async Task MigrateDataFromJson(string jsonFilePath)
    {
        try
        {
            // Read JSON file
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var languages = JsonConvert.DeserializeObject<List<Language>>(jsonContent);

            if (languages == null)
            {
                throw new Exception("Failed to deserialize languages data");
            }

            // Validate data to ensure all IDs are present
            var allShlokas = languages.SelectMany(l => l.Data).ToList();
            Console.WriteLine($"Total shlokas loaded from JSON: {allShlokas.Count}");
            Console.WriteLine($"Total chapters: {languages.Count}");
            
            // Print the chapters with their names and shloka counts
            foreach (var lang in languages)
            {
                Console.WriteLine($"Chapter {lang.Id}: {lang.Name1} - {lang.Data?.Count ?? 0} shlokas");
            }
            
            // Check for missing IDs
            var existingIds = allShlokas.Select(s => s.Id).OrderBy(id => id).ToList();
            var minId = existingIds.Min();
            var maxId = existingIds.Max();
            Console.WriteLine($"ID range: {minId} to {maxId}");
            
            // Check that all chapters have shlokas
            var emptyChapters = languages.Where(l => l.Data == null || !l.Data.Any()).ToList();
            if (emptyChapters.Any())
            {
                Console.WriteLine($"WARNING: Found {emptyChapters.Count} chapters with no shlokas");
                foreach (var emptyChapter in emptyChapters)
                {
                    Console.WriteLine($"Empty chapter: {emptyChapter.Id} - {emptyChapter.Name1}");
                }
            }
            
            // Missing IDs check
            var missingIds = new List<int>();
            for (int i = minId; i <= maxId; i++)
            {
                if (!existingIds.Contains(i))
                {
                    missingIds.Add(i);
                    Console.WriteLine($"Missing ID: {i}");
                }
            }

            if (missingIds.Any())
            {
                Console.WriteLine($"Warning: {missingIds.Count} IDs are missing in the range {minId}-{maxId}");
            }

            // Upload to Firebase
            Console.WriteLine("Starting upload to Firebase...");
            int chaptersUploaded = 0;
            
            foreach (var language in languages)
            {
                if (language.Data == null || !language.Data.Any())
                {
                    Console.WriteLine($"Skipping chapter {language.Id} ({language.Name1}) because it has no shlokas");
                    continue;
                }
                
                Console.WriteLine($"Uploading chapter {language.Id}: {language.Name1} with {language.Data.Count} shlokas");
                var docRef = _db.Collection(_collectionName).Document(language.Id.ToString());
                
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "name1", language.Name1 ?? "" },
                    { "name2", language.Name2 ?? "" },
                    { "number1", language.Number1 ?? "" },
                    { "data", JsonConvert.SerializeObject(language.Data) }
                });
                
                chaptersUploaded++;
            }

            Console.WriteLine($"Successfully migrated {chaptersUploaded} chapters to Firebase");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error migrating data: {ex.Message}");
            throw;
        }
    }
} 