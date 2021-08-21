using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class BoardManager : MonoBehaviour, IOnEventCallback
{
    public List<Tile> onedimTiles = new List<Tile>();

    private List<Tile> emptyTiles = new List<Tile>();

    private List<Tile[]> columns = new List<Tile[]>();

    private List<Tile[]> rows = new List<Tile[]>();

    private Tile[,] multiTiles = new Tile[4, 4];

    public GameState state = GameState.None;

    private bool flag = false;

    private bool[] animationComplete = new bool[4] { true, true, true, true };

    const byte TileCreatedEventCode = 1;
    
    [Range(0.0f, 10f)]
    public float delay = 0.05f;

    private void Awake()
    {
        InputManager.Instance.OnTouchReceived += ButtonPressed;
    }

    private void Start()
    {
        OnGameStart();
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnGameStart()
    {
        ScoreTracker.Instance.Score = 0;

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

        Generate();
        Generate();

        state = GameState.Playing;
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }
    private void Generate()
    {
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
            emptyTiles[x].Number = randomNum;

            emptyTiles[x].tileTransform.localScale = new Vector2(0.2f, 0.2f);

            emptyTiles[x].tileTransform.DOScale(new Vector2(1, 1), 0.25f);

            emptyTiles.RemoveAt((x));
        }
        else
        {
            Debug.LogError("Out of Empty Tiles");
        }
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

                ScoreTracker.Instance.Score += tiles[i].Number;

                RaiseTileCreatedEvent(tiles[i].Number);

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

                ScoreTracker.Instance.Score += tiles[i].Number;

                RaiseTileCreatedEvent(tiles[i].Number);

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
    private void ButtonPressed(Direction direction)
    {
        if (state == GameState.Playing)
        {
            ResetTileMoves();

            flag = false;

            if (delay > 0)
            {
                StartCoroutine(MoveCoroutine(direction));
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
    }

    private IEnumerator MakeOneLineMoveUp(Tile[] tiles, int index)
    {
        animationComplete[index] = false;

        while(MakeOneMoveUp(tiles))
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

    private IEnumerator MoveCoroutine(Direction dir)
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

        if(flag)
        {
            UpdateEmptyTiles();
            Generate();
        }

        if (!CanMove())
        {
            // Ask for Shuffle Popup

            Debug.LogError("GameOver");
        }
        else
        {
            state = GameState.Playing;
        }

    }

    public void Shuffle()
    {
        StartCoroutine(ShuffleCoroutine());
    }

    private IEnumerator ShuffleCoroutine()
    {
        state = GameState.Waiting;

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
        state = GameState.Playing;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Shuffle();
        }
    }

    void RaiseTileCreatedEvent(int tileVlaue)
    {
        object content = tileVlaue;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(TileCreatedEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch(photonEvent.Code)
        {
            case TileCreatedEventCode:
                int tileValue = (int)photonEvent.CustomData;
                Debug.LogErrorFormat("Tile Created Event Received. Sender {0} Tile Value {1}", photonEvent.Sender, tileValue);
                break;
        }
    }
}
public enum GameState
{
    None,
    Playing,
    Waiting,
    GameOver
}
