using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Object_Pools;
using UtilityScripts;

public class AgitateData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.AGITATE;
    public override string name => "Agitate";
    public override string description => "This Ability may be used on a wild monster to force it to enter a state of frenzy. It will terrorize nearby Villagers if possible." +
        "\nAgitating a monster produces 1 Chaos Orb. Additional 2 Chaos Orbs are produced each time the monster kills a Villager.";
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
                Messenger.Broadcast(PlayerSkillSignals.AGITATE_ACTIVATED, targetPOI);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "agitated", null, LogUtilities.Agitate_Tags);
                log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                LogPool.Release(log);
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
            bool isValid = base.IsValid(target);
            return isValid && !(targetCharacter is Dragon) && (targetCharacter.faction == null || !targetCharacter.faction.isPlayerFaction) && !targetCharacter.movementComponent.isStationary;
        }
        return false;
    }
    #endregion
}
