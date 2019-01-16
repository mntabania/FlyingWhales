﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToSpreadUndeath : Interaction {

    private const string Undeath_Cancelled = "Undeath Cancelled";
    private const string Undeath_Proceeds = "Undeath Proceeds";
    private const string Normal_Undeath = "Normal Undeath";

    public Area targetLocation { get; private set; }

    public MoveToSpreadUndeath(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_SPREAD_UNDEATH, 0) {
        _name = "Move To Spread Undeath";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState undeathCancelled = new InteractionState(Undeath_Cancelled, this);
        InteractionState undeathProceeds = new InteractionState(Undeath_Proceeds, this);
        InteractionState normalUndeath = new InteractionState(Normal_Undeath, this);

        targetLocation = GetTargetLocation(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        undeathCancelled.SetEffect(() => UndeathCancelledRewardEffect(undeathCancelled));
        undeathProceeds.SetEffect(() => UndeathProceedsRewardEffect(undeathProceeds));
        normalUndeath.SetEffect(() => NormalUndeathRewardEffect(normalUndeath));

        _states.Add(startState.name, startState);
        _states.Add(undeathCancelled.name, undeathCancelled);
        _states.Add(undeathProceeds.name, undeathProceeds);
        _states.Add(normalUndeath.name, normalUndeath);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
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
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Undeath_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Undeath_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Undeath]);
    }
    #endregion

    #region Reward Effects
    private void UndeathCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
        //if (state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
        //state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void UndeathProceedsRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void NormalUndeathRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateEvent());
    }

    private void CreateEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SPREAD_UNDEATH_ACTION, targetLocation.coreTile.landmarkOnTile);
        _characterInvolved.SetForcedInteraction(interaction);
    }

    private Area GetTargetLocation(Character character) {
        /* A character travels to an area that his faction does not own that has at least one character not part of his faction. This action may be done by Skeletons
         * 
         * From all locations that has at least one character not part of same faction, choose via weight based:
            - Location is not owned by any faction: Weight +15
            - Location is owned by a Faction that is Enemy or Disliked by Abductor's Faction: Weight +25
            - Location is owned by a Faction with Neutral relationship with Abductor's Faction: Weight +15
            - Location is owned by a Faction with Friend relationship with Abductor's Faction: Weight +5
            - Location is owned by a Faction with Ally relationship with Abductor's Faction: Weight +2 */
        WeightedDictionary<Area> choices = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.owner != null && currArea.owner.id == character.faction.id) {
                continue; //skip
            }
            if (currArea.HasCharacterThatIsNotFromFaction(character.faction)) {
                int weight = 0;

                if (currArea.owner == null) {
                    weight += 15; //Location is not owned by any faction: Weight +15
                } else {
                    FactionRelationship rel = currArea.owner.GetRelationshipWith(character.faction);
                    switch (rel.relationshipStatus) {
                        case FACTION_RELATIONSHIP_STATUS.ENEMY:
                        case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                            weight += 25;
                            break;
                        case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                            weight += 15;
                            break;
                        case FACTION_RELATIONSHIP_STATUS.FRIEND:
                            weight += 5;
                            break;
                        case FACTION_RELATIONSHIP_STATUS.ALLY:
                            weight += 2;
                            break;
                    }
                }

                if (weight > 0) {
                    choices.AddElement(currArea, weight);
                }
            }
        }

        if (choices.GetTotalOfWeights() > 0) {
            return choices.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find target location for move to charm of " + _characterInvolved.faction.name);
    }
}
