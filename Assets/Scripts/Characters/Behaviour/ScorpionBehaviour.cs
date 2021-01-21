using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ScorpionBehaviour : BaseMonsterBehaviour {

    public ScorpionBehaviour() {
        priority = 9;
    }

    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        //between 6pm and 6am
        if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) >= 18 ||
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) < 6) {
            if ((character as Scorpion).heldCharacter == null) {
                character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                character.reactionComponent.SetIsHidden(false);
                return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, true);
            }
        } else {
            character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
            character.reactionComponent.SetIsHidden(true);
            return character.jobComponent.PlanIdleLongStandStill(out producedJob);
        }

        return false;
    }
}