using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DamageAnalyzer : MonoBehaviour
{
    public Image fillBar;

    private Sequence m_Sequence;

    public TextMeshProUGUI percentText;

    private void OnEnable()
    {
        StartAnimation();
    }

    public void StartAnimation()
    {
        fillBar.fillAmount = 0.1f;

        if (m_Sequence != null)
        {
            m_Sequence.Kill();
        }
        m_Sequence = DOTween.Sequence();

        m_Sequence.Append(fillBar.DOFillAmount(1, 0.2f)).Append(fillBar.DOFillAmount(0.2f, 0.13f)).SetLoops(-1, LoopType.Yoyo);

        m_Sequence.OnUpdate(() => { CalculateFillAmount(); });
    }

    private void CalculateFillAmount()
    {
        float _fill = 0;

        if (fillBar.fillAmount < 0.7f)
        {
            _fill = (fillBar.fillAmount / 0.7f) * 25;
        }
        else
        {
            float difference = fillBar.fillAmount - 0.7f;

            float x = 0.7f - difference;

            _fill = (x / 0.7f) * 25;
        }

        percentText.text = string.Format("{0:####}%", _fill);
    }
    public void KillAnimation()
    {
        m_Sequence.Kill();
    }
}
