using System.Collections.Generic;
using Logs;
using Object_Pools;
using UtilityScripts;

public class EvangelizeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EVANGELIZE;
    public override string name => "Preach";
    public override string description => "This Ability instructs the character to preach about Demon Worship to someone they know. Only available on Cultists."; //GetDescription();
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public EvangelizeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            if(character.characterClass.className == "Cult Leader") {
                //Cultist leader should have all characters as the target for evangelization
                List<Character> choices = ObjectPoolManager.Instance.CreateNewCharactersList();
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

                ObjectPoolManager.Instance.ReturnCharactersListToPool(choices);
            } else {
                List<Character> choices = ObjectPoolManager.Instance.CreateNewCharactersList();
                character.PopulateListOfCultistTargets(choices, x => !x.isDead && x.isNormalCharacter && x.race.IsSapient());
                UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeEvangelized(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
                    shouldShowConfirmationWindowOnPick: false, layer: 25, asButton: false);
                ObjectPoolManager.Instance.ReturnCharactersListToPool(choices);
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
            return "This Action instructs the character to Preach about Demon Worship to any Villager. NOTE: This requires the actor to have a Cultist Kit"; 
        }
        return "This Action instructs the character to Preach about Demon Worship to someone they know. NOTE: This requires the actor to have a Cultist Kit";
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
            
            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_evangelize", null, LogUtilities.Cultist_Instruct_Tags);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog, true);
            
            if (actor.jobComponent.TryCreateEvangelizeJob(targetCharacter) == false) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "evangelize_fail", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            } else {
                base.ActivateAbility(actor);
            }
        }
    }
}