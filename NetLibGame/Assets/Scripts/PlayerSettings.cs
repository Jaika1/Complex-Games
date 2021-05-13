using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSettings
{
    private static string saveLocation = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\My Games\Jaika1\Werewolf\";
    private static string fileName = "playerConfig.json";
    private static PlayerSettings instance = null;

    public static string SaveLocation => saveLocation;
    public static string FileName => fileName;
    public static string FullPath => saveLocation + fileName;
    public static PlayerSettings Instance => instance;

    public static void LoadConfig()
    {
        if (File.Exists(FullPath))
        {
            instance = JsonUtility.FromJson<PlayerSettings>(File.ReadAllText(FullPath));
        }
        else
        {
            instance = new PlayerSettings();
            instance.SaveConfig();
        }
    }

    public void SaveConfig()
    {
        if (!Directory.Exists(SaveLocation))
            Directory.CreateDirectory(SaveLocation);

        File.WriteAllText(FullPath, JsonUtility.ToJson(this, true));
    }

    // ACTUAL FIELDS

    public string PlayerName = $"Player{UnityEngine.Random.Range(1000, 9999)}";
}
