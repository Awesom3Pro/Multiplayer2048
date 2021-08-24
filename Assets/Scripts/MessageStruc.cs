using System.Collections.Generic;

[System.Serializable]
public class MessageStruc
{
    public GenerateTileStruc lastStruc;
    public Direction lastDirection;
}

[System.Serializable]
public class GameStartStruc
{
    public int[] tileIndex;
    public int[] tileValues;
}
