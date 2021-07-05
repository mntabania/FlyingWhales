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
    public class Flying : Trait {

        protected Character owner;

        #region Getter
        public int stackCount = 0;
        public override Type serializedData => typeof(SaveDataFlying);
        #endregion

        public Flying() {
            name = "Flying";
            description = "Flying Creature";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataFlying flying = saveDataTrait as SaveDataFlying;
            Assert.IsNotNull(flying);
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        #endregion
    }
}

#region Save Data
public class SaveDataFlying : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Traits.Flying flying = trait as Traits.Flying;
        Assert.IsNotNull(flying);
        stackCount = flying.stackCount;
    }
}
#endregion