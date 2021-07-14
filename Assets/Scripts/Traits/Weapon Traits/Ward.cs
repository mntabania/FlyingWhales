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
    public class Ward : Trait {

        public override Type serializedData => typeof(SaveDataWard);

        protected Character owner;

        #region Getter
        public int stackCount = 0;
        #endregion

        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataWard data = saveDataTrait as SaveDataWard;
            stackCount = data.stackCount;
        }
    }
}

#region Save Data
public class SaveDataWard : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Ward data = trait as Ward;
        stackCount = data.stackCount;
    }
}
#endregion