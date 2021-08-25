using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameOverPanel : MonoBehaviour
{
    public TMP_Text gameOverMessageText;

    public Transform gameOverPanel;

    public void ShowGameOverText(string message)
    {
        gameOverMessageText.text = message;

        gameOverPanel.gameObject.SetActive(true);

        gameOverPanel.transform.localScale = Vector3.zero;

        gameOverPanel.DOScale(1, 0.25f);
    }

    public void RemoveGameOver()
    {
        gameOverPanel.transform.localScale = Vector3.zero;

        gameOverPanel.gameObject.SetActive(false);
    }
}