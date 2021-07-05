using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logs;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class JudgeCharacter : GoapAction {

    public JudgeCharacter() : base(INTERACTION_TYPE.JUDGE_CHARACTER) {
        actionIconString = GoapActionStateDB.Judge_Icon;
        doesNotStopTargetCharacter = true;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes, LOG_TAG.Crimes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Judge Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion

#region State Effects
    public void PreJudgeSuccess(ActualGoapNode goapNode) {
        WeightedDictionary<string> weights = new WeightedDictionary<string>();
        Character targetCharacter = goapNode.poiTarget as Character;
        Character actor = goapNode.actor;
        //Criminal criminalTrait = targetCharacter.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
        FactionRelationship factionRelationship = actor.faction.GetRelationshipWith(targetCharacter.faction); //Will only be null if target and actor HAVE THE SAME FACTION
        string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);

        //TODO: Loop all through crime and get weight result of each, at the end the highest count result will be the ultimate punishment
        //Right now just get the first crime
        CrimeData crimeData = targetCharacter.crimeComponent.GetFirstCrimeWantedBy(actor.faction, CRIME_STATUS.Unpunished);

        if (crimeData != null) {
#if DEBUG_LOG
            string debugLog = $"{actor.name} is going to judge {targetCharacter.name}";
#endif

            int absolve = 0;
            int whip = 0;
            int kill = 0;
            int exile = 0;

            //Base Weights
            if ((factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) || crimeData == null) {
                whip = 5;
                kill = 100;
                exile = 10;
#if DEBUG_LOG
                debugLog += "\n-Hostile Faction or No Crime Data: absolve = 0, whip = 5, kill = 100, exile = 10";
#endif
            } else {
                if (crimeData.crimeSeverity == CRIME_SEVERITY.Misdemeanor) {
                    absolve = 50;
                    whip = 100;
#if DEBUG_LOG
                    debugLog += "\n-Misdemeanor: absolve = 50, whip = 100, kill = 0, exile = 0";
#endif
                } else if (crimeData.crimeSeverity == CRIME_SEVERITY.Serious) {
                    absolve = 5;
                    whip = 20;
                    kill = 50;
                    exile = 50;
#if DEBUG_LOG
                    debugLog += "\n-Serious Crime: absolve = 5, whip = 20, kill = 50, exile = 50";
#endif
                } else if (crimeData.crimeSeverity == CRIME_SEVERITY.Heinous) {
                    whip = 5;
                    kill = 100;
                    exile = 50;
#if DEBUG_LOG
                    debugLog += "\n-Heinous Crime: absolve = 0, whip = 5, kill = 100, exile = 50";
#endif
                }
            }

            //Modifiers
            if (targetCharacter.faction == actor.faction) {
                absolve = Mathf.RoundToInt(absolve * 1.5f);
                whip = Mathf.RoundToInt(whip * 1.5f);
#if DEBUG_LOG
                debugLog += "\n-Same Faction: absolve = x1.5, whip = x1.5, kill = x1, exile = x1";
#endif
            }

            if (opinionLabel == RelationshipManager.Close_Friend) {
                absolve *= 3;
                whip *= 2;
                kill *= 0;
                exile = Mathf.RoundToInt(exile * 0.5f);
#if DEBUG_LOG
                debugLog += "\n-Close Friend: absolve = x3, whip = x2, kill = x0, exile = x0.5";
#endif
            } else if (opinionLabel == RelationshipManager.Friend) {
                absolve *= 2;
                whip *= 2;
                kill = Mathf.RoundToInt(kill * 0.1f);
                exile = Mathf.RoundToInt(exile * 0.5f);
#if DEBUG_LOG
                debugLog += "\n-Friend: absolve = x2, whip = x2, kill = x0.1, exile = x0.5";
#endif
            } else if (opinionLabel == RelationshipManager.Enemy) {
                absolve = Mathf.RoundToInt(absolve * 0.1f);
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 2;
                exile = Mathf.RoundToInt(exile * 1.5f);
#if DEBUG_LOG
                debugLog += "\n-Enemy: absolve = x0.1, whip = x0.5, kill = x2, exile = x1.5";
#endif
            } else if (opinionLabel == RelationshipManager.Rival) {
                absolve *= 0;
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 3;
                exile = Mathf.RoundToInt(exile * 1.5f);
#if DEBUG_LOG
                debugLog += "\n-Rival: absolve = x0, whip = x0.5, kill = x3, exile = x1.5";
#endif
            }

            if (actor.traitContainer.HasTrait("Ruthless")) {
                absolve *= 0;
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 2;
                exile *= 1;
#if DEBUG_LOG
                debugLog += "\n-Ruthless judge: absolve = x0, whip = x0.5, kill = x2, exile = x1";
#endif
            }

            if (crimeData.crimeType == CRIME_TYPE.Plagued && actor.homeSettlement != null) {
                Locations.Settlements.Settlement_Events.PlaguedEvent plaguedEventEvent = actor.homeSettlement.eventManager.GetActiveEvent<Locations.Settlements.Settlement_Events.PlaguedEvent>();
                if (plaguedEventEvent != null) {
                    switch (plaguedEventEvent.rulerDecision) {
                        case PLAGUE_EVENT_RESPONSE.Slay:
                            absolve *= 0;
                            whip *= 0;
                            kill *= 1;
                            exile = Mathf.RoundToInt(exile * 0.2f);
#if DEBUG_LOG
                            debugLog += "\n-Plagued-Slay: absolve = x0, whip = x0, kill = x1, exile = x0.2";
#endif
                            break;
                        case PLAGUE_EVENT_RESPONSE.Exile:
                            absolve *= 0;
                            whip *= 0;
                            kill = Mathf.RoundToInt(kill * 0.2f);
                            exile *= 1;
#if DEBUG_LOG
                            debugLog += "\n-Plagued-Exile: absolve = x0, whip = x0, kill = x0.2, exile = x1";
#endif
                            break;
                    }
                }
            }
            
            if(crimeData.crimeType == CRIME_TYPE.Vampire) {
                if (actor.traitContainer.HasTrait("Hemophobic")) {
                    absolve *= 0;
                    whip *= 0;
                    kill *= 2;
                    exile *= 1;
#if DEBUG_LOG
                    debugLog += "\n-Vampire prisoner, Hemophobic: absolve = x0, whip = x0, kill = x2, exile = x1";
#endif
                } else if (actor.traitContainer.HasTrait("Hemophiliac")) {
                    absolve *= 3;
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill *= 0;
                    exile = Mathf.RoundToInt(whip * 0.5f);
#if DEBUG_LOG
                    debugLog += "\n-Vampire prisoner, Hemophiliac: absolve = x3, whip = x0.5, kill = x0, exile = x0.5";
#endif
                }
            } else if (crimeData.crimeType == CRIME_TYPE.Werewolf) {
                if (actor.traitContainer.HasTrait("Lycanphobic")) {
                    absolve *= 0;
                    whip *= 0;
                    kill *= 2;
                    exile *= 1;
#if DEBUG_LOG
                    debugLog += "\n-Werewolf prisoner, Lycanphobic: absolve = x0, whip = x0, kill = x2, exile = x1";
#endif
                } else if (actor.traitContainer.HasTrait("Lycanphiliac")) {
                    absolve *= 3;
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill *= 0;
                    exile = Mathf.RoundToInt(whip * 0.5f);
#if DEBUG_LOG
                    debugLog += "\n-Werewolf prisoner, Lycanphiliac: absolve = x3, whip = x0.5, kill = x0, exile = x0.5";
#endif
                }
            }

            if (targetCharacter.faction != actor.faction) {
                if (factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Neutral) {
                    absolve = Mathf.RoundToInt(absolve * 0.5f);
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill = Mathf.RoundToInt(kill * 2f);
                    exile *= 0;
#if DEBUG_LOG
                    debugLog += "\n-Neutral Faction: absolve = x0.5, whip = x0.5, kill = x2, exile = x0";
#endif
                } else if (factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                    absolve = Mathf.RoundToInt(absolve * 0.2f);
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill *= 3;
                    exile *= 0;
#if DEBUG_LOG
                    debugLog += "\n-Hostile Faction: absolve = x0.2, whip = x0.5, kill = x3, exile = x0";
#endif
                }
            }

            weights.AddElement("Absolve", absolve);
            weights.AddElement("Whip", whip);
            weights.AddElement("Execute", kill);
            weights.AddElement("Exile", exile);
#if DEBUG_LOG
            debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";
#endif

            string chosen = weights.PickRandomElementGivenWeights();
#if DEBUG_LOG
            debugLog += $"\n\n{chosen}";
            actor.logComponent.PrintLogIfActive(debugLog);
#endif

            if (chosen == "Absolve") {
                TargetAbsolved(goapNode);
            } else if (chosen == "Whip") {
                TargetWhip(goapNode);
            } else if (chosen == "Execute") {
                if (GameUtilities.RollChance(50) && !goapNode.actor.traitContainer.HasTrait("Pyrophobic")) {
                    TargetBurnAtStake(goapNode);
                } else {
                    TargetExecuted(goapNode);
                }
            } else if (chosen == "Exile") {
                TargetExiled(goapNode);
            }
        }

    }
    private void TargetExecuted(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXECUTE, goapNode.target, goapNode.actor);
        job.SetCannotBePushedBack(true);
        job.SetDoNotRecalculate(true);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetAbsolved(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.ABSOLVE, goapNode.target, goapNode.actor);
        job.SetCannotBePushedBack(true);
        job.SetDoNotRecalculate(true);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetExiled(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXILE, goapNode.target, goapNode.actor);
        job.SetCannotBePushedBack(true);
        job.SetDoNotRecalculate(true);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetWhip(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.WHIP, goapNode.target, goapNode.actor);
        job.SetCannotBePushedBack(true);
        job.SetDoNotRecalculate(true);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetBurnAtStake(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;

        LocationStructure targetStructure = null;
        LocationGridTile targetTile = null;
        if(actor.currentRegion != null) {
            targetStructure = actor.currentRegion.wilderness;
        }
        if(targetStructure != null) {
            if (actor.homeSettlement != null) {
                targetTile = CollectionUtilities.GetRandomElement(targetStructure.unoccupiedTiles.Where(tile => tile.IsNextToSettlement(actor.homeSettlement) && actor.movementComponent.HasPathToEvenIfDiffRegion(tile)));
            } else if (goapNode.poiTarget.gridTileLocation != null) {
                targetTile = goapNode.poiTarget.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
            } else if (actor.gridTileLocation != null) {
                targetTile = actor.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
            }
        }
        if(targetStructure != null && targetTile != null) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.BURN_AT_STAKE, goapNode.target, goapNode.actor);
            job.SetCannotBePushedBack(true);
            job.SetDoNotRecalculate(true);

            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { targetStructure, targetTile });
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { targetStructure, targetTile });
            goapNode.actor.jobQueue.AddJobInQueue(job);
        } else {
            //Fall back if actor cannot burn at stake, just execute prisoner
            TargetExecuted(goapNode);
        }
    }
#endregion

    private void CreateJudgeLog(ActualGoapNode goapNode, string result) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "judge result", goapNode, logTags);
        log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, result, LOG_IDENTIFIER.STRING_1);
        goapNode.OverrideDescriptionLog(log);
    }
    
}