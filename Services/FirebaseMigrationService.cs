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

    public async Task VerifyAndFixMissingShlokas(string jsonFilePath)
    {
        try
        {
            // Read JSON file for complete data
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var completeLanguages = JsonConvert.DeserializeObject<List<Language>>(jsonContent);

            if (completeLanguages == null)
            {
                throw new Exception("Failed to deserialize languages data from JSON file");
            }

            // Get all complete shlokas from JSON
            var completeShlokas = completeLanguages.SelectMany(l => l.Data).ToList();
            Console.WriteLine($"Total shlokas in JSON file: {completeShlokas.Count}");
            Console.WriteLine($"Total chapters in JSON file: {completeLanguages.Count}");

            // Get current data from Firebase
            var snapshot = await _db.Collection(_collectionName).GetSnapshotAsync();
            var currentLanguages = new List<Language>();
            
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var language = new Language
                {
                    Id = int.Parse(doc.Id),
                    Name1 = data["name1"].ToString(),
                    Name2 = data["name2"].ToString(),
                    Number1 = data.ContainsKey("number1") ? data["number1"].ToString() : "",
                    Data = JsonConvert.DeserializeObject<List<Shloka>>(data["data"].ToString() ?? "[]") ?? new List<Shloka>()
                };
                currentLanguages.Add(language);
            }

            var currentShlokas = currentLanguages.SelectMany(l => l.Data).ToList();
            Console.WriteLine($"Total shlokas in Firebase: {currentShlokas.Count}");
            Console.WriteLine($"Total chapters in Firebase: {currentLanguages.Count}");

            // Check for missing chapters
            var existingChapterIds = currentLanguages.Select(l => l.Id).ToHashSet();
            var missingChapters = completeLanguages.Where(l => !existingChapterIds.Contains(l.Id)).ToList();
            
            if (missingChapters.Any())
            {
                Console.WriteLine($"Found {missingChapters.Count} missing chapters that need to be added");
                foreach (var chapter in missingChapters)
                {
                    Console.WriteLine($"Adding missing chapter {chapter.Id}: {chapter.Name1} with {chapter.Data?.Count ?? 0} shlokas");
                    await _db.Collection(_collectionName)
                        .Document(chapter.Id.ToString())
                        .SetAsync(new Dictionary<string, object>
                        {
                            { "name1", chapter.Name1 ?? "" },
                            { "name2", chapter.Name2 ?? "" },
                            { "number1", chapter.Number1 ?? "" },
                            { "data", JsonConvert.SerializeObject(chapter.Data) }
                        });
                }
            }

            // Find missing shlokas in existing chapters
            var currentIds = currentShlokas.Select(s => s.Id).ToHashSet();
            var missingShlokas = completeShlokas.Where(s => !currentIds.Contains(s.Id)).ToList();
            Console.WriteLine($"Missing shlokas count: {missingShlokas.Count}");

            if (missingShlokas.Count > 0)
            {
                Console.WriteLine("Uploading missing shlokas to Firebase...");
                
                // Group missing shlokas by their correct language/chapter
                foreach (var language in completeLanguages)
                {
                    // Get the corresponding language in Firebase
                    var firebaseLanguage = currentLanguages.FirstOrDefault(l => l.Id == language.Id);
                    
                    if (firebaseLanguage != null)
                    {
                        // Find shlokas that should be in this language but are missing
                        var languageMissingShlokas = missingShlokas
                            .Where(s => language.Data.Any(originalShloka => originalShloka.Id == s.Id))
                            .ToList();
                        
                        if (languageMissingShlokas.Any())
                        {
                            Console.WriteLine($"Adding {languageMissingShlokas.Count} missing shlokas to chapter {language.Id}: {language.Name1}");
                            
                            // Add missing shlokas to the existing data
                            var updatedShlokas = firebaseLanguage.Data.ToList();
                            updatedShlokas.AddRange(languageMissingShlokas);
                            
                            // Update Firebase
                            await _db.Collection(_collectionName)
                                .Document(language.Id.ToString())
                                .UpdateAsync(new Dictionary<string, object>
                                {
                                    { "data", JsonConvert.SerializeObject(updatedShlokas.OrderBy(s => s.Id).ToList()) }
                                });
                        }
                    }
                }
                
                Console.WriteLine("Missing shlokas have been added to Firebase");
            }
            else
            {
                Console.WriteLine("No missing shlokas found. Firebase data is complete.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying and fixing shlokas: {ex.Message}");
            throw;
        }
    }

    public async Task FixChapterIdsAndMigrate(string jsonFilePath)
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

            Console.WriteLine("Original chapter IDs before fixing:");
            foreach (var lang in languages)
            {
                Console.WriteLine($"Chapter ID: {lang.Id}, Name: {lang.Name1}, Number: {lang.Number1}");
            }

            // Fix the chapter IDs - assign sequential numbers from 1-18
            for (int i = 0; i < languages.Count; i++)
            {
                languages[i].Id = i + 1;
                // If Number1 exists, use it as the chapter number
                if (string.IsNullOrEmpty(languages[i].Number1))
                {
                    languages[i].Number1 = (i + 1).ToString();
                }
            }

            Console.WriteLine("\nFixed chapter IDs:");
            foreach (var lang in languages)
            {
                Console.WriteLine($"Chapter ID: {lang.Id}, Name: {lang.Name1}, Number: {lang.Number1}");
            }

            // Save the fixed JSON back to file
            var fixedJsonPath = Path.Combine(Path.GetDirectoryName(jsonFilePath), "fixed_languages.json");
            await File.WriteAllTextAsync(fixedJsonPath, JsonConvert.SerializeObject(languages, Formatting.Indented));
            Console.WriteLine($"Fixed JSON saved to: {fixedJsonPath}");

            // Now upload the fixed data to Firebase
            Console.WriteLine("\nStarting upload of fixed data to Firebase...");
            
            // Clear existing data first
            var snapshot = await _db.Collection(_collectionName).GetSnapshotAsync();
            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }
            
            // Upload fixed chapters
            foreach (var language in languages)
            {
                Console.WriteLine($"Uploading chapter {language.Id}: {language.Name1} with {language.Data?.Count ?? 0} shlokas");
                var docRef = _db.Collection(_collectionName).Document(language.Id.ToString());
                
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "name1", language.Name1 ?? "" },
                    { "name2", language.Name2 ?? "" },
                    { "number1", language.Number1 ?? "" },
                    { "data", JsonConvert.SerializeObject(language.Data) }
                });
            }

            Console.WriteLine("Successfully uploaded all chapters with correct IDs to Firebase");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing chapter IDs: {ex.Message}");
            throw;
        }
    }
} 