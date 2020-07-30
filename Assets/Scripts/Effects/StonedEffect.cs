using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using SpriteGlow;
using UnityEngine;

public class StonedEffect : PooledObject {

    [SerializeField] private SpriteRenderer monsterSprite;
    [SerializeField] private SpriteGlowEffect glowEffect;

    public void PlayEffect(Sprite sprite) {
        monsterSprite.sprite = sprite;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(value => glowEffect.AlphaThreshold = value, 0f, 0.45f, 1f));
        //sequence.Append(monsterSprite.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.1f).SetEase(Ease.InBounce));
        //sequence.OnComplete(OnCompleteSequence);
        sequence.Play();
    }

    //private void OnCompleteSequence() {
    //    AudioManager.Instance.PlayParticleMagnet();
    //    PlayerUI.Instance.monsterToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f);
    //    ObjectPoolManager.Instance.DestroyObject(this);
    //}
    //public override void Reset() {
    //    base.Reset();
    //    trailEffect.gameObject.SetActive(false);
    //}
}
