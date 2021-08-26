using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ButtonUI : MonoBehaviour
{
    public TMP_Text progressText;
    public ButtonType buttonType;
    public static bool isClickAllowed;

    private void OnEnable()
    {
        progressText.text = "";
    }
    public void OnButtonClick()
    {
        if(buttonType == ButtonType.Random)
        {
            ConnectionManager.Instance.StartRandomPlayerMode();
        }
        else if (buttonType == ButtonType.Single)
        {
            ConnectionManager.Instance.StartSinglePlayerMode();
        }
        else if(buttonType == ButtonType.Friends)
        {

        }
    }
}

public enum ButtonType
{
    Single,
    Random,
    Friends
}
