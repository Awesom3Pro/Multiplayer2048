using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreTracker : MonoBehaviour
{
    private static ScoreTracker instance;

    public static ScoreTracker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScoreTracker>();
            }

            return instance;
        }
    }

    private int score = 0;

    private int highscore;

    private int fillScore = 0;

    public TMP_Text scoreLabel;

    public bool IsAttackAllowed
    {
        get
        {
            return fillScore >= 800;
        }
    }

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;

            if (highscore < score)
            {
                highscore = score;
            }
            scoreLabel.text = score.ToString();
        }
    }

    public int AttackRefill
    {
        get
        {
            return fillScore;
        }

        set
        {
            fillScore = value;
        }
    }

    public Transform energyScale;

    private const float maxWidth = 1;

    private void Start()
    {
        UpdateFillBar();
    }
    public void UpdateFillBar()
    {
        energyScale.DOScaleY((fillScore / 800.0f) * maxWidth, 0.2f).SetEase(Ease.OutBounce);
    }

    public void Deployed()
    {
        AttackRefill = 0;

        UpdateFillBar();
    }
}
