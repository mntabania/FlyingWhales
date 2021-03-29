using UnityEngine;

public class PassiveManaRegen : PassiveSkill {
    public override string name => "Passive Mana Regeneration";
    public override string description => "Passive Mana Regeneration";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Passive_Mana_Regen;
    
    public override void ActivateSkill() {
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
    }
    private void OnHourStarted() {
        if(PlayerManager.Instance.player.mana < 45) {
            PlayerManager.Instance.player.AdjustMana(15);
        }
    }
}