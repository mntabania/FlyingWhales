using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

namespace Traits {
    public class Dead : Status {
        public List<Character> charactersThatSawThisDead { get; private set; }
        public Dead() {
            name = "Dead";
            description = "This character's life has been extinguished.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_FOOD };
            charactersThatSawThisDead = new List<Character>();
            hindersMovement = true;
            hindersWitness = true;
            hindersAttackTarget = true;
            hindersPerform = true;
        }

        #region General
        public void AddCharacterThatSawThisDead(Character character) {
            charactersThatSawThisDead.Add(character);
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                //character.IsNormalCharacter
                if (character.minion == null && character.gridTileLocation != null && character.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement)
                    && settlement is NPCSettlement npcSettlement) {
                    LocationStructure targetStructure = npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ??
                                                        npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                    GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, character, npcSettlement);
                    buryJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanTakeBuryJob);
                    buryJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[]{ targetStructure });
                    buryJob.SetStillApplicableChecker(() => IsBuryJobStillApplicable(character, npcSettlement));
                    npcSettlement.AddToAvailableJobs(buryJob);
                }
            }
        }
        private bool IsBuryJobStillApplicable(Character target, NPCSettlement npcSettlement) {
            return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(npcSettlement);
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

    public class SaveDataDead : SaveDataTrait {
        public List<int> characterIDsThatSawThisDead;

        public override void Save(Trait trait) {
            base.Save(trait);
            Dead dead = trait as Dead;
            characterIDsThatSawThisDead = new List<int>();
            for (int i = 0; i < dead.charactersThatSawThisDead.Count; i++) {
                characterIDsThatSawThisDead.Add(dead.charactersThatSawThisDead[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Dead dead = trait as Dead;
            for (int i = 0; i < characterIDsThatSawThisDead.Count; i++) {
                dead.AddCharacterThatSawThisDead(CharacterManager.Instance.GetCharacterByID(characterIDsThatSawThisDead[i]));
            }
            return trait;
        }
    }
}