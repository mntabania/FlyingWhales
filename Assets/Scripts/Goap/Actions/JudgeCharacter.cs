using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class JudgeCharacter : GoapAction {

    public JudgeCharacter() : base(INTERACTION_TYPE.JUDGE_CHARACTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Judge Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
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

        CrimeData crimeData = null;
        if(criminalTrait != null) {
            crimeData = criminalTrait.crimeData;
            crimeData.SetJudge(actor);
        }

        string debugLog = $"{actor.name} is going to judge {targetCharacter.name}";


        int absolve = 0;
        int whip = 0;
        int kill = 0;
        int exile = 0;

        //Base Weights
        if ((factionRelationship != null && factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) || crimeData == null) {
            whip = 5;
            kill = 100;
            exile = 10;
            debugLog += "\n-Hostile Faction or No Crime Data: absolve = 0, whip = 5, kill = 100, exile = 10";
        } else {
            if (crimeData.crimeType == CRIME_TYPE.MISDEMEANOR) {
                absolve = 50;
                whip = 100;
                debugLog += "\n-Misdemeanor: absolve = 50, whip = 100, kill = 0, exile = 0";
            } else if (crimeData.crimeType == CRIME_TYPE.SERIOUS) {
                absolve = 5;
                whip = 20;
                kill = 50;
                exile = 50;
                debugLog += "\n-Serious Crime: absolve = 5, whip = 20, kill = 50, exile = 50";
            } else if (crimeData.crimeType == CRIME_TYPE.HEINOUS) {
                whip = 5;
                kill = 100;
                exile = 50;
                debugLog += "\n-Heinous Crime: absolve = 0, whip = 5, kill = 100, exile = 50";
            }
        }

        //Modifiers
        if(targetCharacter.faction == actor.faction) {
            absolve = Mathf.RoundToInt(absolve * 1.5f);
            whip = Mathf.RoundToInt(whip * 1.5f);
            debugLog += "\n-Same Faction: absolve = x1.5, whip = x1.5, kill = x1, exile = x1";
        } else {
            if (factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.COLD_WAR) {
                absolve = Mathf.RoundToInt(absolve * 0.5f);
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill = Mathf.RoundToInt(kill * 1.5f);
                exile *= 2;
                debugLog += "\n-Cold War Faction: absolve = x0.5, whip = x0.5, kill = x1.5, exile = x2";
            } else if (factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                absolve = Mathf.RoundToInt(absolve * 0.2f);
                whip = Mathf.RoundToInt(whip * 0.5f);
                kill *= 2;
                exile = Mathf.RoundToInt(exile * 1.5f);
                debugLog += "\n-Hostile Faction: absolve = x0.2, whip = x0.5, kill = x2, exile = x1.5";
            }
        }

        if(opinionLabel == OpinionComponent.Close_Friend) {
            absolve *= 3;
            whip *= 2;
            kill *= 0;
            exile = Mathf.RoundToInt(exile * 0.2f);
            debugLog += "\n-Close Friend: absolve = x3, whip = x2, kill = x0, exile = x0.2";
        } else if (opinionLabel == OpinionComponent.Friend) {
            absolve *= 2;
            whip *= 2;
            kill = Mathf.RoundToInt(kill * 0.1f);
            exile = Mathf.RoundToInt(exile * 0.5f);
            debugLog += "\n-Friend: absolve = x2, whip = x2, kill = x0.1, exile = x0.5";
        } else if (opinionLabel == OpinionComponent.Enemy) {
            absolve = Mathf.RoundToInt(absolve * 0.1f);
            whip = Mathf.RoundToInt(whip * 0.5f);
            kill *= 2;
            exile = Mathf.RoundToInt(exile * 1.5f);
            debugLog += "\n-Enemy: absolve = x0.1, whip = x0.5, kill = x2, exile = x1.5";
        } else if (opinionLabel == OpinionComponent.Rival) {
            absolve *= 0;
            whip = Mathf.RoundToInt(whip * 0.5f);
            kill *= 3;
            exile = Mathf.RoundToInt(exile * 1.5f);
            debugLog += "\n-Rival: absolve = x0, whip = x0.5, kill = x3, exile = x1.5";
        }

        weights.AddElement("Absolve", absolve);
        weights.AddElement("Whip", whip);
        weights.AddElement("Kill", kill);
        weights.AddElement("Exile", exile);

        debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";

        string chosen = weights.PickRandomElementGivenWeights();
        debugLog += $"\n\n{chosen}";
        actor.logComponent.PrintLogIfActive(debugLog);
        CreateJudgeLog(goapNode, chosen);

        if (chosen == "Absolve") {
            if(crimeData != null) { crimeData.SetCrimeStatus(CRIME_STATUS.Absolved); }
            TargetReleased(goapNode);
        } else if (chosen == "Whip") {
            if (crimeData != null) { crimeData.SetCrimeStatus(CRIME_STATUS.Punished); }
            TargetWhip(goapNode);
        } else if(chosen == "Kill") {
            TargetExecuted(goapNode);
        } else if (chosen == "Exile") {
            if (crimeData != null) { crimeData.SetCrimeStatus(CRIME_STATUS.Exiled); }
            TargetExiled(goapNode);
        }

        //WeightedFloatDictionary<string> weights = new WeightedFloatDictionary<string>();

        //Character targetCharacter = goapNode.poiTarget as Character;

        //float absolve = 0f;
        //float whip = 0f;
        //float kill = 0f;
        //float exile = 0f;

        ////base weights
        //if (targetCharacter.faction != goapNode.actor.faction && targetCharacter.faction.GetRelationshipWith(goapNode.actor.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
        //    //hostile faction
        //    whip = 5f;
        //    kill = 100f;
        //    exile = 10f;
        //} else {
        //    //criminal traits
        //    List<Criminal> crimes = targetCharacter.traitContainer.GetAllTraitsOf(TRAIT_TYPE.CRIMINAL).Select(x => x as Criminal).ToList();

        //    if (crimes.Count > 0) {
        //        for (int i = 0; i < crimes.Count; i++) {
        //            Criminal trait = crimes[i];
        //            switch (trait.crimeSeverity) {
        //                case CRIME_TYPE.MISDEMEANOR:
        //                    absolve += 50f;
        //                    whip += 100f;
        //                    break;
        //                case CRIME_TYPE.SERIOUS:
        //                    absolve += 5f;
        //                    whip += 20f;
        //                    kill += 50f;
        //                    exile += 50f;
        //                    break;
        //                case CRIME_TYPE.HEINOUS:
        //                    whip += 5f;
        //                    kill += 100f;
        //                    exile += 50f;
        //                    break;
        //            }
        //        }
        //    } else {
        //        kill = 100f;
        //        Debug.LogWarning(goapNode.actor.name + " is trying to judge " + targetCharacter.name + " but has no crime, and is not part of a hostile faction.");
        //    }
        //}

        ////modifiers
        //if (targetCharacter.faction == goapNode.actor.faction) {
        //    absolve *= 1.5f;
        //    whip *= 1.5f;
        //} else {
        //    FACTION_RELATIONSHIP_STATUS rel = goapNode.actor.faction.GetRelationshipWith(targetCharacter.faction).relationshipStatus;
        //    switch (rel) {
        //        case FACTION_RELATIONSHIP_STATUS.COLD_WAR:
        //            absolve *= 0.5f;
        //            whip *= 0.5f;
        //            kill *= 1.5f;
        //            exile *= 2f;
        //            break;
        //        case FACTION_RELATIONSHIP_STATUS.HOSTILE:
        //            absolve *= 0.2f;
        //            whip *= 0.5f;
        //            kill *= 2f;
        //            exile *= 1.5f;
        //            break;
        //    }
        //}

        //if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TYPE.LOVER)) {
        //    absolve *= 2f;
        //    whip *= 2f;
        //    kill *= 0.2f;
        //    exile *= 0.5f;
        //}
        //if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TYPE.RELATIVE)) {
        //    absolve *= 2f;
        //    whip *= 2f;
        //    kill *= 0.5f;
        //    exile *= 0.5f;
        //}

        //if (goapNode.actor.opinionComponent.IsFriendsWith(targetCharacter)) {
        //    absolve *= 2f;
        //    whip *= 2f;
        //    kill *= 0.5f;
        //    exile *= 0.5f;
        //} else if (goapNode.actor.opinionComponent.IsEnemiesWith(targetCharacter)) {
        //    absolve *= 0.2f;
        //    whip *= 0.5f;
        //    kill *= 2f;
        //    exile *= 1.5f;
        //}
    }
    private void TargetExecuted(ActualGoapNode goapNode) {
        (goapNode.poiTarget as Character).Death("executed", deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
    }
    private void TargetReleased(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        //**Effect 2**: If target is from a different faction or unaligned, target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        //if (goapNode.poiTarget.factionOwner == FactionManager.Instance.neutralFaction || goapNode.poiTarget.factionOwner != goapNode.actor.faction) {
        //    ForceTargetReturnHome(goapNode);
        //}
        goapNode.poiTarget.traitContainer.RemoveAllTraitsByName(goapNode.poiTarget, "Criminal");
        if (!targetCharacter.isAtHomeRegion) {
            ForceTargetReturnHome(goapNode);
        }
        ////**Effect 3**: If target is from the same faction, remove any Criminal type trait from him.
        //else {
        //    goapNode.poiTarget.traitContainer.RemoveAllTraitsByType(goapNode.poiTarget, TRAIT_TYPE.CRIMINAL);
        //}
        //**Effect 1**: Remove target's Restrained trait
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
    }
    public void TargetExiled(ActualGoapNode goapNode) {
        //**Effect 2**: Target becomes unaligned and will have his Home Location set to a random different location
        Character target = goapNode.poiTarget as Character;
        target.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        //List<Settlement> choices = new List<Settlement>(LandmarkManager.Instance.allNonPlayerAreas.Where(x => x.owner == null)); //limited choices to only use un owned areas
        List<Region> choices = GridMap.Instance.allRegions.Where(x => !x.coreTile.isCorrupted).ToList();
        //TODO:
        // if (choices == null || choices.Count <= 0) {
        //     choices = GridMap.Instance.allRegions.Where(x => x != PlayerManager.Instance.player.playerSettlement.region).ToList();
        // }
        choices.Remove(target.homeRegion);
        //TODO:
        // Region newHome = choices[Random.Range(0, choices.Count)];
        // target.MigrateHomeTo(newHome);

        //**Effect 3**: Target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        ForceTargetReturnHome(goapNode);

        //**Effect 4**: Remove any Criminal type trait from him.
        goapNode.poiTarget.traitContainer.RemoveAllTraitsByName(goapNode.poiTarget, "Criminal");

        //**Effect 1**: Remove target's Restrained trait
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
    }
    public void TargetWhip(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Injured");
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Lethargic");
        //**Effect 4**: Remove any Criminal type trait from him.
        goapNode.poiTarget.traitContainer.RemoveAllTraitsByName(goapNode.poiTarget, "Criminal");
        //**Effect 1**: Remove target's Restrained trait
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
    }
    #endregion

    private void CreateJudgeLog(ActualGoapNode goapNode, string logKey) {
        Log log = new Log(GameManager.Instance.Today(), "GoapAction", goapName, logKey, goapNode);
        log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.OverrideDescriptionLog(log);
    }

    private void ForceTargetReturnHome(ActualGoapNode goapNode) {
        Character target = goapNode.poiTarget as Character;
        target.AdjustIgnoreHostilities(1); //target should ignore hostilities or be ignored by other hostiles, until it returns home.
        target.AddOnLeaveAreaAction(() => target.AdjustIgnoreHostilities(-1));
        //target.AddOnLeaveAreaAction(() => target.ClearAllAwarenessOfType(POINT_OF_INTEREST_TYPE.ITEM, POINT_OF_INTEREST_TYPE.TILE_OBJECT));

        // CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.RETURN_HOME, CHARACTER_STATE.MOVE_OUT, target);
        // target.jobQueue.AddJobInQueue(job);
        target.PlanIdleReturnHome();
    }

    //#region Intel Reactions
    //private List<string> EndState1Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo);

    //    //Recipient and Actor are the same
    //    if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I've done!"
    //        reactions.Add(string.Format("I know what I've done!", actor.name));
    //        //-**Recipient Effect**:  no effect
    //    }
    //    //Recipient considers Target a personal Enemy:
    //    else if (recipient.relationshipContainer.HasRelationshipWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Target Name] deserves that!"
    //        reactions.Add(string.Format("{0} deserves that!", target.name));
    //        //-**Recipient Effect * *: no effect
    //    }
    //    //Recipient considers Actor a personal Enemy:
    //     else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Actor Name] is truly ruthless."
    //        reactions.Add(string.Format("{0} is truly ruthless.", actor.name));
    //        //-**Recipient Effect * *: no effect
    //    }
    //    //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
    //    else if (recipient.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.AFFAIR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE).Contains(poiTargetAlterEgo)) {
    //        //- **Recipient Response Text**: "I cannot forgive [Actor Name] for executing [Target Name]!"
    //        reactions.Add(string.Format("I cannot forgive {0} for executing {1}!", actor.name, target.name));
    //        //-**Recipient Effect * *:  Recipient will consider Actor an Enemy
    //        if (!recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            RelationshipManager.Instance.CreateNewOneWayRelationship(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
    //        }
    //    }
    //    //Recipient and Target have no relationship but are from the same faction:
    //    else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
    //        //- **Recipient Response Text**: "That is sad but I trust that it was a just judgment."
    //        reactions.Add("That is sad but I trust that it was a just judgment.");
    //        //-**Recipient Effect * *: no effect
    //    }
    //    return reactions;
    //}
    //private List<string> EndState2Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo);

    //    //Recipient and Actor are the same
    //    if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I've done!"
    //        reactions.Add(string.Format("I know what I've done!", actor.name));
    //        //-**Recipient Effect**:  no effect
    //    }
    //    //Recipient and Target are the same
    //    else if (recipient == target) {
    //        //- **Recipient Response Text**: "I am relieved that I was released."
    //        reactions.Add("I am relieved that I was released.");
    //        //-**Recipient Effect**:  no effect
    //    }
    //    //Recipient considers Target a personal Enemy:
    //    else if (recipient.relationshipContainer.HasRelationshipWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Target Name] shouldn't have been let go so easily!"
    //        reactions.Add(string.Format("{0} shouldn't have been let go so easily!", target.name));
    //        //- **Recipient Effect**: If they don't have any relationship yet, Recipient will consider Actor an Enemy
    //        if (!recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo)) {
    //            RelationshipManager.Instance.CreateNewOneWayRelationship(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
    //        }
    //    }
    //    //Recipient considers Actor a personal Enemy:
    //     else if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Actor Name] is simply naive."
    //        reactions.Add(string.Format("{0} is simply naive.", actor.name));
    //        //-**Recipient Effect * *: no effect
    //    }
    //    //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
    //    else if (recipient.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.AFFAIR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE).Contains(target.currentAlterEgo)) {
    //        //- **Recipient Response Text**: "I am grateful that [Actor Name] released [Target Name] unharmed."
    //        reactions.Add(string.Format("I am grateful that {0} released {1} unharmed.", actor.name, target.name));
    //        //- **Recipient Effect**:  If they don't have any relationship yet, Recipient will consider Actor a Friend
    //        if (!recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo)) {
    //            RelationshipManager.Instance.CreateNewOneWayRelationship(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
    //        }
    //    }
    //    //Recipient and Target have no relationship but are from the same faction:
    //    else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
    //        //- **Recipient Response Text**: "I trust that it was a just judgment."
    //        reactions.Add("I trust that it was a just judgment.");
    //        //-**Recipient Effect * *: no effect
    //    }
    //    return reactions;
    //}
    //private List<string> EndState3Reactions(Character recipient, Intel intel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo);

    //    //Recipient and Actor are the same
    //    if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I've done!"
    //        reactions.Add(string.Format("I know what I've done!", actor.name));
    //        //-**Recipient Effect**:  no effect
    //    }
    //    //Recipient and Target are the same
    //    else if (recipient == target) {
    //        //- **Recipient Response Text**: "I am sad that I was exiled but at least I am still alive."
    //        reactions.Add("I am sad that I was exiled but at least I am still alive.");
    //        //-**Recipient Effect**:  no effect
    //    }
    //    //Recipient considers Target a personal Enemy:
    //    else if (recipient.relationshipContainer.HasRelationshipWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Target Name] shouldn't have been let go so easily!"
    //        reactions.Add(string.Format("{0} shouldn't have been let go so easily!", target.name));
    //        //- **Recipient Effect**: no effect
    //    }
    //    //Recipient considers Actor a personal Enemy:
    //     else if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //        //- **Recipient Response Text**: "[Actor Name] is simply naive."
    //        reactions.Add(string.Format("{0} is irrational.", actor.name));
    //        //-**Recipient Effect * *: no effect
    //    }
    //    //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
    //    else if (recipient.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.AFFAIR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE).Contains(poiTargetAlterEgo)) {
    //        //- **Recipient Response Text**: "I am grateful that [Actor Name] exiled [Target Name] unharmed."
    //        reactions.Add(string.Format("I am grateful that {0} exiled {1} unharmed.", actor.name, target.name));
    //        //- **Recipient Effect**:  no effect
    //    }
    //    //Recipient and Target have no relationship but are from the same faction:
    //    else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
    //        //- **Recipient Response Text**: "I trust that it was a just judgment."
    //        reactions.Add("I trust that it was a just judgment.");
    //        //-**Recipient Effect * *: no effect
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class JudgeCharacterData : GoapActionData {
    public JudgeCharacterData() : base(INTERACTION_TYPE.JUDGE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }
}