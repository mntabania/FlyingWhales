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
    public class MonsterWard : Ward {

        public MonsterWard() {
            name = "Monster Ward";
            description = "Reduce damage taken from wild monsters.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataMonsterWard saveData = saveDataTrait as SaveDataMonsterWard;
            Assert.IsNotNull(saveData);
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