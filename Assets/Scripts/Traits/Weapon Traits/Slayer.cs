﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Traits {
    public class Slayer : Trait {

        protected Character owner;

        #region Getter
        public int stackCount = 0;
        public override Type serializedData => typeof(SaveDataMonsterSlayer);
        #endregion
    }
}

#region Save Data
public class SaveDataMonsterSlayer : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Traits.Slayer slayer = trait as Traits.Slayer;
        Assert.IsNotNull(slayer);
        stackCount = slayer.stackCount;
    }
}
#endregion