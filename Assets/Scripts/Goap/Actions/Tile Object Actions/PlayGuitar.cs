using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class PlayGuitar : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public PlayGuitar() : base(INTERACTION_TYPE.PLAY_GUITAR) {
        //validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT, };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        // showNotification = false;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Play Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
#if DEBUG_LOG
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return 2000;
                    }
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Times Played > 5)";
#endif
        }

        if (target.gridTileLocation != null) {
            if (actor.trapStructure.IsTrapped()) {
                if (actor.trapStructure.IsTrapStructure(target.gridTileLocation.structure)) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += " +2000(Actor trapped and guitar is not at trap structure)";
#endif
                }
            } else {
                if (target.gridTileLocation.structure != actor.homeStructure) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += " +2000(Actor is not trapped and guitar is not at home)";
#endif
                }
            }
        }

        if (actor.traitContainer.HasTrait("Music Lover")) {
            cost += -25;
#if DEBUG_LOG
            costLog += " -25(Music Lover)";
#endif
        }
        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost}(10 x Times Played)";

        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        //actor.needsComponent.AdjustDoNotGetBored(-1);
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Play Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        Trait trait = witness.traitContainer.GetTraitOrStatus<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                if (witness.HasAfflictedByPlayerWith(trait)) {
                    PLAYER_SKILL_TYPE playerSkillType = trait.GetAfflictionSkillType();
                    if (playerSkillType != PLAYER_SKILL_TYPE.NONE) {
                        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerSkillType);
                        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(playerSkillType);
                        if (playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Angry_Upon_Hear_Music, skillData.currentLevel)) {
                            reactions.Add(EMOTION.Anger);
                        }
                    }
                }
                if (witness.moodComponent.moodState == MOOD_STATE.Bad) {
                    if (!reactions.Contains(EMOTION.Anger)) {
                        reactions.Add(EMOTION.Anger);    
                    }
                } else if (witness.moodComponent.moodState == MOOD_STATE.Critical) {
                    reactions.Add(EMOTION.Rage);
                } else {
                    reactions.Add(EMOTION.Disapproval);
                }
            } else {
                reactions.Add(EMOTION.Approval);
                if (RelationshipManager.Instance.GetCompatibilityBetween(witness, actor) >= 4 && RelationshipManager.IsSexuallyCompatible(witness, actor) &&
                   witness.moodComponent.moodState != MOOD_STATE.Critical) {
                    int value = 50;
                    if (actor.traitContainer.HasTrait("Unattractive")) {
                        value = 20;
                    }
                    if (UnityEngine.Random.Range(0, 100) < value) {
                        reactions.Add(EMOTION.Arousal);
                    }
                }
            }
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        MusicHater musicHater = witness.traitContainer.GetTraitOrStatus<MusicHater>("Music Hater");
        musicHater?.ReactToMusicPerformer(witness, actor);
        return base.ReactionToActor(actor, target, witness, node, status);
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (witness.traitContainer.HasTrait("Music Hater")) {
            return REACTABLE_EFFECT.Negative;
        } else if (witness.traitContainer.HasTrait("Music Lover")) {
            return REACTABLE_EFFECT.Positive;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region State Effects
    public void PrePlaySuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
    }
    public void PerTickPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(3.34f);
    }
    public void AfterPlaySuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    //public void PreTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            if (actor.traitContainer.HasTrait("Music Hater")) {
                return false; //music haters will never play guitar
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            // LocationGridTile knownLoc = poiTarget.gridTileLocation;
            // //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
            // if (knownLoc.structure.isDwelling) {
            //     if (actor.homeStructure == knownLoc.structure) {
            //         return true;
            //     } else {
            //         IDwelling dwelling = knownLoc.structure as IDwelling;
            //         if (dwelling.IsOccupied()) {
            //             for (int i = 0; i < dwelling.residents.Count; i++) {
            //                 Character currResident = dwelling.residents[i];
            //                 if (currResident.RelationshipManager.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
            //                     return true;
            //                 }
            //             }
            //             //the actor does NOT have any positive relations with any resident
            //             return false;
            //         } else {
            //             //in cases that the guitar is at a dwelling with no residents, always allow.
            //             return true;
            //         }
            //     }
            // } else {
            //     //in cases that the guitar is not inside a dwelling, always allow.
            //     return true;
            // }
            return true;
        } 
        return false;
    }
#endregion
}