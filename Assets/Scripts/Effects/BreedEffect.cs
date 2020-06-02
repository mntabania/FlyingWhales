using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using SpriteGlow;
using UnityEngine;

public class BreedEffect : PooledObject {

    [SerializeField] private SpriteRenderer monsterSprite;
    [SerializeField] private SpriteGlowEffect glowEffect;
    [SerializeField] private TrailRenderer trailEffect;
    // [SerializeField] private Transform target;
    // private void Awake() {
    //     PlayEffect(null);
    // }
    public void PlayEffect(Sprite sprite) {
        trailEffect.gameObject.SetActive(true);
        monsterSprite.sprite = sprite;
        Vector3 targetPos = InnerMapCameraMove.Instance.innerMapsCamera.ScreenToWorldPoint(PlayerUI.Instance.monsterToggle.transform.position);
        // Vector3 targetPos = target.transform.position;
        
        Vector3 controlPointA = transform.position;
        controlPointA.x -= 5f;
        
        Vector3 controlPointB = targetPos;
        controlPointB.y -= 5f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(value => glowEffect.AlphaThreshold = value, 0f, 0.9f, 1f));
        sequence.Append(monsterSprite.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.5f).SetEase(Ease.InBounce));
        sequence.Append(transform.DOPath(new[] {targetPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier)
            .SetEase(Ease.InSine));
        sequence.OnComplete(OnCompleteSequence);
        sequence.Play();
    }

    private void OnCompleteSequence() {
        PlayerUI.Instance.monsterToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    public override void Reset() {
        base.Reset();
        trailEffect.gameObject.SetActive(false);
    }
}
