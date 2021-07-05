using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;

public class AreaMapTextPopup : PooledObject {
    [SerializeField] private TextMeshPro text;

    public void Show(string p_text, Vector3 p_startPos, Color p_textColor) {
        text.text = p_text;
        text.color = p_textColor;
        transform.position = p_startPos;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + 1f, 1f));
        sequence.Join(text.DOFade(0f, 1.2f).SetEase(Ease.InQuint));
        sequence.OnComplete(OnCompleteTween);
        sequence.Play();
    }
    private void OnCompleteTween() {
        ObjectPoolManager.Instance.DestroyObject(this);
    }
}
