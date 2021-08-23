using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpponentScoreTracker : MonoBehaviour
{
    private static OpponentScoreTracker instance;

    public static OpponentScoreTracker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OpponentScoreTracker>();
            }

            return instance;
        }
    }

    private int score = 0;

    public TMP_Text scoreLabel;

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;

            scoreLabel.text = score.ToString();
        }
    }

    private void Awake()
    {
        // retrieve highscore and score
    }
}
