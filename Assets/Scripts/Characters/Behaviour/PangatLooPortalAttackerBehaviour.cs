using System.Collections;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PangatLooPortalAttackerBehaviour : CharacterBehaviourComponent {
    public PangatLooPortalAttackerBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n{character.name} is a Pangat Loo Portal Attacker";
#endif
        if (character.currentStructure is ThePortal portal) {
#if DEBUG_LOG
            log += $"\n-Already at portal, will attack portal";
#endif
            if (portal.objectsThatContributeToDamage.Count > 0 && !portal.hasBeenDestroyed) {
#if DEBUG_LOG
                log += "\n-Has tile object that contribute damage";
                log += "\n-Adding tile object as hostile";
#endif
                TileObject chosenTileObject = null;
                IDamageable nearestDamageableObject = portal.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                    chosenTileObject = tileObject;
                }
                if (chosenTileObject != null) {
                    character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                    producedJob = null;
                    return true;
                } else {
#if DEBUG_LOG
                    log += "\n-No tile object that contribute damage/target structure is destroyed, go home";
#endif
                    return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
#if DEBUG_LOG
                log += "\n-No tile object that contribute damage/target structure is destroyed, go home";
#endif
                return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
            }
        } else {
#if DEBUG_LOG
            log += $"\n-character is not yet at portal, will go there now...";
#endif
            //character is not yet at target village
            Area targetArea = PlayerManager.Instance.player.portalArea;
            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetArea.gridTileComponent.gridTiles);
            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
        }
        
    }
}
