using System;
using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Hemophobic : Trait {

        private List<Character> _knownVampires;
        private Character _owner;

        #region getters
        public List<Character> knownVampires => _knownVampires;
        public override Type serializedData => typeof(SaveDataHemophobic);
        #endregion
        
        public Hemophobic() {
            name = "Hemophobic";
            description = "Deathly afraid of Vampires.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = false;
            mutuallyExclusive = new[] {"Hemophiliac"};
            _knownVampires = new List<Character>();
        }

        public void OnBecomeAwareOfVampire(Character vampire) {
            if (!_knownVampires.Contains(vampire)) {
                _knownVampires.Add(vampire);
                _owner.relationshipContainer.AdjustOpinion(_owner, vampire, "Hemophobic", -30);
                _owner.relationshipContainer.BreakUp(_owner, vampire, "is a Vampire");
            }
        }

        public bool IsVampireKnown(Character character) {
            return _knownVampires.Contains(character);
        }
        
        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                _owner = character;
            }
        }
        #endregion
        
        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataHemophobic saveData = saveDataTrait as SaveDataHemophobic;
            Assert.IsNotNull(saveData);
            knownVampires.AddRange(SaveUtilities.ConvertIDListToCharacters(saveData.knownVampires));
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataHemophobic : SaveDataTrait {
    public List<string> knownVampires;

    public override void Save(Trait trait) {
        base.Save(trait);
        Hemophobic hemophobic = trait as Hemophobic;
        Assert.IsNotNull(hemophobic);
        knownVampires = SaveUtilities.ConvertSavableListToIDs(hemophobic.knownVampires);
    }
}
#endregion