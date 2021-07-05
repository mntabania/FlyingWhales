public class HealData : PlayerAction {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAL;
    public override string name => "Heal";
    public override string description => "This Ability replenishes a portion of the character's HP.";
    public HealData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            int processedHeal = (int)(targetCharacter.maxHP * PlayerSkillManager.Instance.GetAdditionalHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.HEAL) / 100f);
            targetCharacter.AdjustHP(processedHeal, ELEMENTAL_TYPE.Normal, showHPBar: true,
                piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.HEAL));
            Messenger.Broadcast(UISignals.UPDATE_CHARACTER_INFO, targetCharacter);
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
    #endregion
}
