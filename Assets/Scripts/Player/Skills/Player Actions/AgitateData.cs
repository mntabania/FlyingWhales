using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class AgitateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.AGITATE;
    public override string name => "Agitate";
    public override string description => "This Action can be used on a monster. The target will enter a state of frenzy and will terrorize nearby Residents.";
    public AgitateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character targetCharacter) {
            targetCharacter.movementComponent.SetEnableDigging(true);
            LocationStructure targetStructure = targetCharacter.gridTileLocation.GetNearestVillageStructureFromThisWithResidents(targetCharacter);
            if(targetStructure != null) {
                targetCharacter.jobQueue.CancelAllJobs();
                targetCharacter.behaviourComponent.SetAttackVillageTarget(targetStructure.settlementLocation as NPCSettlement);
                targetCharacter.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
                targetCharacter.behaviourComponent.SetIsAgitated(true);

                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "agitated");
                log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToInvolvedObjects();
            } else {
                targetCharacter.movementComponent.SetEnableDigging(false);
            }
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Hibernating");
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            return !(targetCharacter is Dragon) && (targetCharacter.faction == null || !targetCharacter.faction.isPlayerFaction);
        }
        return false;
    }
    #endregion
}
