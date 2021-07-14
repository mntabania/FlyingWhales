using System;
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

        public override Type serializedData => typeof(SaveDataSlayer);

        #region Getter
        public int stackCount = 0;
        #endregion

        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataSlayer data = saveDataTrait as SaveDataSlayer;
            stackCount = data.stackCount;
        }
    }
}

#region Save Data
public class SaveDataSlayer : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Slayer data = trait as Slayer;
        stackCount = data.stackCount;
    }
}
#endregion