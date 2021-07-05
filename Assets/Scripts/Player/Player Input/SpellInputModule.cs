using Inner_Maps;
using Locations.Settlements;
using Ruinarch;
namespace Player_Input {
    public class SpellInputModule : PlayerInputModule {

        private LocationGridTile _lastHoveredTile;
        private bool _canTargetLastHoveredTile;
        
        public override void OnUpdate() {
            if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default); 
                PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();
            } else { 
                LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                bool canTarget = false; 
                IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi; 
                string hoverText = string.Empty; 
                for (int i = 0; i < PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes.Length; i++) {
                    switch (PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes[i]) { 
                        case SPELL_TARGET.CHARACTER: 
                        case SPELL_TARGET.TILE_OBJECT: 
                            if (hoveredPOI != null) { 
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredPOI, ref hoverText); 
                            } 
                            break; 
                        case SPELL_TARGET.TILE: 
                            if (hoveredTile != null) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile); 
                            } 
                            break; 
                        case SPELL_TARGET.AREA: 
                            if (hoveredTile != null) {
                                //Note: This checking is for performance improvements, so that if the player does not exit the last hovered tile,
                                //Can Target does not need to be called again, if by chance that the target is no longer valid while the player is
                                //hovering it, then the spell cast will still fail, since Can Target is also checked before spell casting.
                                if (_lastHoveredTile != hoveredTile) {
                                    _lastHoveredTile = hoveredTile;
                                    canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile.area);
                                    _canTargetLastHoveredTile = canTarget;
                                } else {
                                    canTarget = _canTargetLastHoveredTile;
                                }
                            } 
                            break;
                        case SPELL_TARGET.SETTLEMENT:
                            if (hoveredTile != null && hoveredTile.IsPartOfSettlement(out var settlement)) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(settlement);
                            }
                            break;
                    }
                    InputManager.Instance.SetCursorTo(canTarget ? InputManager.Cursor_Type.Check : InputManager.Cursor_Type.Cross);
                }
                if (canTarget) {
                    PlayerManager.Instance.player.currentActivePlayerSpell.ShowValidHighlight(hoveredTile);
                } else {
                    if (hoveredTile == null || PlayerManager.Instance.player.currentActivePlayerSpell.ShowInvalidHighlight(hoveredTile, ref hoverText) == false) {
                        PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();    
                    }
                }
                if(!string.IsNullOrEmpty(hoverText)) {
                    UIManager.Instance.ShowSmallInfo(hoverText);
                } else { 
                    UIManager.Instance.HideSmallInfo(); 
                } 
            }
        }
    }
}