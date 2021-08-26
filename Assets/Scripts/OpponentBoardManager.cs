using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpponentBoardManager : MonoBehaviour, IOnEventCallback
{
    public List<Tile> onedimTiles = new List<Tile>();

    private List<Tile> emptyTiles = new List<Tile>();

    private List<Tile[]> columns = new List<Tile[]>();

    private List<Tile[]> rows = new List<Tile[]>();

    private Tile[,] multiTiles = new Tile[4, 4];

    private bool flag = false;

    private bool[] animationComplete = new bool[4] { true, true, true, true };

    private GameState state;

    [Range(0.0f, 10f)]
    public float delay = 0.05f;

    private List<MessageStruc> queueMoves = new List<MessageStruc>();

    public GameObject opponentBoard;

    private bool isSinglePlayer;

    void OnEnable()
    {
        if (PlayerPrefs.GetInt("MODE") == 0)
        {
            isSinglePlayer = true;

            opponentBoard.gameObject.SetActive(false);
        }

        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnGameStart(int[] tileIndexes, int[] tileValue)
    {
        OpponentScoreTracker.Instance.Score = 0;

        for (int i = 0; i < onedimTiles.Count; i++)
        {
            Tile t = onedimTiles[i];

            t.Number = 0;

            t.indColumn = i % 4;
            t.indRow = i / 4;

            multiTiles[i / 4, i % 4] = t;

            t.gameObject.name = String.Format("({0},{1})", t.indRow, t.indColumn);

            emptyTiles.Add(t);
        }

        for (int i = 0; i < 4; i++)
        {
            List<Tile> col = new List<Tile>();
            List<Tile> row = new List<Tile>();

            for (int j = 0; j < 4; j++)
            {
                col.Add(multiTiles[j, i]);
                row.Add(multiTiles[i, j]);
            }
            rows.Add(row.ToArray());

            columns.Add(col.ToArray());
        }

        Generate(tileValue[0], tileIndexes[0]);
        Generate(tileValue[1], tileIndexes[1]);

        state = GameState.Playing;
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }
    private GenerateTileStruc Generate(int z = -1, int tile = -1)
    {
        GenerateTileStruc struc = new GenerateTileStruc();

        if (emptyTiles.Count > 0)
        {
            int x = UnityEngine.Random.Range(0, emptyTiles.Count); ;
            int randomNum = UnityEngine.Random.Range(0, 10);

            if (randomNum >= 8)
            {
                randomNum = 4;
            }
            else
            {
                randomNum = 2;
            }
            randomNum = z == -1 ? randomNum : z;

            x = tile == -1 ? x : tile;

            emptyTiles[x].Number = randomNum;

            emptyTiles[x].tileTransform.localScale = new Vector2(0.2f, 0.2f);

            emptyTiles[x].tileTransform.DOScale(new Vector2(1, 1), 0.25f);

            emptyTiles.RemoveAt((x));

            struc.TileNumber = randomNum;

            struc.TileIndex = x;
        }
        else
        {
            Debug.LogError("Out of Empty Tiles");
        }

        return struc;
    }

    private bool MakeOneMoveDown(Tile[] tiles)
    {
        for (int i = 0; i < tiles.Length - 1; i++)
        {
            if (tiles[i].Number == 0 && tiles[i + 1].Number != 0)
            {
                tiles[i].Number = tiles[i + 1].Number;

                tiles[i + 1].Number = 0;

                return true;
            }

            if (tiles[i].Number == tiles[i + 1].Number && !tiles[i].IsMerged && !tiles[i + 1].IsMerged && tiles[i].Number != 0)
            {
                tiles[i].Number *= 2;

                tiles[i + 1].Number = 0;

                tiles[i].IsMerged = true;

                tiles[i].tileTransform.localScale = new Vector2(0.2f, 0.2f);

                tiles[i].tileTransform.DOScale(new Vector2(1, 1), 0.25f);

                OpponentScoreTracker.Instance.Score += tiles[i].Number;

                return true;
            }
        }
        return false;
    }
    private bool MakeOneMoveUp(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            if (tiles[i].Number == 0 && tiles[i - 1].Number != 0)
            {
                tiles[i].Number = tiles[i - 1].Number;

                tiles[i - 1].Number = 0;

                return true;
            }

            if (tiles[i].Number == tiles[i - 1].Number && !tiles[i].IsMerged && !tiles[i - 1].IsMerged && tiles[i].Number != 0 && tiles[i].Number != 11)
            {
                tiles[i].Number *= 2;

                tiles[i - 1].Number = 0;

                tiles[i].IsMerged = true;

                tiles[i].tileTransform.localScale = new Vector2(0.2f, 0.2f);

                tiles[i].tileTransform.DOScale(new Vector2(1, 1), 0.25f);

                OpponentScoreTracker.Instance.Score += tiles[i].Number;

                return true;
            }
        }
        return false;
    }

    private void ResetTileMoves()
    {
        foreach (Tile t in onedimTiles)
        {
            t.IsMerged = false;
        }
    }

    private void UpdateEmptyTiles()
    {
        emptyTiles.Clear();

        for (int x = 0; x < onedimTiles.Count; x++)
        {
            if (onedimTiles[x].Number == 0)
            {
                emptyTiles.Add(onedimTiles[x]);
            }
        }
    }

    private bool CanMove()
    {
        if (emptyTiles.Count > 0)
        {
            return true;
        }

        for (int i = 0; i < columns.Count; i++)
        {
            for (int j = 0; j < rows.Count - 1; j++)
            {
                if (multiTiles[j, i].Number == multiTiles[j + 1, i].Number)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < columns.Count - 1; j++)
            {
                if (multiTiles[i, j].Number == multiTiles[i, j + 1].Number)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void ButtonPressed(Direction direction, int tileIndex, int tileValue)
    {
        ResetTileMoves();

        flag = false;

        if (delay > 0)
        {
            StartCoroutine(MoveCoroutine(direction, tileIndex, tileValue));
        }
        else
        {

            for (int i = 0; i < rows.Count; i++)
            {
                switch (direction)
                {
                    case Direction.LEFT:
                        while (MakeOneMoveDown(rows[i]))
                        {
                            flag = true;
                        }
                        break;
                    case Direction.RIGHT:
                        while (MakeOneMoveUp(rows[i]))
                        {
                            flag = true;
                        }
                        break;
                    case Direction.DOWN:
                        while (MakeOneMoveUp(columns[i]))
                        {
                            flag = true;
                        }
                        break;
                    case Direction.UP:
                        while (MakeOneMoveDown(columns[i]))
                        {
                            flag = true;
                        }
                        break;
                }
            }

            if (flag)
            {

                UpdateEmptyTiles();
                Generate();

                if (!CanMove())
                {
                    // Ask shuffle
                }
            }
        }
    }

    private IEnumerator MakeOneLineMoveUp(Tile[] tiles, int index)
    {
        animationComplete[index] = false;

        while (MakeOneMoveUp(tiles))
        {
            flag = true;

            yield return new WaitForSeconds(delay);
        }

        animationComplete[index] = true;
    }

    private IEnumerator MakeOneLineMoveDown(Tile[] tiles, int index)
    {
        animationComplete[index] = false;

        while (MakeOneMoveDown(tiles))
        {
            flag = true;

            yield return new WaitForSeconds(delay);
        }

        animationComplete[index] = true;
    }

    private IEnumerator MoveCoroutine(Direction dir, int tileIndex, int tileValue)
    {
        state = GameState.Waiting;

        switch (dir)
        {
            case Direction.DOWN:
                for (int i = 0; i < columns.Count; i++)
                {
                    StartCoroutine(MakeOneLineMoveUp(columns[i], i));
                }
                break;
            case Direction.LEFT:
                for (int i = 0; i < rows.Count; i++)
                {
                    StartCoroutine(MakeOneLineMoveDown(rows[i], i));
                }
                break;
            case Direction.RIGHT:
                for (int i = 0; i < rows.Count; i++)
                {
                    StartCoroutine(MakeOneLineMoveUp(rows[i], i));
                }
                break;
            case Direction.UP:
                for (int i = 0; i < columns.Count; i++)
                {
                    StartCoroutine(MakeOneLineMoveDown(columns[i], i));
                }
                break;
        }

        while (!(IsAnimationCompleted()))
        {
            yield return null;
        }

        if (flag)
        {
            UpdateEmptyTiles();

            Generate(tileValue, tileIndex);
        }

        queueMoves.RemoveAt(0);

        if (CanMove())
        {
            PlayQueue();
            state = GameState.Playing;
        }
    }

    public void Shuffle()
    {
        StartCoroutine(ShuffleCoroutine());
    }

    private IEnumerator ShuffleCoroutine()
    {
        for (int x = 0; x < onedimTiles.Count; x++)
        {
            for (int y = x + 1; y < onedimTiles.Count; y++)
            {
                if (onedimTiles[x].Number > onedimTiles[y].Number)
                {
                    int num = onedimTiles[y].Number;

                    onedimTiles[y].Number = onedimTiles[x].Number;

                    onedimTiles[x].Number = num;

                    yield return new WaitForSeconds(delay);
                }
            }
        }

        int start = 8;
        int end = 11;
        while (start < end)
        {
            int temp = onedimTiles[start].Number;
            onedimTiles[start].Number = onedimTiles[end].Number;
            onedimTiles[end].Number = temp;
            start++;
            end--;
            yield return new WaitForSeconds(delay);
        }
        start = 0;
        end = 3;
        while (start < end)
        {
            int temp = onedimTiles[start].Number;
            onedimTiles[start].Number = onedimTiles[end].Number;
            onedimTiles[end].Number = temp;
            start++;
            end--;
            yield return new WaitForSeconds(delay);
        }
    }

    private bool IsAnimationCompleted()
    {
        bool complete = true;

        foreach (bool j in animationComplete)
        {
            if (j == false)
            {
                complete = j;
                break;
            }
        }

        return complete;
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Constants.TileCreatedEventCode:
                Debug.LogError("Getting Move Command");
                MessageStruc tileValue = JsonConvert.DeserializeObject<MessageStruc>((string)photonEvent.CustomData);
                queueMoves.Add(tileValue);
                PlayQueue();
                break;
            case Constants.OnGameStartEventCode:
                Debug.LogError("Opponent Game Initiated");
                GameStartStruc startValues = JsonConvert.DeserializeObject<GameStartStruc>((string)photonEvent.CustomData);
                OnGameStart(startValues.tileIndex, startValues.tileValues);
                break;

        }
    }

    private void PlayQueue()
    {
        if (state == GameState.Playing)
        {
            if (queueMoves.Count > 0)
            {
                ButtonPressed(queueMoves[0].lastDirection, queueMoves[0].lastStruc.TileIndex, queueMoves[0].lastStruc.TileNumber);
            }
        }
    }
}
