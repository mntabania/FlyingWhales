using System.Collections;
using Logs;
using Object_Pools;
using UnityEngine;
using UtilityScripts;
using UnityEngine.Localization.Settings;

public class PlagueData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLAGUE;
    public override string name => "Plague";
    public override string description => GetDescription();
    public override string localizedDescription => GetDescription();

    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public PlagueData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "player_afflicted", null, LogUtilities.Player_Life_Changes_Tags);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Plagued", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        LogPool.Release(log);
        if (targetPOI is Character) {
            (targetPOI as Character).interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, targetPOI);
        }
        // targetPOI.traitContainer.AddTrait(targetPOI, "Plagued");
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Plagued", "Robust", "Beast")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Robust")) {
            reasons += $"Robust Villagers are immune to Plague,";
        }
        if (targetCharacter.traitContainer.HasTrait("Plagued")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion

    private string GetDescription() {
        string modifiedDescription = $"{LocalizationSettings.StringDatabase.GetLocalizedString("AfflictionsNameAndDescription_Table", name + "_Description")}";
        //string modifiedDescription = $"This Affliction will afflict a character with a virulent disease. The Plague may start spreading to others before eventually killing the character.";
        if (GameManager.Instance.gameHasStarted) {
            modifiedDescription = $"{modifiedDescription}\n\n{PlagueDisease.Instance.GetPlagueEffectsSummary()}";
        }
        return modifiedDescription;
    }
}