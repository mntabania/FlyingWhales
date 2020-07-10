public class CultistTransformData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.CULTIST_TRANSFORM;
    public override string name => "Transform";
    public override string description => "This Action forces the character to transform into an abomination.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    
    public CultistTransformData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            character.jobComponent.TriggerCultistTransform();
            if (UIManager.Instance.characterInfoUI.isShowing && 
                UIManager.Instance.characterInfoUI.activeCharacter == character) {
                UIManager.Instance.characterInfoUI.CloseMenu();    
            }
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}