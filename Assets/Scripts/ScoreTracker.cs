using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

            if (highscore < score)
            {
                highscore = score;
                // Update Text
                //Save
            }
            scoreLabel.text = score.ToString();
            Debug.Log(score);
        }
    }

    private void Awake()
    {
        // retrieve highscore and score
    }
}
