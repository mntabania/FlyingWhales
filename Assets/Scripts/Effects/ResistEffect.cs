using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using SpriteGlow;
using UnityEngine;

public class ResistEffect : PooledObject {

    [SerializeField] private SpriteRenderer monsterSprite;
    [SerializeField] private SpriteGlowEffect glowEffect;

    public void PlayEffect(Sprite sprite) {
        monsterSprite.sprite = sprite;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(value => glowEffect.AlphaThreshold = value, 0.3f, 0f, 1f));
        sequence.Append(monsterSprite.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.1f).SetEase(Ease.OutBounce));
        sequence.OnComplete(OnCompleteSequence);
        sequence.Play();
    }

    private void OnCompleteSequence() {
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
