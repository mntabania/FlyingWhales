public class TriggerFlawChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Successful Trigger Flaw";
    public override string description => "Chaos Orbs from Successful Trigger Flaw";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Trigger_Flaw_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(PlayerSkillSignals.FLAW_TRIGGER_SUCCESS, OnTriggerFlawSuccess);
    }

    void OnTriggerFlawSuccess(Character p_character) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, 1, p_character.gridTileLocation.parentMap);
    }
}