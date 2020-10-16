using System;
using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Hemophiliac : Trait {

        private List<Character> _knownVampires;
        private Character _owner;
        
        #region getters
        public List<Character> knownVampires => _knownVampires;
        public override Type serializedData => typeof(SaveDataHemophiliac);
        #endregion
        
        public Hemophiliac() {
            name = "Hemophiliac";
            description = "Obsessed with Vampires.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            mutuallyExclusive = new[] {"Hemophobic"};
            _knownVampires = new List<Character>();
        }

        public void OnBecomeAwareOfVampire(Character vampire) {
            if (!_knownVampires.Contains(vampire)) {
                _knownVampires.Add(vampire);
                _owner.relationshipContainer.AdjustOpinion(_owner, vampire, "Hemophiliac", 30);
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
            SaveDataHemophiliac saveData = saveDataTrait as SaveDataHemophiliac;
            Assert.IsNotNull(saveData);
            knownVampires.AddRange(SaveUtilities.ConvertIDListToCharacters(saveData.knownVampires));
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataHemophiliac : SaveDataTrait {
    public List<string> knownVampires;

    public override void Save(Trait trait) {
        base.Save(trait);
        Hemophiliac hemophiliac = trait as Hemophiliac;
        Assert.IsNotNull(hemophiliac);
        knownVampires = SaveUtilities.ConvertSavableListToIDs(hemophiliac.knownVampires);
    }
}
#endregion