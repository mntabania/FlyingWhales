using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Traits;
using Character_Talents;

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
                weight += AdditionalWeightBasedOnSocialTalent(member);

                if (member is Summon || member.characterClass.IsZombie()) {
                    if (faction.HasMemberThatIsSapientAndIsAtHomeOrHasJoinedQuest()) {
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

                if(weight > 0) {
                    //Do not add character as successor if they have 0 or less weight, because they will have 0% chance of becoming faction leader
                    //This might also cause a NaN in the chances because if all successors have 0 weights, then the total weight will be 0
                    //To get the percentage you will have to divide the successor weight from the total weight, so it will be 0/0 = NaN
                    weightedDictionary.AddElement(member, weight);

                    if (successorList.Count > 0) {
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
        }
        #endregion

        private int AdditionalWeightBasedOnSocialTalent(Character p_character) {
            if (p_character.HasTalents()) {
                CharacterTalent talent = p_character.talentComponent.GetTalent(CHARACTER_TALENT.Social);
                int level = talent.level;
                switch (level) {
                    case 1:
                        return 0;
                    case 2:
                        return 50;
                    case 3:
                        return 100;
                    case 4:
                        return 250;
                    case 5:
                        return 250;
                    default:
                        return 0;
                }
            }
            return 0;
        }
    }
}