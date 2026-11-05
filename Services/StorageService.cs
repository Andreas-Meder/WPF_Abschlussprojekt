using Passwortspeicher.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Passwortspeicher.Services;

public class StorageService
{
    private const string FilePath = "benutzer.json";

    public List<Benutzer> LoadUsers()
    {
        if (!File.Exists(FilePath))
            return new List<Benutzer>();

        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<Benutzer>>(json) ?? new List<Benutzer>();
    }

    public void SaveUsers(List<Benutzer> mehereBenutzer)
    {
        string json = JsonSerializer.Serialize(mehereBenutzer, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}