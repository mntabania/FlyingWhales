using System.Collections.Generic;
using Logs;
using Object_Pools;
using UtilityScripts;

public class CultistBoobyTrapData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP;
    public override string name => "Booby Trap";
    public override string description => "This Ability instructs the character to Booby Trap an object owned by someone they know. Only available on Cultists.";
    public override bool canBeCastOnBlessed => true;
    
    public CultistBoobyTrapData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = ObjectPoolManager.Instance.CreateNewCharactersList();
            character.PopulateListOfCultistTargets(choices, x => x.isNormalCharacter && x.race.IsSapient() && !x.isDead);
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeTrapped(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25, asButton: false);
            ObjectPoolManager.Instance.ReturnCharactersListToPool(choices);
        }
        // base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.limiterComponent.canPerform == false) {
                return false;
            }
            if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
                return false;
            }
            return targetCharacter.isDead == false; //&& targetCharacter.homeSettlement != null;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (PlayerSkillManager.Instance.selectedArchetype != PLAYER_ARCHETYPE.Ravager) {
            return false;
        }
        return base.IsValid(target);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.limiterComponent.canPerform == false) {
            reasons += "Cannot be used while target is incapacitated,";
        }
        if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
            reasons += "Slaves cannot perform this action,";
        }
        return reasons;
    }
    #endregion

    private bool CanBeTrapped(Character owner, Character target) {
        if (target.traitContainer.HasTrait("Cultist")) {
            return false;
        }
        if (owner.relationshipContainer.HasOpinionLabelWithCharacter(target, RelationshipManager.Close_Friend)) {
            return false;
        }
        return true;
    }
    private void OnHoverEnter(Character owner, Character target) {
        if (target.traitContainer.HasTrait("Cultist")) {
            // UIManager.Instance.ShowSmallInfo("<color=red>Cannot target Cultists.</color>");
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Cultists."));
            return;
        }
        if (owner.relationshipContainer.HasOpinionLabelWithCharacter(target, RelationshipManager.Close_Friend)) {
            // UIManager.Instance.ShowSmallInfo("<color=red>Cannot target Close Friends.</color>");
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Close Friends."));
            return;
        }
        string relationshipSummary = owner.visuals.GetRelationshipSummary(target);
        if (!string.IsNullOrEmpty(relationshipSummary)) {
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, relationshipSummary);    
        }
    }
    private void OnHoverExit(Character target) {
        // UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    private void OnChooseCharacter(object obj, Character actor) {
        if (obj is Character targetCharacter) {
            UIManager.Instance.HideObjectPicker();
            
            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_trap", null, LogUtilities.Cultist_Instruct_Tags);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog, true);
            
            if (actor.jobComponent.CreatePlaceTrapJob(targetCharacter, JOB_TYPE.CULTIST_BOOBY_TRAP) == false) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "cultist_no_trap_target", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
            base.ActivateAbility(actor);
        }
    }
}