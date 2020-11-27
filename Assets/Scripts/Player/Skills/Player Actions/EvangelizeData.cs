using System.Collections.Generic;
using Logs;

public class EvangelizeData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.EVANGELIZE;
    public override string name => "Preach";
    public override string description => GetDescription();
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public EvangelizeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            if(character.characterClass.className == "Cult Leader") {
                //Cultist leader should have all characters as the target for evangelization
                List<Character> choices = new List<Character>();
                for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                    Character target = CharacterManager.Instance.allCharacters[i];
                    //if (resident.isNormalCharacter && resident.traitContainer.HasTrait("Cultist") == false && 
                    //    resident.isDead == false &&
                    //    character.relationshipContainer.HasOpinionLabelWithCharacter(resident, RelationshipManager.Close_Friend) == false) {
                    if(!target.isDead && target.isNormalCharacter && target.race.IsSapient()) {
                        choices.Add(target);
                    }
                }
                UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeEvangelized(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
                    shouldShowConfirmationWindowOnPick: false, layer: 25, asButton: false);
            } else {
                List<Character> choices = character.GetListOfCultistTargets(x => !x.isDead && x.isNormalCharacter && x.race.IsSapient());
                if (choices == null) { choices = new List<Character>(); }
                UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeEvangelized(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
                    shouldShowConfirmationWindowOnPick: false, layer: 25, asButton: false);
            }

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
            return targetCharacter.isDead == false; //&& targetCharacter.traitContainer.HasTrait("Cultist"); //&& targetCharacter.homeSettlement != null
        }
        return false;
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
    private string GetDescription() {
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.characterClass.className == "Cult Leader") {
            return "This Action instructs the character to Preach about Demon Worship to any Villager."; 
        }
        return "This Action instructs the character to Preach about Demon Worship to someone they know.";
    }
    #endregion

    private bool CanBeEvangelized(Character owner, Character target) {
        if (target.traitContainer.HasTrait("Cultist")) {
            return false;
        }
        AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(target);
        if (awarenessState == AWARENESS_STATE.Missing) {
            return false;
        }
        if (awarenessState == AWARENESS_STATE.Presumed_Dead) {
            return false;
        }
        if (target.traitContainer.HasTrait("Travelling")) {
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
        AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(target);
        if (awarenessState == AWARENESS_STATE.Missing) {
            // UIManager.Instance.ShowSmallInfo("<color=red>Cannot target Missing characters.</color>");
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Missing characters."));
            return;
        }
        if (awarenessState == AWARENESS_STATE.Presumed_Dead) {
            // UIManager.Instance.ShowSmallInfo("<color=red>Cannot target Presumed Dead characters.</color>");
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Presumed Dead characters."));
            return;
        }
        if (target.traitContainer.HasTrait("Travelling")) {
            // UIManager.Instance.ShowSmallInfo("<color=red>Cannot target Travelling characters.</color>");
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Travelling characters."));
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
            
            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_evangelize", null, LOG_TAG.Player, LOG_TAG.Crimes);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog);
            
            if (actor.jobComponent.TryCreateEvangelizeJob(targetCharacter) == false) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "evangelize_fail", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            } else {
                base.ActivateAbility(actor);
            }
        }
    }
}