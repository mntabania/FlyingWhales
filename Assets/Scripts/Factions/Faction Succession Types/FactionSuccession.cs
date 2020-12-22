using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
using Traits;

namespace Factions.Faction_Succession {
    public class FactionSuccession {
        public FACTION_SUCCESSION_TYPE type { get; protected set; }
        public string name { get; protected set; }

        public FactionSuccession(FACTION_SUCCESSION_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }

        #region Virtuals
        public virtual void PopulateSuccessorListWeightsInOrder(List<Character> successorList, WeightedDictionary<Character> weightedDictionary, Faction faction) { }
        public virtual Character PickSuccessor(Character[] successors, int[] weights) {
            int totalOfAllWeights = weights.Sum();
            int chance = GameUtilities.RandomBetweenTwoNumbers(0, totalOfAllWeights);
            int upperBound = 0;
            int lowerBound = 0;
            for (int i = 0; i < weights.Length; i++) {
                int weightOfCurrElement = weights[i];
                upperBound += weightOfCurrElement;
                if (chance >= lowerBound && chance < upperBound) {
                    return successors[i];
                }
                lowerBound = upperBound;
            }

            //If the code goes here it means that there are no picked successor from the weights
            //It is either no successors or all successors has 0 weights
            //If there is a successor but the weight is 0, just pick the first successor in list
            for (int i = 0; i < successors.Length; i++) {
                Character character = successors[i];
                if(character != null) {
                    return character;
                }
            }
            return null;
        }
        #endregion

        protected bool CanBeCandidateForSuccession(Character character, Faction faction) {
            if (character.isDead /*|| character.isMissing*/ || character.isBeingSeized || character.isInLimbo || character.isFactionLeader) {
                return false;
            }
            return true;
        }
    }
}