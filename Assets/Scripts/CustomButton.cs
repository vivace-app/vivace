using System;
using CriWare;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public Action onClickCallback;

    [SerializeField] private CriAtomSource criAtomSource;
    [SerializeField] private CanvasGroup canvasGroup;

    public CustomButton(Action onClickCallback)
    {
        this.onClickCallback = onClickCallback;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        criAtomSource.Play();
        transform.DOScale(0.95f, 0.24f).SetEase(Ease.OutCubic);
        canvasGroup.DOFade(0.75f, 0.24f).SetEase(Ease.OutCubic);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.24f).SetEase(Ease.OutCubic);
        canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
    }
}