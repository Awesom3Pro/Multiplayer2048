using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileFormat
{
    public int number;
    public Sprite faceSprite;

    TileFormat()
    {

    }
}
public class TileStyles : MonoBehaviour
{
    private static TileStyles instance;

    public static TileStyles Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<TileStyles>();
            }

            return instance;
        }
    }
    public List<TileFormat> tileFormats = new List<TileFormat>();

}
