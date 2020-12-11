using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Factions.Faction_Succession {
    public class Power: FactionSuccession {

        public Power() : base (FACTION_SUCCESSION_TYPE.Power) { }

        #region Overrides
        public override void PopulateSuccessorListWeightsInOrder(List<Character> successorList, WeightedDictionary<Character> weightedDictionary, Faction faction) {
            base.PopulateSuccessorListWeightsInOrder(successorList, weightedDictionary, faction);
            for (int i = 0; i < faction.characters.Count; i++) {
                Character member = faction.characters[i];
                if (!CanBeCandidateForSuccession(member, faction)) {
                    continue;
                }
                int weight = 0;

                //Base
                weight = GameUtilities.RandomBetweenTwoNumbers(40, 60);

                //TODO: per kill of the character: +20-30

                //if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                //    //TODO: if character is a known Vampire
                //    weight += 100;
                //}
                //if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                //    //TODO: if character is a known Lycan
                //    weight += 100;
                //}

                if (member.characterClass.IsCombatant()) {
                    weight += GameUtilities.RandomBetweenTwoNumbers(50, 70);
                }
                if (member.traitContainer.HasTrait("Mighty")) {
                    weight += 50;
                }
                if (member.traitContainer.HasTrait("Ruthless")) {
                    weight += 50;
                }
                if (member.traitContainer.HasTrait("Authoritative")) {
                    weight += 50;
                }
                if (faction.factionType.IsCivilian(member.characterClass.className)) {
                    weight *= 0;
                }
                if (member.traitContainer.HasTrait("Enslaved")) {
                    weight *= 0;
                }
                if (member.crimeComponent.IsWantedBy(faction)) {
                    weight *= 0;
                }

                if (weight < 0) {
                    weight = 0;
                }

                weightedDictionary.AddElement(member, weight);

                if(successorList.Count > 0) {
                    bool hasInserted = false;
                    for (int j = 0; j < successorList.Count; j++) {
                        int currWeight = weightedDictionary.GetElementWeight(successorList[j]);
                        if (weight > currWeight) {
                            hasInserted = true;
                            successorList.Insert(j, member);
                            break;
                        }
                    }
                    if (!hasInserted) {
                        successorList.Add(member);
                    }
                } else {
                    successorList.Add(member);
                }
            }
        }
        #endregion
    }
}