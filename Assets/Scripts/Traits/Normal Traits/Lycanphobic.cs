using System;
using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Lycanphobic : Trait {

        private Character _owner;
        private List<Character> _knownLycans;
        
        #region getters
        public List<Character> knownLycans => _knownLycans;
        public override Type serializedData => typeof(SaveDataLycanphobic);
        #endregion
        
        public Lycanphobic() {
            name = "Lycanphobic";
            description = "Deathly afraid of Lycanthropes.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = false;
            mutuallyExclusive = new[] {"Lycanphiliac"};
            _knownLycans = new List<Character>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Become_Faction_Leader);
        }
        

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
            }
        }
        public override void OnBecomeFactionLeader(Character leader, Faction faction) {
            base.OnBecomeFactionLeader(leader, faction);
            faction.factionType.AddIdeology(FACTION_IDEOLOGY.Hates_Werewolves);
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
                _owner.relationshipContainer.AdjustOpinion(_owner, lycan, "Lycanphobic", -30);
                _owner.relationshipContainer.BreakUp(_owner, lycan, "is a Lycanthrope");
            }
        }
        #endregion
        
        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataLycanphobic saveData = saveDataTrait as SaveDataLycanphobic;
            Assert.IsNotNull(saveData);
            _knownLycans.AddRange(SaveUtilities.ConvertIDListToCharacters(saveData.knownLycans));
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataLycanphobic : SaveDataTrait {
    public List<string> knownLycans;

    public override void Save(Trait trait) {
        base.Save(trait);
        Lycanphobic lycanphobic = trait as Lycanphobic;
        Assert.IsNotNull(lycanphobic);
        knownLycans = SaveUtilities.ConvertSavableListToIDs(lycanphobic.knownLycans);
    }
}
#endregion