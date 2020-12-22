using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class AgitateData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.AGITATE;
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

                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "agitated", null, LOG_TAG.Player, LOG_TAG.Combat);
                log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            } else {
                targetCharacter.movementComponent.SetEnableDigging(false);
                PlayerUI.Instance.ShowGeneralConfirmation("AGITATE FAILED", "Cannot find a valid village target.");
            }
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.currentStructure is Kennel) {
                return false;
            }
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Hibernating") && !targetCharacter.behaviourComponent.isAgitated && !targetCharacter.isDead && targetCharacter.limiterComponent.canPerform;
        }
        return canPerform;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.currentStructure is Kennel) {
            reasons += "Monsters inside the Kennel cannot be Agitated.";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            return !(targetCharacter is Dragon) && (targetCharacter.faction == null || !targetCharacter.faction.isPlayerFaction) && !targetCharacter.movementComponent.isStationary;
        }
        return false;
    }
    #endregion
}
