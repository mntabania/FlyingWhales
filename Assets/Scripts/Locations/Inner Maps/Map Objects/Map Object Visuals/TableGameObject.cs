using UnityEngine;

public class TableGameObject : TileObjectGameObject {
    [SerializeField] private BoxCollider2D blockerCollider;
    public override void SetVisual(Sprite sprite) {
        base.SetVisual(sprite);
        UpdateBlockerPosition();
    }
    private void UpdateBlockerPosition() {
        if (usedSprite != null && usedSprite.name.Contains("bartop")) {
            blockerCollider.offset = new Vector2(0f, 0.5f);
        } else {
            blockerCollider.offset = Vector2.zero;
        }
    }
    public void SetBlockerState(bool state) {
        blockerCollider.enabled = state;
    }
    public override void Reset() {
        base.Reset();
        blockerCollider.enabled = true;
    }
}
