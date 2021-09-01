using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    public static DataManager Instance
    {
        get
        {
            return instance;
        }
    }

    public PlayerData localPlayerData;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        localPlayerData = PlayerData.LoadData();
    }

    public void SavePlayerData()
    {
        PlayerData.SaveData(localPlayerData);
    }
}
