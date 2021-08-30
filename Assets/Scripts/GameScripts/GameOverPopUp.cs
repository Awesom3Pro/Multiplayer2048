using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class GameOverPopUp : MonoBehaviour
{
    public Transform panelTransform;
    public TMP_Text result;
    private bool moving = false;

    public void ShowGameOverPopup(bool won = false)
    {
        moving = true;

        panelTransform.gameObject.SetActive(true);

        result.text = won ? "You won" : "You Lost";

        panelTransform.DOMoveX(0, 0.4f).SetEase(Ease.OutBounce).OnComplete(() => { moving = false; });
    }

    public void BackToMenu()
    {
        if (!moving)
        {
            panelTransform.DOMoveX(8, 0.4f).SetEase(Ease.InBounce).OnComplete(() =>
            {
                ConnectionManager.Instance.LeaveRoom();

                panelTransform.gameObject.SetActive(false);
            });
        }
    }
}
