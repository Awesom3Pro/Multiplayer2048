using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;
using Newtonsoft.Json;

public class BoardManager : MonoBehaviour, IOnEventCallback
{
    [Header("References")]
    public List<Tile> onedimTiles = new List<Tile>();

    public LoadingBoard loader;

    private List<Tile> emptyTiles = new List<Tile>();

    private List<Tile[]> columns = new List<Tile[]>();

    private List<Tile[]> rows = new List<Tile[]>();

    private Tile[,] multiTiles = new Tile[4, 4];

    public GameState state = GameState.None;

    private bool flag = false;

    public bool isSinglePlayer;

    public bool IsLoadingComplete;

    private bool[] animationComplete = new bool[4] { true, true, true, true };

    const byte TileCreatedEventCode = 1;

    [Range(0.0f, 10f)]
    public float delay = 0.05f;

    int elapsedTime = 0;

    private int moveIndex;

    [SerializeField] TextMeshProUGUI timerText;

    private void Awake()
    {
       
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("MODE") == 0)
        {
            isSinglePlayer = true;
        }
        InputManager.Instance.OnTouchReceived += ButtonPressed;

        OnGameStart();
    }

    void OnEnable()
    {
        if(!isSinglePlayer)
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        if(!isSinglePlayer)
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnGameStart()
    {
        elapsedTime = 0;

        moveIndex = 0;

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

        GenerateTileStruc gt1 = Generate();

        GenerateTileStruc gt2 = Generate();

        int[] tileIndexes = new int[] { gt1.TileIndex, gt2.TileIndex };

        int[] tileValues = new int[] { gt1.TileNumber, gt2.TileNumber };

        GameStartStruc struc = new GameStartStruc();
        struc.tileIndex = tileIndexes;
        struc.tileValues = tileValues;
        RaiseOnGameStartedEvent(struc);
        IsLoadingComplete = true;
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

                ScoreTracker.Instance.Score += tiles[i].Number;

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
        if (state == GameState.Playing && IsLoadingComplete)
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

        if(!isSinglePlayer)
        IsLoadingComplete = false;

        while (!(IsAnimationCompleted()))
        {
            yield return null;
        }

        MessageStruc struc = new MessageStruc();

        struc.lastDirection = dir;

        if (flag)
        {
            UpdateEmptyTiles();

            GenerateTileStruc tileStruc = Generate();

            struc.lastStruc = tileStruc;

            struc.moveIndex = moveIndex;

            if (!RaiseTileCreatedEvent(struc))
            {
                Debug.LogError("Event couldn't be sent");
            }
        }

        if (!isSinglePlayer)
        {
            loader.OnLoadStart();

            while (!IsLoadingComplete)
            {
                yield return null;
            }

            loader.OnLoadQuit(() =>
            {

                if (!CanMove())
                {
                    state = GameState.GameOver;

                    OnGameOver(0);
                }
                else
                {
                    state = GameState.Playing;
                }

            });
        }
        else
        {
            if (!CanMove())
            {
                state = GameState.GameOver;

                RaiseGameOver();

                OnGameOver(0);
            }
            else
            {
                state = GameState.Playing;
            }
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            Shuffle();
        }

        UpdateTime();
    }

    bool RaiseTileCreatedEvent(MessageStruc tileStruc)
    {
        if (!isSinglePlayer)
        {
            object struc = JsonConvert.SerializeObject(tileStruc);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(TileCreatedEventCode, struc, raiseEventOptions, SendOptions.SendReliable);
        }

        return false;
    }

    bool RaiseOnGameStartedEvent(GameStartStruc gameStruc)
    {
        if (!isSinglePlayer)
        {
            Debug.LogError("Raised Game Started Event");
            object struc = JsonConvert.SerializeObject(gameStruc);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(Constants.OnGameStartEventCode, struc, raiseEventOptions, SendOptions.SendReliable);
        }
        return false;
    }

    bool RaiseGameOver()
    {
        if (!isSinglePlayer)
        {
            object x = (int)1;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(Constants.OnGameOverCode, x, raiseEventOptions, SendOptions.SendReliable);
        }

        return false;
    }

    private void OnGameOver(int num)
    {
        if (num == 1)
        {
            Debug.LogError("You won");
        }
        else
        {
            RaiseGameOver();
            Debug.LogError("Opponent Wins");
        }
    }

    void UpdateTime()
    {
        int timeSinceStart = (int)(PhotonNetwork.Time - ConnectionManager.GameStartTime);
        if (timeSinceStart != elapsedTime)
        {
            elapsedTime = timeSinceStart;
            UpdateTimerText(elapsedTime);
        }
    }

    void UpdateTimerText(int seconds)
    {
        int minutes = seconds / 60;
        seconds %= 60;

        timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    public void OnEvent(EventData photonEvent)
    {

        switch (photonEvent.Code)
        {
            case Constants.OnGameOverCode:
                Debug.LogError("Getting Move Command");
                int tileValue = (int)photonEvent.CustomData;

                break;
            case Constants.OnOpponentTileMergeCode:
                Debug.LogError("Opponent Game Initiated");
                int x = ((int)photonEvent.CustomData);
                OnGameOver(x);
                break;
            case Constants.OnTileCreatedEventReceivedCode:
                Debug.LogError("Received handshake from Opponent");
                IsLoadingComplete = true;
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
