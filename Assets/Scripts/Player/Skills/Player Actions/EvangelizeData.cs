using System.Collections.Generic;
using Logs;

public class EvangelizeData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.EVANGELIZE;
    public override string name => "Evangelize";
    public override string description => "This Action forces the character to turn another character into a Cultist.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;

    
    public EvangelizeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = new List<Character>();
            for (int i = 0; i < character.homeSettlement.residents.Count; i++) {
                Character resident = character.homeSettlement.residents[i];
                //if (resident.isNormalCharacter && resident.traitContainer.HasTrait("Cultist") == false && 
                //    resident.isDead == false &&
                //    character.relationshipContainer.HasOpinionLabelWithCharacter(resident, RelationshipManager.Close_Friend) == false) {
                if (character.jobComponent.IsValidEvangelizeTarget(character)) {
                    choices.Add(resident);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 40, asButton: false);
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