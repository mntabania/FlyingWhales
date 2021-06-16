using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
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
            AddTraitOverrideFunctionIdentifier(TraitManager.Villager_Reaction);
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

        #region Reactions
        public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog) {
            base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
            if (!HasAlreadyBeenSeenByCharacter(actor)) {
                AddCharacterThatSaw(actor);

                if (actor.traitContainer.HasTrait("Suspicious")) {
                    if (GameUtilities.RollChance(50)) {
                        actor.jobComponent.TriggerDestroy(owner);
                    } else {
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, owner);
                    }
                } else {
                    if (GameUtilities.RollChance(50) && !actor.jobQueue.HasJob(JOB_TYPE.INSPECT, owner) && !actor.defaultCharacterTrait.HasAlreadyInspectedObject(owner)) {
                        actor.jobComponent.TriggerInspect(owner);
                    } else if (!actor.IsInventoryAtFullCapacity() && !actor.HasItem(owner.name) && !actor.HasOwnedItemInHomeStructure(owner.name)) {
                        actor.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, owner);
                    }
                }
            }
        }
        #endregion
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