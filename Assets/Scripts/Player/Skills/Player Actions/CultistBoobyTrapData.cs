using System.Collections.Generic;

public class CultistBoobyTrapData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.CULTIST_BOOBY_TRAP;
    public override string name => "Cultist - Booby Trap";
    public override string description => "This Action forces the character to Booby Trap an object owned by a specified target.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    
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
                    character.relationshipContainer.HasOpinionLabelWithCharacter(resident, BaseRelationshipContainer.Close_Friend) == false) {
                    choices.Add(resident);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, character), showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 20, asButton: false);
        }
        // base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerformAbility = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerformAbility) {
            return targetCharacter.homeSettlement != null;    
        }
        return false;
    }
    #endregion

    private void OnChooseCharacter(object obj, Character actor) {
        if (obj is Character targetCharacter) {
            UIManager.Instance.HideObjectPicker();
            if (actor.jobComponent.CreatePlaceTrapJob(targetCharacter, JOB_TYPE.CULTIST_BOOBY_TRAP) == false) {
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, "Booby Trap", LOG_IDENTIFIER.STRING_1);
                actor.logComponent.RegisterLog(log);
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
            base.ActivateAbility(actor);
        }
    }
}