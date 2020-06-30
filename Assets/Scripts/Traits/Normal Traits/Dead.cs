using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

namespace Traits {
    public class Dead : Status {
        public override bool isSingleton => true;

        public Dead() {
            name = "Dead";
            description = "This character's life has been extinguished.";
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
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character owner = removedFrom as Character;
                owner.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.BURY);
            }
        }
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
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