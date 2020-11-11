using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class Interesting : Trait {

        public List<Character> charactersThatSaw { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataInteresting);
        #endregion

        public Interesting() {
            name = "Interesting";
            description = "An interesting thing.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.INSPECT, INTERACTION_TYPE.ASSAULT };
            charactersThatSaw = new List<Character>();
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataInteresting dataTrait = p_saveDataTrait as SaveDataInteresting;
            Assert.IsNotNull(dataTrait);
            charactersThatSaw.AddRange(SaveUtilities.ConvertIDListToCharacters(dataTrait.charactersThatSaw));
        }
        #endregion

        public void AddCharacterThatSaw(Character character) {
            charactersThatSaw.Add(character);
        }
        public bool HasAlreadyBeenSeenByCharacter(Character character) {
            return charactersThatSaw.Contains(character);
        }
    }
}

#region Save Data
public class SaveDataInteresting : SaveDataTrait {
    public List<string> charactersThatSaw;
    public override void Save(Trait trait) {
        base.Save(trait);
        Interesting interesting = trait as Interesting;
        Assert.IsNotNull(interesting);
        charactersThatSaw = SaveUtilities.ConvertSavableListToIDs(interesting.charactersThatSaw);
    }
}
#endregion