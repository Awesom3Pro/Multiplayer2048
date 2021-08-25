using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileFormat
{
    public int number;
    public Color faceColor;
    public Color numberColor;
    TileFormat()
    {
        faceColor = new Color(1, 1, 1, 1);
        numberColor = new Color(1, 1, 1, 1);
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
