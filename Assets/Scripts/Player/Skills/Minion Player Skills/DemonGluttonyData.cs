﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGluttonyData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_GLUTTONY;
    public override string name { get { return "Gluttony Demon"; } }
    public override string description => "This Demon is a robust ranged magic-user that deals Water damage. Can be summoned to defend an Area or Structure. NOTE: Cannot be summoned on an active settlement.";

    public DemonGluttonyData() {
        minionType = MINION_TYPE.Gluttony;
        className = "Gluttony";
    }
}
