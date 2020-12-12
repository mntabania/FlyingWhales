using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Traits;

namespace Factions.Faction_Succession {
    public class Popularity : FactionSuccession {

        public Popularity() : base (FACTION_SUCCESSION_TYPE.Popularity) { }

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

                if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                    Vampire vampire = member.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                    if(vampire != null && vampire.DoesFactionKnowThisVampire(faction, false)) {
                        weight += 100;
                    }
                }
                if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                    if (member.isLycanthrope && member.lycanData.DoesFactionKnowThisLycan(faction)) {
                        weight += 100;
                    }
                }

                if (member.characterClass.className == "Noble") {
                    weight += 40;
                }

                for (int j = 0; j < faction.characters.Count; j++) {
                    Character currMember = faction.characters[j];
                    if(currMember != member && !currMember.isDead) {
                        if (currMember.relationshipContainer.IsFriendsWith(member)) {
                            weight += GameUtilities.RandomBetweenTwoNumbers(40, 50);
                        } else if (currMember.relationshipContainer.IsEnemiesWith(member)) {
                            weight += -20;
                        }
                    }
                }
                if (member.traitContainer.HasTrait("Inspiring")) {
                    weight += 25;
                }
                if (member.traitContainer.HasTrait("Authoritative")) {
                    weight += 50;
                }
                if (member.traitContainer.HasTrait("Unattractive")) {
                    weight += -20;
                }
                if (member.hasUnresolvedCrime) {
                    weight += -50;
                }
                if (faction.factionType.IsCivilian(member.characterClass.className)) {
                    weight += -40;
                }
                if (member is Summon || member.characterClass.IsZombie()) {
                    if (faction.HasMemberThatMeetCriteria(c => c.race.IsSapient() && (c.IsAtHome() || c.partyComponent.isMemberThatJoinedQuest))) {
                        weight *= 0;
                    }
                }
                if (member.traitContainer.HasTrait("Enslaved")) {
                    weight *= 0;
                }
                if (member.crimeComponent.IsWantedBy(faction)) {
                    weight *= 0;
                }

                if(weight < 0) {
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