using Inner_Maps;
using Ruinarch;
namespace Player_Input {
    public class IntelInputModule : PlayerInputModule {
        public override void OnUpdate() {
            IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
            if (hoveredPOI != null) {
                string hoverText = string.Empty;
                InputManager.Instance.SetCursorTo(PlayerManager.Instance.player.CanShareIntelTo(hoveredPOI, ref hoverText, PlayerManager.Instance.player.currentActiveIntel)
                    ? InputManager.Cursor_Type.Check
                    : InputManager.Cursor_Type.Cross);
                if(hoverText != string.Empty) {
                    UIManager.Instance.ShowSmallInfo(hoverText);
                }
            } else {
                UIManager.Instance.HideSmallInfo();
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            }
        }
    }
}