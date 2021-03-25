﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KennelData : DemonicStructurePlayerSkill {
    
    public override string name => "Kennel";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.KENNEL;
    public override string description => "This Structure allows the Player to imprison a monster. The Player may then spawn copies of it for use in various parties. Imprisoned monsters may also be drained of Spirit Energy.";
    public KennelData() {
        structureType = STRUCTURE_TYPE.KENNEL;
    }
}