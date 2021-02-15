using Inner_Maps;
using Ruinarch;
namespace Player_Input {
    public class SeizeInputModule : PlayerInputModule {
        public override void OnUpdate() {
            if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                PlayerManager.Instance.player.seizeComponent.DisableFollowMousePosition();
            } else {
                PlayerManager.Instance.player.seizeComponent.EnableFollowMousePosition();
                PlayerManager.Instance.player.seizeComponent.FollowMousePosition();
                LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                if (hoveredTile != null) {
                    InputManager.Instance.SetCursorTo(PlayerManager.Instance.player.seizeComponent.CanUnseizeHere(hoveredTile) ? InputManager.Cursor_Type.Check : InputManager.Cursor_Type.Cross);
                } else {
                    InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
                }
            }
        }
    }
}