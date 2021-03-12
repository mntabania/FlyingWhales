using Inner_Maps.Location_Structures;

public class FullHealData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FULL_HEAL;
    public override string name => "Full Heal";
    public override string description => "This Action fully replenishes a character's HP.";
    public FullHealData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            AudioManager.Instance.CreateSFXAt(targetCharacter.gridTileLocation, SOUND_EFFECT.Heal);
            GameManager.Instance.CreateParticleEffectAt(targetCharacter, PARTICLE_EFFECT.Heal, false);
            targetCharacter.AdjustHP(targetCharacter.maxHP, ELEMENTAL_TYPE.Normal, showHPBar: true);
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if(targetCharacter.IsHealthFull()) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.isDead) {
            reasons += $"{targetCharacter.name} is already dead,";
        }
        if (targetCharacter.IsHealthFull()) {
            reasons += $"{targetCharacter.name} is at full HP,";
        }
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be healed.";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                return targetCharacter.currentStructure is Kennel || targetCharacter.currentStructure is TortureChambers;
            }
            return false;
        }
        return false;
    }
    #endregion
}
