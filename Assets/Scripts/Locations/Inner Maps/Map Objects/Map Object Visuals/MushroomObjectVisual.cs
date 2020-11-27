public class MushroomObjectVisual : CropObjectVisual {
    protected override void ShowGrowingSprite() {
        base.ShowGrowingSprite();
        SetActiveState(false);
    }
    protected override void ShowRipeSprite() {
        base.ShowRipeSprite();
        SetActiveState(true);
    }
}
