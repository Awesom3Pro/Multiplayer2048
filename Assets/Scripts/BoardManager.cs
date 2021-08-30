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

    public GameObject mergeAnim;

    public GameOverPopUp gameOver;

    public Transform deployButtonTransform;

    public Transform hpBar;

    public GameState state = GameState.None;

    public bool IsLoadingComplete;

    private List<Tile> emptyTiles = new List<Tile>();

    private List<Tile[]> columns = new List<Tile[]>();

    private List<Tile[]> rows = new List<Tile[]>();

    private Tile[,] multiTiles = new Tile[4, 4];

    private bool flag = false;

    public bool isSinglePlayer;

    private bool[] animationComplete = new bool[4] { true, true, true, true };

    [Range(0.0f, 10f)]
    public float delay = 0.05f;

    private int elapsedTime = 0;

    private float healthPercentage;

    private const float maxWidth = 3.3f;

    private int moveIndex;

    [SerializeField] TextMeshProUGUI timerText;

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
        if (!isSinglePlayer)
            PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        if (!isSinglePlayer)
            PhotonNetwork.RemoveCallbackTarget(this);
    }

    #region GameStart
    private void OnGameStart()
    {
        elapsedTime = 0;

        moveIndex = 0;

        healthPercentage = 100;

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

        StartCoroutine(OnSendingGameStart(struc, () =>
        {
            IsLoadingComplete = true;
            state = GameState.Playing;

        }));
    }
    #endregion
    public void Reset()
    {
        SceneManager.LoadScene(0);
    }

    public void Deploy()
    {
        if (IsLoadingComplete && ScoreTracker.Instance.IsAttackAllowed)
        {
            StartCoroutine(OnReceivedHealthUpdate(-25, () => { ScoreTracker.Instance.Deployed(); }));
        }
    }

    #region Generate
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
    #endregion

    #region TilesMovement/Merge

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

    private void ResetTileMoves()
    {
        foreach (Tile t in onedimTiles)
        {
            t.IsMerged = false;
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


                ShowMergeAnimation(tiles[i].tileTransform, deployButtonTransform);


                ScoreTracker.Instance.Score += tiles[i].Number;
                ScoreTracker.Instance.AttackRefill += tiles[i].Number;
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

                ShowMergeAnimation(tiles[i].tileTransform, deployButtonTransform);

                ScoreTracker.Instance.Score += tiles[i].Number;
                ScoreTracker.Instance.AttackRefill += tiles[i].Number;

                return true;
            }
        }
        return false;
    }
    #endregion

    #region InputCalculation
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
    #endregion

    #region Move Coroutines
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



        while (!(IsAnimationCompleted()))
        {
            yield return null;
        }

        MessageStruc struc = new MessageStruc();

        struc.lastDirection = dir;

        if (flag)
        {
            UpdateEmptyTiles();

            if (!isSinglePlayer)
                IsLoadingComplete = false;

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

    #endregion

    #region R&D
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
    #endregion

    #region Photon
    bool RaiseTileCreatedEvent(MessageStruc tileStruc)
    {
        if (!isSinglePlayer)
        {
            object struc = JsonConvert.SerializeObject(tileStruc);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(Constants.TileCreatedEventCode, struc, raiseEventOptions, SendOptions.SendReliable);
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

    private bool RaiseAttackEvent(float damageToDeal)
    {
        if (!isSinglePlayer)
        {
            object x = (float)damageToDeal;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(Constants.OnGotAttacked, x, raiseEventOptions, SendOptions.SendReliable);
        }
        return false;
    }

    private bool RaiseHealthUpdatedEvent(float hp)
    {
        if (!isSinglePlayer)
        {
            object x = (float)hp;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            return PhotonNetwork.RaiseEvent(Constants.OnGotAttackedReceived, x, raiseEventOptions, SendOptions.SendReliable);
        }
        return false;
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Constants.OnGameOverCode:
                Debug.LogError("Getting Move Command");
                int tileValue = (int)photonEvent.CustomData;
                OnGameOver(tileValue);
                break;
            case Constants.OnGotAttacked:
                Debug.LogError("Opponent Game Initiated");
                float x = ((float)photonEvent.CustomData);
                UpdateHealth(x);
                break;
            case Constants.OnTileCreatedEventReceivedCode:
                Debug.LogError("Received handshake from Opponent");
                IsLoadingComplete = true;
                break;
            case Constants.OnGameStartReceivedCode:
                Debug.LogError("Received Game Started in Opponent");
                IsLoadingComplete = true;
                break;
            case Constants.OnGotAttackedReceived:
                Debug.LogError("Received");
                IsLoadingComplete = true;
                break;
        }
    }

    private IEnumerator OnSendingGameStart(GameStartStruc structure, Action action)
    {
        IsLoadingComplete = false;

        RaiseOnGameStartedEvent(structure);

        loader.OnLoadStart();

        if (!isSinglePlayer)
        {
            while (!IsLoadingComplete)
            {
                yield return null;
            }
        }

        loader.OnLoadQuit(() =>
        {
            action?.Invoke();
        });
    }

    private IEnumerator OnReceivedHealthUpdate(float dmg, Action action)
    {
        IsLoadingComplete = false;

        RaiseAttackEvent(dmg);

        loader.OnLoadStart();

        if (!isSinglePlayer)
        {
            while (!IsLoadingComplete)
            {
                yield return null;
            }
        }

        loader.OnLoadQuit(() =>
        {
            action?.Invoke();
        });
    }
    #endregion
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

        gameOver.ShowGameOverPopup(num == 1);
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

    private void ShowMergeAnimation(Transform intialT, Transform destinationT)
    {
        GameObject x = Instantiate(mergeAnim);
        x.transform.position = intialT.position;
        SpriteRenderer z = x.GetComponent<SpriteRenderer>();
        //z.DOColor(new Color(1, 1, 1, 0), 0.25f);
        x.transform.DOMove(destinationT.position, 0.25f).OnComplete(() =>
      {
          ScoreTracker.Instance.UpdateFillBar();
          Destroy(x);
      });
    }

    private void UpdateHealth(float healthValueDeduction)
    {
        healthPercentage = healthPercentage + healthValueDeduction;

        hpBar.DOKill(true);

        hpBar.DOScaleX((healthPercentage / 100) * 3.3f, 0.3f);

        hpBar.transform.localScale = new Vector3(Mathf.Clamp(hpBar.transform.localScale.x, 0, 3.3f), hpBar.transform.localScale.y, 0);

        RaiseHealthUpdatedEvent(healthPercentage);

        if (healthPercentage <= 0)
        {
            OnGameOver(0);
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
