using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

namespace Traits {
    public class Dead : Status {
        // public override bool isSingleton => true; //Removed because responsible characters need to be per instance

        public Dead() {
            name = "Dead";
            description = "Simply no longer alive.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.RAISE_CORPSE };
            hindersMovement = true;
            hindersWitness = true;
            hindersAttackTarget = true;
            hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                //character.IsNormalCharacter()
                character.jobComponent.TriggerBuryMe();
                if(responsibleCharacter != null) {
                    responsibleCharacter.combatComponent.AdjustNumOfKilledCharacters(1);
                } else if (responsibleCharacters != null) {
                    for (int i = 0; i < responsibleCharacters.Count; i++) {
                        Character responsible = responsibleCharacters[i];
                        responsible.combatComponent.AdjustNumOfKilledCharacters(1);
                    }
                }
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character owner = removedFrom as Character;
                owner.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY);
            }
        }
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return descriptionInUI;
            }
            return $"This character was killed by {responsibleCharacter.name}";
        }
        #endregion
    }

    //public class SaveDataDead : SaveDataTrait {
    //    public List<int> characterIDsThatSawThisDead;

    //    public override void Save(Trait trait) {
    //        base.Save(trait);
    //        Dead dead = trait as Dead;
    //        characterIDsThatSawThisDead = new List<int>();
    //        for (int i = 0; i < dead.charactersThatSawThisDead.Count; i++) {
    //            characterIDsThatSawThisDead.Add(dead.charactersThatSawThisDead[i].id);
    //        }
    //    }

    //    public override Trait Load(ref Character responsibleCharacter) {
    //        Trait trait = base.Load(ref responsibleCharacter);
    //        Dead dead = trait as Dead;
    //        for (int i = 0; i < characterIDsThatSawThisDead.Count; i++) {
    //            dead.AddCharacterThatSawThisDead(CharacterManager.Instance.GetCharacterByID(characterIDsThatSawThisDead[i]));
    //        }
    //        return trait;
    //    }
    //}
}