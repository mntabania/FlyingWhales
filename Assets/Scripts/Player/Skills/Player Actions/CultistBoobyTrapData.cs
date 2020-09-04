using System.Collections.Generic;

public class CultistBoobyTrapData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.CULTIST_BOOBY_TRAP;
    public override string name => "Booby Trap Neighbor";
    public override string description => "This Action forces the character to Booby Trap an object owned by a specified target.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public CultistBoobyTrapData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            List<Character> choices = new List<Character>();
            for (int i = 0; i < character.homeSettlement.residents.Count; i++) {
                Character resident = character.homeSettlement.residents[i];
                if (resident.isNormalCharacter && resident.traitContainer.HasTrait("Cultist") == false && 
                    resident.isDead == false &&
                    character.relationshipContainer.HasOpinionLabelWithCharacter(resident, RelationshipManager.Close_Friend) == false) {
                    choices.Add(resident);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 20, asButton: false);
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
            
            Log instructedLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "instructed_trap");
            instructedLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            instructedLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            actor.logComponent.RegisterLog(instructedLog);
            PlayerManager.Instance.player.ShowNotificationFromPlayer(instructedLog);
            
            if (actor.jobComponent.CreatePlaceTrapJob(targetCharacter, JOB_TYPE.CULTIST_BOOBY_TRAP) == false) {
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cultist_no_trap_target");
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                actor.logComponent.RegisterLog(log);
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
            base.ActivateAbility(actor);
        }
    }
}