using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    private int number;

    public int Number
    {
        get
        {
            return number;
        }

        set
        {
            number = value;

            if (number == 0)
            {
                SetVisible(false);
            }
            else
            {
                ApplyStyle(number);
                SetVisible(true);
            }
        }
    }

    public int indRow;
    public int indColumn;

    public bool IsMerged { get; set; }

    public SpriteRenderer spriteRenderer;
    public Transform tileTransform;

    private void Awake()
    {
        tileTransform = spriteRenderer.gameObject.GetComponent<Transform>();
    }

    private void ApplyStyleIndex(int index)
    {
        spriteRenderer.sprite = TileStyles.Instance.tileFormats[index].faceSprite;
    }

    public void ApplyStyle(int num)
    {
        switch (num)
        {
            case 2:
                ApplyStyleIndex(0);
                break;
            case 4:
                ApplyStyleIndex(1);
                break;
            case 8:
                ApplyStyleIndex(2);
                break;
            case 16:
                ApplyStyleIndex(3);
                break;
            case 32:
                ApplyStyleIndex(4);
                break;
            case 64:
                ApplyStyleIndex(5);
                break;
            case 128:
                ApplyStyleIndex(6);
                break;
            case 256:
                ApplyStyleIndex(7);
                break;
            case 512:
                ApplyStyleIndex(8);
                break;
            case 1024:
                ApplyStyleIndex(9);
                break;
            case 2048:
                ApplyStyleIndex(10);
                break;
            case 4096:
                ApplyStyleIndex(11);
                break;
            default:
                Debug.LogError(string.Format("{0}  was passed as number", num));
                break;
        }
    }

    private void SetVisible(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
    }
}
