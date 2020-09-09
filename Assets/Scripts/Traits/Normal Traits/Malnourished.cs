using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    public class Malnourished : Status {

        private Character owner;
        private readonly int deathDuration;
        private int _currentDeathDuration;

        public int currentDeathDuration => _currentDeathDuration;
        public override Type serializedData => typeof(SaveDataMalnourished);
        public Malnourished() {
            name = "Malnourished";
            description = "Has not eaten for a very long time.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            moodEffect = -8;
            deathDuration = GameManager.Instance.GetTicksBasedOnHour(36);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataMalnourished saveDataMalnourished = saveDataTrait as SaveDataMalnourished;
            Assert.IsNotNull(saveDataMalnourished);
            _currentDeathDuration = saveDataMalnourished.currentDeathDuration;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            owner = addTo as Character;
        }
        #endregion
        

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            _currentDeathDuration = 0;
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            CheckDeath(owner);
        }
        #endregion

        private void CheckDeath(Character owner) {
            _currentDeathDuration = currentDeathDuration + 1;
            if(currentDeathDuration >= deathDuration) {
                owner.Death("starvation");
            }
        }
    }
}

#region Save Data
public class SaveDataMalnourished : SaveDataTrait {
    public int currentDeathDuration;
    public override void Save(Trait trait) {
        base.Save(trait);
        Malnourished malnourished = trait as Malnourished;
        Assert.IsNotNull(malnourished);
        currentDeathDuration = malnourished.currentDeathDuration;
    }
}
#endregion
