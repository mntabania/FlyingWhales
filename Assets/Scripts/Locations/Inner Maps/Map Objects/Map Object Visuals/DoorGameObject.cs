using UnityEngine;

public class DoorGameObject : TileObjectGameObject {
    [SerializeField] private BoxCollider2D blockerCollider;
    
    public void SetBlockerState(bool state) {
        blockerCollider.enabled = state;
    }
    public override void Reset() {
        base.Reset();
        blockerCollider.enabled = true;
    }
}