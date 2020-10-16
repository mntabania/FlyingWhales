using System;
using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Lycanphiliac : Trait {

        private Character _owner;
        private List<Character> _knownLycans;
        
        #region getters
        public List<Character> knownLycans => _knownLycans;
        public override Type serializedData => typeof(SaveDataLycanphiliac);
        #endregion
        
        public Lycanphiliac() {
            name = "Lycanphiliac";
            description = "Obsessed with Lycanthropes.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = false;
            mutuallyExclusive = new[] {"Lycanphobic"};
            _knownLycans = new List<Character>();
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

        #region Awareness
        public void OnBecomeAwareOfLycan(Character lycan) {
            if (!_knownLycans.Contains(lycan)) {
                _knownLycans.Add(lycan);
                _owner.relationshipContainer.AdjustOpinion(_owner, lycan, "Lycanphiliac", 30);
            }
        }
        #endregion
        
        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataLycanphiliac saveData = saveDataTrait as SaveDataLycanphiliac;
            Assert.IsNotNull(saveData);
            _knownLycans.AddRange(SaveUtilities.ConvertIDListToCharacters(saveData.knownLycans));
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataLycanphiliac : SaveDataTrait {
    public List<string> knownLycans;

    public override void Save(Trait trait) {
        base.Save(trait);
        Lycanphiliac lycanphiliac = trait as Lycanphiliac;
        Assert.IsNotNull(lycanphiliac);
        knownLycans = SaveUtilities.ConvertSavableListToIDs(lycanphiliac.knownLycans);
    }
}
#endregion