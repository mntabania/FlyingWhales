using UnityEngine;

public class AutoAbsorbChaosOrb : PassiveSkill {
    public override string name => "Automatically absorb Chaos Orbs";
    public override string description => "Expired Mana Orbs are auto absorbed";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Auto_Absorb_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<ChaosOrb>(PlayerSignals.CHAOS_ORB_EXPIRED, OnChaosOrbExpired);
    }
    private void OnChaosOrbExpired(ChaosOrb chaosOrb) {
        //https://trello.com/c/baCCZSMn/2162-chaos-orbs-are-auto-absorbed-when-expired-but-only-gives-minimal-mana
        PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(2);
    }
}