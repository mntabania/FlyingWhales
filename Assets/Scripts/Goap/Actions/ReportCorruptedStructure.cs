using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Inner_Maps;
using Logs;
using UtilityScripts;

public class ReportCorruptedStructure : GoapAction {

    public ReportCorruptedStructure() : base(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Player};
    }

    #region Override
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Report Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        LocationStructure whereToReport = node.otherData[1].obj as LocationStructure;
        return whereToReport;
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        LocationStructure whereToReport = goapNode.otherData[1].obj as LocationStructure;
        if(whereToReport != null) {
            return whereToReport.GetRandomPassableTile();
        }
        return null;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return null;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        LocationStructure structureToReport = node.otherData[0].obj as LocationStructure;
        log.AddToFillers(structureToReport, structureToReport.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_2);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.homeSettlement != null;
        }
        return false;
    }
#endregion

#region State Effects
    //public void PreReportSuccess(ActualGoapNode goapNode) {
    //    object[] otherData = goapNode.otherData;
    //    LocationStructure structureToReport = otherData[0] as LocationStructure;
    //    goapNode.descriptionLog.AddToFillers(structureToReport, structureToReport.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_2);
    //}
    public void AfterReportSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        LocationStructure structureToReport = otherData[0].obj as LocationStructure;
        if (!InnerMapManager.Instance.HasWorldKnownDemonicStructure(structureToReport)) {
            InnerMapManager.Instance.AddWorldKnownDemonicStructure(structureToReport);
            // UIManager.Instance.ShowYesNoConfirmation("Demonic Structure Reported",
            //     $"Your demonic structure {structureToReport.name} has been reported by {goapNode.actor.name}! They can now attack this structure!", 
            //     onClickNoAction: goapNode.actor.CenterOnCharacter, yesBtnText: "OK", noBtnText: $"Jump to {goapNode.actor.name}", 
            //     showCover:true, pauseAndResume: true);
            // PlayerUI.Instance.ShowGeneralConfirmation("Demonic Structure Reported", "Your demonic structure " + structureToReport.name + " has been reported! They can now attack this structure!");
        }
        PlayerManager.Instance.player.AddCharacterThatHasReported(goapNode.actor);
        PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(20); //15
        PlayerManager.Instance.player.retaliationComponent.ReportDemonicStructureRetaliation(goapNode.actor);

        //Remove counter attack temporarily, since we now have retaliation
        //TriggerCounterattack(goapNode.actor, structureToReport);
    }
#endregion
    
     private void TriggerCounterattack(Character character, LocationStructure targetDemonicStructure) {
#if DEBUG_LOG
        string debugLog = GameManager.Instance.TodayLogString() + "Counterattack!";
#endif

        //LocationStructure targetDemonicStructure = InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure() ? 
        //    CollectionUtilities.GetRandomElement(InnerMapManager.Instance.worldKnownDemonicStructures): 
        //    PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
        if (targetDemonicStructure == null) {
            //it is assumed that this only happens if the player casts a spell that is seen by another character,
            //but results in the destruction of the portal
            return;
        }
#if DEBUG_LOG
        debugLog += "\n-TARGET: " + targetDemonicStructure.name;
#endif

        if(character.faction != null && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Counterattack, targetDemonicStructure)) {
            character.faction.partyQuestBoard.CreateCounterattackPartyQuest(character, character.homeSettlement, targetDemonicStructure);
        }

        //if (character.faction != null && character.faction.isMajorNonPlayer) {
        //    debugLog += "\n-CHOSEN FACTION: " + character.faction.name;
        //    character.faction.factionJobTriggerComponent.TriggerCounterattackPartyJob(targetDemonicStructure);
        //} else {
        //    Faction chosenFaction = FactionManager.Instance.GetRandomMajorNonPlayerFaction();
        //    if(chosenFaction != null) {
        //        debugLog += "\n-CHOSEN FACTION: " + chosenFaction.name;
        //        chosenFaction.factionJobTriggerComponent.TriggerCounterattackPartyJob(targetDemonicStructure);
        //    } else {
        //        Debug.LogError("No faction for counterattack!");
        //    }    
        //}


#if DEBUG_LOG
        Debug.Log(debugLog);
#endif
        //List<Character> characters = new List<Character>();
        //int count = 0;
        //for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
        //    Character character = CharacterManager.Instance.allCharacters[i];
        //    if (character.limiterComponent.canPerform && character.canMove && character.limiterComponent.canWitness && character.faction.isMajorNonPlayerFriendlyNeutral
        //        && (character.race == RACE.HUMANS || character.race == RACE.ELVES) 
        //        && !character.combatComponent.isInCombat
        //        && !(character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
        //        && character.traitContainer.HasTrait("Combatant")
        //        && character.isAlliedWithPlayer == false) {
        //        count++;
        //        debugLog += "\n-RETALIATOR: " + character.name;
        //        characters.Add(character);
        //        //character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
        //        if (count >= 5) {
        //            break;
        //        }
        //    }
        //}
        //if (characters.Count < 3) {
        //    //Create Angels
        //    CharacterManager.Instance.SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
        //    //NPCSettlement spawnSettlement = LandmarkManager.Instance.GetRandomVillageSettlement();
        //    //region = spawnSettlement.region;
        //    Region region = targetDemonicStructure.location;
        //    HexTile spawnHex = targetDemonicStructure.location.GetRandomUncorruptedPlainHex();
        //    //if (spawnSettlement != null) {
        //    //    spawnHex = spawnSettlement.GetRandomHexTile();
        //    //} else {
        //    //    spawnHex = targetDemonicStructure.location.GetRandomPlainHex();
        //    //}
        //    characters.Clear();
        //    int angelCount = UnityEngine.Random.Range(3, 6);
        //    for (int i = 0; i < angelCount; i++) {
        //        SUMMON_TYPE angelType = SUMMON_TYPE.Warrior_Angel;
        //        if(UnityEngine.Random.Range(0, 2) == 0) { angelType = SUMMON_TYPE.Magical_Angel; }
        //        LocationGridTile spawnTile = spawnHex.GetRandomTile();
        //        Summon angel = CharacterManager.Instance.CreateNewSummon(angelType, FactionManager.Instance.vagrantFaction, homeRegion: region);
        //        CharacterManager.Instance.PlaceSummon(angel, spawnTile);
        //        angel.behaviourComponent.SetIsAttackingDemonicStructure(true, CharacterManager.Instance.currentDemonicStructureTargetOfAngels);
        //        characters.Add(angel);
        //    }
        //    attackingCharacters = characters;
        //    Messenger.Broadcast(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, characters);
        //} else {
        //    for (int i = 0; i < characters.Count; i++) {
        //        characters[i].behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
        //    }
        //    attackingCharacters = characters;
        //    Messenger.Broadcast(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, characters, targetDemonicStructure as DemonicStructure);    
        //}

        //Debug.Log(debugLog);
    }
}