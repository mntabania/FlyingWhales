using System.Collections.Generic;
using Logs;

public class CultistBoobyTrapData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.CULTIST_BOOBY_TRAP;
    public override string name => "Booby Trap Neighbor";
    public override string description => "This Action instructs the character to Booby Trap an object owned by someone they know.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public CultistBoobyTrapData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = character.GetListOfCultistTargets(x => x.isNormalCharacter && x.race.IsSapient() && !x.isDead);
            if (choices == null) { choices = new List<Character>(); }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeTrapped(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25, asButton: false);
        }
        // base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.canPerform == false) {
                return false;
            }
            return targetCharacter.isDead == false && targetCharacter.homeSettlement != null;
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
        if (targetCharacter.canPerform == false) {
            reasons += "Cannot be used while target is incapacitated,";
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
            
            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_trap", null, LOG_TAG.Player, LOG_TAG.Crimes);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog);
            
            if (actor.jobComponent.CreatePlaceTrapJob(targetCharacter, JOB_TYPE.CULTIST_BOOBY_TRAP) == false) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "cultist_no_trap_target", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
            base.ActivateAbility(actor);
        }
    }
}