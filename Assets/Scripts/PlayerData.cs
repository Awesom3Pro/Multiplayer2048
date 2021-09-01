using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    const string KEY_PLAYER_DATA = "pData";
    const string EDITOR_EXTENSION = "_EDITOR";
    public string playerName;
    public int matchesPlayed;
    public int matchesWon;

    public PlayerData()
    {
        playerName = "";
        matchesPlayed = 0;
        matchesWon = 0;
    }

    static string GetCSVData(PlayerData playerData)
    {
        return string.Format("{0},{1},{2}", playerData.playerName, playerData.matchesPlayed, playerData.matchesWon);
    }

    static PlayerData GetPlayerDataFromCSVString(string csvString)
    {
        string[] data = csvString.Split(',');

        int mPlayed = int.Parse(data[1]);
        int mWon = int.Parse(data[2]);

        return new PlayerData() { matchesPlayed = mPlayed, matchesWon = mWon, playerName = data[0] };
    }

    public static void SaveData(PlayerData playerData)
    {
        string key = KEY_PLAYER_DATA;
#if UNITY_EDITOR
        key += EDITOR_EXTENSION;
#endif
        PlayerPrefs.SetString(key, GetCSVData(playerData));
    }

    public static PlayerData LoadData()
    {
        string defaultPlayerName = SystemInfo.deviceUniqueIdentifier;
        string key = KEY_PLAYER_DATA;
#if UNITY_EDITOR
        defaultPlayerName += EDITOR_EXTENSION;
        key += EDITOR_EXTENSION;
#endif
        string data = PlayerPrefs.GetString(key, GetCSVData(new PlayerData() { playerName = defaultPlayerName}));
        return GetPlayerDataFromCSVString(data);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/2048/Clear Editor Related Player Prefs")]
    public static void ClearEditorRelatedPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(KEY_PLAYER_DATA + EDITOR_EXTENSION);
    }
#endif
}
