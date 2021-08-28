using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

public class LoadingUI : MonoBehaviour
{
    public Image loadImage;
    public CanvasGroup buttonLayout;
    public CanvasGroup loadingUI;

    private void OnEnable()
    {
        ConnectionManager.Instance.OnSingleButtonClicked += SingleMode;
        ConnectionManager.Instance.OnMultiplayerButtonClicked += MultiplayerMode;

    }

    private void OnDisable()
    {
        ConnectionManager.Instance.OnSingleButtonClicked -= SingleMode;
        ConnectionManager.Instance.OnMultiplayerButtonClicked -= MultiplayerMode;
    }

    private void MultiplayerMode()
    {
        StartLoading(() => { JoinedRoom();
            ConnectionManager.Instance.OnRoomJoinedInvoked += JoinedRoom;
            ConnectionManager.Instance.OnButtonClick();
        });
    }

    private void SingleMode()
    {
        StartLoading(() =>
        {
            loadImage.DOFillAmount(1, 0.2f).OnComplete(() =>
                {
                    ConnectionManager.Instance.LoadSinglePlayer();
                });
        });
    }

    private void JoinedRoom()
    {
        ConnectionManager.Instance.OnRoomJoinedInvoked -= JoinedRoom;
        ConnectionManager.Instance.OnGameToLoad += Gameloading;
        loadImage.DOFillAmount(0.35f, 0.1f);
        //DOVirtual.DelayedCall(0.1f, () =>
        //{
        //    loadImage.DOFillAmount(0.9f, ConnectionManager.Instance.roomWaitTimeOut);
        //});
    }

    private void Gameloading()
    {
        ConnectionManager.Instance.OnGameToLoad -= Gameloading;
        loadImage.DOFillAmount(1, 0.1f);
    }

    public void StartLoading(Action action)
    {
        loadImage.fillAmount = 0;

        buttonLayout.interactable = false;

        buttonLayout.DOFade(0, 0.5f).OnComplete(() =>
        {
            loadingUI.DOFade(1, 0.3f).OnComplete(() =>
          {
              action?.Invoke();
          });
        });
    }
}
