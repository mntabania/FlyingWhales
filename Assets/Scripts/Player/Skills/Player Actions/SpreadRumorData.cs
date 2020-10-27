using System.Collections.Generic;
using Logs;

public class SpreadRumorData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SPREAD_RUMOR;
    public override string name => "Spread Rumor";
    public override string description => "This Action instructs the character to Spread a Rumor about someone they know.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;

    
    public SpreadRumorData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = character.GetListOfCultistTargets(x => x.isNormalCharacter && x.traitContainer.HasTrait("Cultist") == false && x.isDead == false && character.relationshipContainer.HasOpinionLabelWithCharacter(x, RelationshipManager.Close_Friend) == false);
            if (choices != null) {
                UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), showCover: true,
                    shouldShowConfirmationWindowOnPick: false, layer: 40, asButton: false);
            }
        }
        // base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.canPerform == false) {
                return false;
            }
            return targetCharacter.isDead == false && targetCharacter.homeSettlement != null && targetCharacter.traitContainer.HasTrait("Cultist");
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.canPerform == false) {
            reasons += "Cannot be used while target is incapacitated,";
        }
        return reasons;
    }
    #endregion

    private void OnChooseCharacter(object obj, Character actor) {
        if (obj is Character targetCharacter) {
            UIManager.Instance.HideObjectPicker();

            Log instructedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_spread_rumor", null, LOG_TAG.Player, LOG_TAG.Crimes);
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            instructedLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog);

            Character spreadRumorOrNegativeInfoTarget = actor.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(targetCharacter);
            if (spreadRumorOrNegativeInfoTarget != null) {
                Rumor rumor = actor.rumorComponent.GenerateNewRandomRumor(spreadRumorOrNegativeInfoTarget, targetCharacter);
                if (rumor != null) {
                    if(!actor.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor)) {
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "spread_rumor_fail", null, LOG_TAG.Player);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToDatabase();
                        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                        base.ActivateAbility(actor);
                    }
                } else {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_rumor_spread_rumor", null, LOG_TAG.Player);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_target_spread_rumor", null, LOG_TAG.Player);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
        }
    }
}