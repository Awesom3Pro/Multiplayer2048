using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class LoadingBoard : MonoBehaviour
{

    public GameObject loadingUI;
    public Transform sprite;
    public SpriteRenderer spriteRender;
    public Color fadeInColor;
    private Color fadeOutColor;

    private void Awake()
    {
        fadeOutColor = new Color(fadeInColor.r, fadeInColor.g, fadeInColor.b, 0);
    }

    public void OnLoadStart()
    {
        loadingUI.SetActive(true);

        sprite.rotation = Quaternion.identity;

        spriteRender.DOColor(fadeInColor, 0.3f).OnComplete(() =>
        {
            sprite.DORotate(new Vector3(180, 0, 0), 0.4f).SetLoops(-1, LoopType.Yoyo);
        });
    }

    public void OnLoadQuit(Action callback)
    {
        sprite.DOKill(true);
        spriteRender.DOColor(fadeOutColor, 0.2f).OnComplete(() => { callback?.Invoke();
            loadingUI.SetActive(false);
        });
    }
}
