using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logs;
using UnityEngine;  
using Traits;

public class JudgeCharacter : GoapAction {

    public JudgeCharacter() : base(INTERACTION_TYPE.JUDGE_CHARACTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes, LOG_TAG.Crimes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Judge Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void PreJudgeSuccess(ActualGoapNode goapNode) {
        WeightedDictionary<string> weights = new WeightedDictionary<string>();
        Character targetCharacter = goapNode.poiTarget as Character;
        Character actor = goapNode.actor;
        Criminal criminalTrait = targetCharacter.traitContainer.GetNormalTrait<Criminal>("Criminal");
        FactionRelationship factionRelationship = actor.faction.GetRelationshipWith(targetCharacter.faction); //Will only be null if target and actor HAVE THE SAME FACTION
        string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);

        //TODO: Loop all through crime and get weight result of each, at the end the highest count result will be the ultimate punishment
        //Right now just get the first crime
        CrimeData crimeData = null;
        List<CrimeData> allCrimesToFaction = null;
        if (criminalTrait != null) {
            allCrimesToFaction = criminalTrait.GetListOfUnpunishedCrimesWantedBy(actor.faction);
            if (allCrimesToFaction != null && allCrimesToFaction.Count > 0) {
                crimeData = allCrimesToFaction[0];
            }
        }

        if (crimeData != null) {
            string debugLog = $"{actor.name} is going to judge {targetCharacter.name}";

            int absolve = 0;
            int whip = 0;
            int kill = 0;
            int exile = 0;

            //Base Weights
            if ((factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) || crimeData == null) {
                whip = 5;
                kill = 100;
                exile = 10;
                debugLog += "\n-Hostile Faction or No Crime Data: absolve = 0, whip = 5, kill = 100, exile = 10";
            } else {
                if (crimeData.crimeSeverity == CRIME_SEVERITY.Misdemeanor) {
                    absolve = 50;
                    whip = 100;
                    debugLog += "\n-Misdemeanor: absolve = 50, whip = 100, kill = 0, exile = 0";
                } else if (crimeData.crimeSeverity == CRIME_SEVERITY.Serious) {
                    absolve = 5;
                    whip = 20;
                    kill = 50;
                    exile = 50;
                    debugLog += "\n-Serious Crime: absolve = 5, whip = 20, kill = 50, exile = 50";
                } else if (crimeData.crimeSeverity == CRIME_SEVERITY.Heinous) {
                    whip = 5;
                    kill = 100;
                    exile = 50;
                    debugLog += "\n-Heinous Crime: absolve = 0, whip = 5, kill = 100, exile = 50";
                }
            }

            //Modifiers
            if (targetCharacter.faction == actor.faction) {
                absolve = Mathf.RoundToInt(absolve * 1.5f);
                whip = Mathf.RoundToInt(whip * 1.5f);
                debugLog += "\n-Same Faction: absolve = x1.5, whip = x1.5, kill = x1, exile = x1";
            } else {
                if (factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Neutral) {
                    absolve = Mathf.RoundToInt(absolve * 0.5f);
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill = Mathf.RoundToInt(kill * 2f);
                    exile *= 2;
                    debugLog += "\n-Cold War Faction: absolve = x0.5, whip = x0.5, kill = x1.5, exile = x2";
                } else if (factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                    absolve = Mathf.RoundToInt(absolve * 0.2f);
                    whip = Mathf.RoundToInt(whip * 0.5f);
                    kill *= 3;
                    exile *= 0;
                    debugLog += "\n-Hostile Faction: absolve = x0.2, whip = x0.5, kill = x2, exile = x1.5";
                }
            }

            if (opinionLabel == RelationshipManager.Close_Friend) {
                absolve *= 3;
                whip *= 2;
                kill *= 0;
                exile = Mathf.RoundToInt(exile * 0.5f);
                debugLog += "\n-Close Friend: absolve = x3, whip = x2, kill = x0, exile = x0.2";
            } else if (opinionLabel == RelationshipManager.Friend) {
                absolve *= 2;
                whip *= 2;
                kill = Mathf.RoundToInt(kill * 0.1f);
                exile = Mathf.RoundToInt(exile * 0.5f);
                debugLog += "\n-Friend: absolve = x2, whip = x2, kill = x0.1, exile = x0.5";
            } else if (opinionLabel == RelationshipManager.Enemy) {
                absolve = Mathf.RoundToInt(absolve * 0.1f);
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 2;
                exile = Mathf.RoundToInt(exile * 1.5f);
                debugLog += "\n-Enemy: absolve = x0.1, whip = x0.5, kill = x2, exile = x1.5";
            } else if (opinionLabel == RelationshipManager.Rival) {
                absolve *= 0;
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 3;
                exile = Mathf.RoundToInt(exile * 1.5f);
                debugLog += "\n-Rival: absolve = x0, whip = x0.5, kill = x3, exile = x1.5";
            }

            weights.AddElement("Absolve", absolve);
            weights.AddElement("Whip", whip);
            weights.AddElement("Execute", kill);
            weights.AddElement("Exile", exile);

            debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";

            string chosen = weights.PickRandomElementGivenWeights();
            debugLog += $"\n\n{chosen}";
            actor.logComponent.PrintLogIfActive(debugLog);

            goapNode.descriptionLog.AddToFillers(null, chosen.ToLower(), LOG_IDENTIFIER.STRING_1);
            //CreateJudgeLog(goapNode, chosen);
            CRIME_STATUS decision = CRIME_STATUS.Absolved;
            if (chosen == "Absolve") {
                decision = CRIME_STATUS.Absolved;
                TargetAbsolved(goapNode);
            } else if (chosen == "Whip") {
                decision = CRIME_STATUS.Punished;
                TargetWhip(goapNode);
            } else if (chosen == "Execute") {
                decision = CRIME_STATUS.Executed;
                TargetExecuted(goapNode);
            } else if (chosen == "Exile") {
                decision = CRIME_STATUS.Exiled;
                TargetExiled(goapNode);
            }
            if(allCrimesToFaction != null) {
                for (int i = 0; i < allCrimesToFaction.Count; i++) {
                    CrimeData crimeToFaction = allCrimesToFaction[i];
                    crimeToFaction.SetCrimeStatus(decision);
                    crimeToFaction.SetJudge(actor);
                }
            }
        }

        //if (crimeData != null) { crimeData.SetCrimeStatus(CRIME_STATUS.Exiled); }
        //TargetExiled(goapNode);
    }
    private void TargetExecuted(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXECUTE, goapNode.target, goapNode.actor);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetAbsolved(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.ABSOLVE, goapNode.target, goapNode.actor);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetExiled(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.EXILE, goapNode.target, goapNode.actor);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    private void TargetWhip(ActualGoapNode goapNode) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.WHIP, goapNode.target, goapNode.actor);
        goapNode.actor.jobQueue.AddJobInQueue(job);
    }
    #endregion

    private void CreateJudgeLog(ActualGoapNode goapNode, string result) {
        Log log = new Log(GameManager.Instance.Today(), "GoapAction", goapName, "judge result", goapNode, LOG_TAG.Crimes, LOG_TAG.Life_Changes, LOG_TAG.Work);
        // if (goapNode != null) {
        //     log.SetLogType(LOG_TYPE.Action);
        // }
        log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, result, LOG_IDENTIFIER.STRING_1);
        goapNode.OverrideDescriptionLog(log);
    }
    
}