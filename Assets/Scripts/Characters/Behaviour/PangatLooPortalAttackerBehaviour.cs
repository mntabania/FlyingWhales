using System.Collections;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PangatLooPortalAttackerBehaviour : CharacterBehaviourComponent {
    public PangatLooPortalAttackerBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is a Pangat Loo Portal Attacker";
        if (character.currentStructure is ThePortal portal) {
            log += $"\n-Already at portal, will attack portal";
            if (portal.objectsThatContributeToDamage.Count > 0 && !portal.hasBeenDestroyed) {
                log += "\n-Has tile object that contribute damage";
                log += "\n-Adding tile object as hostile";
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
                    log += "\n-No tile object that contribute damage/target structure is destroyed, go home";
                    return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
                log += "\n-No tile object that contribute damage/target structure is destroyed, go home";
                return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
            }
        } else {
            log += $"\n-character is not yet at portal, will go there now...";
            //character is not yet at target village
            HexTile targetHextile = PlayerManager.Instance.player.portalTile;
            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.locationGridTiles);
            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
        }
        
    }
}
