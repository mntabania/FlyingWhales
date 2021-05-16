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

        protected Character owner;

        #region Getter
        public int stackCount = 0;
        public override Type serializedData => typeof(SaveDataMonsterWard);
        #endregion
    }
}

#region Save Data
public class SaveDataMonsterWard : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Traits.Ward ward = trait as Traits.Ward;
        Assert.IsNotNull(ward);
        stackCount = ward.stackCount;
    }
}
#endregion