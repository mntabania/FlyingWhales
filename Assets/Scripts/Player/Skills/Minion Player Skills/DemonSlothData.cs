using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSlothData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_SLOTH;
    public override string name => "Sloth Demon";
    public override string description => "This Demon is a tough melee magic-user that deals Ice damage. Can be summoned to defend an Area or Structure. NOTE: Cannot be summoned on an active settlement.";

    public DemonSlothData() {
        minionType = MINION_TYPE.Sloth;
        className = "Sloth";
    }
}
