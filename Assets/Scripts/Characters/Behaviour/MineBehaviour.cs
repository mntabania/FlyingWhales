using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UtilityScripts;

public class MineBehaviour : CharacterBehaviourComponent {
    
    public MineBehaviour() {
        priority = 440;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        NPCSettlement homeSettlement = character.homeSettlement;
        if (homeSettlement != null) {
            List<LocationStructure> mineShacks = homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
            LocationGridTile targetTile = null;
            if(mineShacks != null && mineShacks.Count > 0) {
                for (int i = 0; i < mineShacks.Count; i++) {
                    Inner_Maps.Location_Structures.Mine mineShack = mineShacks[i] as Inner_Maps.Location_Structures.Mine;
                    if(mineShack != null && mineShack.connectedCave != null) {
                        targetTile = mineShack.connectedCave.GetRandomPassableTileThatIsNotOccupied();
                        if (targetTile != null) {
                            break;
                        }
                    }
                }
            }
            if(targetTile != null) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE, INTERACTION_TYPE.MINE, targetTile.tileObjectComponent.genericTileObject, character);
                job.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                producedJob = job;
                return true;
            }
        }
        return false;
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.movementComponent.SetEnableDigging(true);
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.movementComponent.SetEnableDigging(false);
    }
}