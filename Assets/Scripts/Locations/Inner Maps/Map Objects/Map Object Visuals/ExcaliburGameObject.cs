using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class ExcaliburGameObject : TileObjectGameObject {

    [Header("Excalibur Specific")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    public override void UpdateTileObjectVisual(TileObject tileObject) {
       Excalibur excalibur = tileObject as Excalibur;
       Assert.IsNotNull(excalibur);
       if (excalibur.lockedState == Excalibur.Locked_State.Locked) {
           SetVisual(lockedSprite);
       } else if (excalibur.lockedState == Excalibur.Locked_State.Unlocked) {
           SetVisual(unlockedSprite);
       }
    }
}