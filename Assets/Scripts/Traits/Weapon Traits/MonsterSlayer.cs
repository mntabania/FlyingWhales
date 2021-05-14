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
    public class MonsterSlayer : Slayer {

        public MonsterSlayer() {
            name = "Monster Slayer";
            description = "Grants additional damage to wild monsters.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataMonsterSlayer saveDataMonsterSlayer = saveDataTrait as SaveDataMonsterSlayer;
            Assert.IsNotNull(saveDataMonsterSlayer);
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
public class SaveDataMonsterSlayer : SaveDataTrait {
    public int stackCount;
    public override void Save(Trait trait) {
        base.Save(trait);
        Traits.MonsterSlayer monsterSlayer = trait as Traits.MonsterSlayer;
        Assert.IsNotNull(monsterSlayer);
        stackCount = monsterSlayer.stackCount;
    }
}
#endregion