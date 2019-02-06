﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToRecruit : Interaction {

    private Character _targetCharacter;
    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    private const string Character_Recruit_Cancelled = "Character Recruit Cancelled";
    private const string Character_Recruit_Continues = "Character Recruit Continues";
    private const string Do_Nothing = "Do nothing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.RECRUIT_ACTION; }
    }

    public MoveToRecruit(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_RECRUIT_ACTION, 0) {
        _name = "Move To Recruit";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetCharacterToBeRecruited(Character character) {
        _targetCharacter = character;
        AddToDebugLog("Set target character to " + _targetCharacter.name);
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterRecruitCancelled = new InteractionState(Character_Recruit_Cancelled, this);
        InteractionState characterRecruitContinues = new InteractionState(Character_Recruit_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        if (_targetCharacter != null) {
            _targetArea = _targetCharacter.specificLocation;
        } else {
            _targetArea = GetTargetLocation(_characterInvolved);
        }
        //if (targetCharacter == null) {
        //    targetCharacter = GetTargetCharacter(_characterInvolved);
        //}
        

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        //startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterRecruitCancelled.SetEffect(() => CharacterRecruitCancelledRewardEffect(characterRecruitCancelled));
        characterRecruitContinues.SetEffect(() => CharacterRecruitContinuesRewardEffect(characterRecruitContinues));
        doNothing.SetEffect(() => DoNothingExpandRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(characterRecruitCancelled.name, characterRecruitCancelled);
        _states.Add(characterRecruitContinues.name, characterRecruitContinues);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PreventOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetLocation(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        Interaction interaction = CreateConnectedEvent(INTERACTION_TYPE.RECRUIT_ACTION, _targetArea);
        (interaction as RecruitAction).SetTargetCharacter(_targetCharacter);
    }
    #endregion

    #region Action Option Effects
    private void PreventOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Character_Recruit_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Recruit_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void CharacterRecruitCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void CharacterRecruitContinuesRewardEffect(InteractionState state) {
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void DoNothingExpandRewardEffect(InteractionState state) {
        StartMoveToAction();
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RECRUIT_ACTION, _targetArea);
    //    (interaction as RecruitAction).SetTargetCharacter(_targetCharacter);
    //    //interaction.SetCanInteractionBeDoneAction(() => IsRecruitActionStillValid(interaction as RecruitAction));
    //    _characterInvolved.SetForcedInteraction(interaction);
    //}

    private Area GetTargetLocation(Character characterInvolved) {
        /*
         Location Selection Weights:

        - location is Home of an unaligned character: Weight +35
        - location is Home of a personal Friend that is not from the character's faction: Weight +15
         */
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            if (AreaHasUnalignedResident(currArea)) {
                weight += 35;
            }
            if (AreaHasNonFactionFriendResident(currArea, characterInvolved)) {
                weight += 15;
            }
        }
        if (locationWeights.GetTotalOfWeights() > 0) {
            return locationWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception(GameManager.Instance.TodayLogString() + _characterInvolved.name + " could not find any location to recruit at!");
    }
    private bool AreaHasUnalignedResident(Area area) {
        for (int i = 0; i < area.areaResidents.Count; i++) {
            Character character = area.areaResidents[i];
            if (character.isFactionless) {
                return true;
            }
        }
        return false;
    }
    private bool AreaHasNonFactionFriendResident(Area area, Character characterInvolved) {
        for (int i = 0; i < area.areaResidents.Count; i++) {
            Character character = area.areaResidents[i];
            if (!character.isFactionless 
                && character.faction.id != characterInvolved.faction.id
                && character.GetFriendTraitWith(characterInvolved) != null) {
                return true;
            }
        }
        return false;
    }


    /*
     NOTE: This is for induce only!
         */
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.id != characterInvolved.id 
                && !currCharacter.isDead 
                && !currCharacter.isLeader 
                && !currCharacter.isDefender
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.minion == null) { //- character must not be in Defender Tile.
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 35; //- character is not part of any Faction: Weight +35
                } else if (currCharacter.faction.id != characterInvolved.faction.id) { //exclude characters with same faction
                    FactionRelationship rel = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    //- character is part of a Faction with Neutral relationship with recruiter's Faction: Weight +15
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                        weight += 15;
                    } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight += 25;
                    }
                }

                if (currCharacter.level > characterInvolved.level) {
                    weight -= 30; //- character is higher level than Recruiter: Weight -30
                } else if (currCharacter.level < characterInvolved.level) { //- character is same level as Recruiter: Weight +0
                    weight += 10; //- character is lower level than Recruiter: Weight +10
                }

                weight = Mathf.Max(0, weight);
                characterWeights.AddElement(currCharacter, weight);
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        throw new System.Exception("Could not find any character to recruit!");
    }
}
