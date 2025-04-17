using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using System;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Microsoft.Extensions.Configuration;

public class FirebaseService
{
    private readonly FirestoreDb _db;
    private readonly string _collectionName = "languages";
    private readonly string _projectId;

    public FirebaseService(IConfiguration configuration)
    {
        _projectId = configuration["Firebase:ProjectId"] ?? throw new ArgumentNullException("Firebase:ProjectId is not configured");
        
        // Initialize Firebase with credentials
        try
        {
            string credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            
            // If environment variable is not set, use local path
            if (string.IsNullOrEmpty(credentialsPath))
            {
                credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials.json");
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            }
            
            // Verify credentials file exists
            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException($"Firebase credentials file not found at: {credentialsPath}");
            }
            
            var credential = GoogleCredential.FromFile(credentialsPath);
            
            // Check if FirebaseApp is already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _projectId
                });
            }
            
            _db = FirestoreDb.Create(_projectId);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to initialize Firebase. Please ensure firebase-credentials.json is in the project root or GOOGLE_APPLICATION_CREDENTIALS environment variable is set correctly.", ex);
        }
    }

    public async Task<List<Language>> GetAllLanguages()
    {
        try
        {
            var snapshot = await _db.Collection(_collectionName).GetSnapshotAsync();
            var languages = new List<Language>();
            
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var language = new Language
                {
                    Id = int.Parse(doc.Id),
                    Name1 = data["name1"].ToString(),
                    Name2 = data["name2"].ToString(),
                    Data = JsonConvert.DeserializeObject<List<Shloka>>(data["data"].ToString() ?? "[]") ?? new List<Shloka>()
                };
                languages.Add(language);
            }

            return languages.OrderBy(l => l.Id).ToList();
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error fetching languages: {ex.Message}");
            return new List<Language>();
        }
    }

    public async Task<Shloka?> GetShlokaById(int id)
    {
        var languages = await GetAllLanguages();
        return languages.SelectMany(l => l.Data)
                       .FirstOrDefault(s => s.Id == id);
    }

    public async Task UpdateShloka(Shloka shloka)
    {
        try
        {
            var languages = await GetAllLanguages();
            foreach (var language in languages)
            {
                var index = language.Data.FindIndex(s => s.Id == shloka.Id);
                if (index != -1)
                {
                    language.Data[index] = shloka;
                    await _db.Collection(_collectionName)
                          .Document(language.Id.ToString())
                          .UpdateAsync(new Dictionary<string, object>
                          {
                              { "data", JsonConvert.SerializeObject(language.Data) }
                          });
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating shloka: {ex.Message}");
        }
    }

    public async Task<Shloka?> ShowShloka(int id)
    {
        var languages = await GetAllLanguages();
        var shloka = languages.SelectMany(l => l.Data)
                            .FirstOrDefault(s => s.Id == id);
        
        if (shloka != null)
        {
            var language = languages.FirstOrDefault(l => l.Data.Any(s => s.Id == id));
            if (language != null)
            {
                await _db.Collection(_collectionName)
                      .Document(language.Id.ToString())
                      .UpdateAsync(new Dictionary<string, object>
                      {
                          { "data", JsonConvert.SerializeObject(language.Data) }
                      });
            }
        }
        
        return shloka;
    }
} 