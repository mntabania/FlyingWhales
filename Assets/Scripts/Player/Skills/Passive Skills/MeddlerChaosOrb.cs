using Traits;
using Interrupts;

public class MeddlerChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs from Successful Meddler Scheme";
    public override string description => "Chaos Orbs from Successful Meddler Schem";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Meddler_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MEDDLER_SCHEME_SUCCESSFUL, OnSuccessMeddlerScheme); 
    }

    void OnSuccessMeddlerScheme(Character p_character) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, 3, p_character.gridTileLocation.parentMap);
    }
}