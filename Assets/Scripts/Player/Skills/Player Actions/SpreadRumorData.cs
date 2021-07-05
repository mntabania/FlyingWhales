using System.Collections.Generic;
using Logs;
using Object_Pools;
using UtilityScripts;

public class SpreadRumorData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPREAD_RUMOR;
    public override string name => "Spread Rumor";
    public override string description => "This Action instructs the character to spread a negative rumor about someone they know. Only available on Cultists.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;

    
    public SpreadRumorData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = ObjectPoolManager.Instance.CreateNewCharactersList();
            character.PopulateListOfCultistTargets(choices, x => x.isNormalCharacter && x.race.IsSapient() && !x.isDead);
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), validityChecker: t => CanBeRumored(character, t), onHoverAction: t => OnHoverEnter(character, t), onHoverExitAction: OnHoverExit, showCover: true,
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
            if (targetCharacter.jobQueue.HasJob(JOB_TYPE.SPREAD_RUMOR)) {
                return false;
            }
            return targetCharacter.isDead == false; //&& targetCharacter.traitContainer.HasTrait("Cultist"); //&& targetCharacter.homeSettlement != null
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.limiterComponent.canPerform == false) {
            reasons = $"{reasons}Cannot be used while target is incapacitated,";
        }
        if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
            reasons += "Slaves cannot perform this action,";
        }
        if (targetCharacter.jobQueue.HasJob(JOB_TYPE.SPREAD_RUMOR)) {
            reasons = $"{reasons}{targetCharacter.name} is already planning to spread rumors,";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(PlayerSkillManager.Instance.selectedArchetype != PLAYER_ARCHETYPE.Puppet_Master && !PlayerSkillManager.Instance.unlockAllSkills) {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion

    private bool CanBeRumored(Character owner, Character target) {
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
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Cultists."));
            return;
        }
        if (owner.relationshipContainer.HasOpinionLabelWithCharacter(target, RelationshipManager.Close_Friend)) {
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, UtilityScripts.Utilities.InvalidColorize("Cannot target Close Friends."));
            return;
        }
        string relationshipSummary = owner.visuals.GetRelationshipSummary(target);
        if (!string.IsNullOrEmpty(relationshipSummary)) {
            PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, relationshipSummary);    
        }
    }
    private void OnHoverExit(Character target) {
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    private void OnChooseCharacter(object obj, Character actor) {
        if (obj is Character targetCharacter) {
            UIManager.Instance.HideObjectPicker();

            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_spread_rumor", null, LogUtilities.Cultist_Instruct_Tags);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog);
            LogPool.Release(instructedLog);

            Character spreadRumorOrNegativeInfoTarget = actor.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(targetCharacter);
            if (spreadRumorOrNegativeInfoTarget != null) {
                Rumor rumor = actor.rumorComponent.GenerateNewRandomRumor(spreadRumorOrNegativeInfoTarget, targetCharacter);
                if (rumor != null) {
                    if(!actor.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor)) {
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "spread_rumor_fail", null, LOG_TAG.Player);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToDatabase();
                        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                        base.ActivateAbility(actor);
                    }
                } else {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_rumor_spread_rumor", null, LOG_TAG.Player);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_target_spread_rumor", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, actor as IPlayerActionTarget);
        }
    }
}