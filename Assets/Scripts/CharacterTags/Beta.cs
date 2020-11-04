using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class Beta : CharacterTag {
    public Beta(Character character) : base(character, CHARACTER_TAG.BETA) {

    }

    #region overrides
    /*
     At the end of a day in a landmark, if there is another character with an Alpha Tag who is unpaired, 
     in a Neutral Stance, and is the lead of an incomplete Party, 
     a character with this tag who is currently in Do Nothing state will have a chance to join his party.
     NOTE: This should only be called when the character is in a Do Nothing State
         */
    public override void PerformDailyAction() {
        base.PerformDailyAction();
        if (character.party != null) {
            return; //this character is already in a party
        }
        List<Character> charactersAtLocation = new List<Character>(character.specificLocation.charactersAtLocation);
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            ECS.Character currCharacter = charactersAtLocation[i];
            if (currCharacter.HasTag(CHARACTER_TAG.ALPHA) && currCharacter.GetCurrentStance() == STANCE.NEUTRAL 
                && !currCharacter.isInCombat && (currCharacter.party == null || !currCharacter.party.isFull) && currCharacter.id != character.id) {
                if (ShouldJoinParty(currCharacter)) {
                    if (currCharacter.party == null) {
                        currCharacter.CreateNewParty();
                    }
                    currCharacter.party.AddPartyMember(character);
                    break;
                }
            }
        }
    }
    #endregion

    private bool ShouldJoinParty(ECS.Character alpha) {
        WeightedDictionary<bool> joinPartyWeights = new WeightedDictionary<bool>();
        int dontJoin = 100; //100 Weight Dont Join
        int join = 20; //20 Weight Join
        if (alpha.faction != null && character.faction != null) {
            if (alpha.faction.id == character.faction.id) {
                join += 50; //+50 Weight Join if Alpha is same faction
            }
        } else {
            join += 50;//+50 Weight Join if both Alpha and Beta are Unaligned
        }
        //TODO: +30 Weight Join for every Positive relationship
        //TODO: +30 Weight Join if there is a Familial relationship
        //TODO: +100 Weight Dont Join if there is a Negative relationship

        joinPartyWeights.AddElement(false, dontJoin);
        joinPartyWeights.AddElement(true, join);
        return joinPartyWeights.PickRandomElementGivenWeights();
    }
}
