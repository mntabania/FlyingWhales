﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Interrupts;

public class CrimeManager : MonoBehaviour {
    public static CrimeManager Instance;

	// Use this for initialization
	void Awake () {
        Instance = this;
	}

    #region Character
    public void MakeCharacterACriminal(Character character, IPointOfInterest target, CRIME_SEVERITY crimeType,
        ICrimeable committedCrime) {
        if (character.traitContainer.HasTrait("Criminal")) {
            //Criminal criminalTrait = character.traitContainer.GetNormalTrait<Criminal>("Criminal");
            //criminalTrait.SetCrime(crimeType, committedCrime, target);
        } else {
            Criminal criminalTrait = new Criminal();
            character.traitContainer.AddTrait(character, criminalTrait);
            criminalTrait.SetCrime(crimeType, committedCrime, target);

            Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "become_criminal");
            addLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            addLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(criminalTrait.crimeData.strCrimeType), LOG_IDENTIFIER.STRING_1);
            addLog.AddToFillers(null, criminalTrait.crimeData.strCrimeType, LOG_IDENTIFIER.STRING_2);
            addLog.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(character, addLog);
        }

    }
    public CRIME_SEVERITY GetCrimeTypeConsideringAction(ActualGoapNode consideredAction) {
        Character actor = consideredAction.actor;
        IPointOfInterest target = consideredAction.poiTarget;
        INTERACTION_TYPE actionType = consideredAction.action.goapType;
        if (actionType == INTERACTION_TYPE.MAKE_LOVE) {
            if (target is Character) {
                Character targetCharacter = target as Character;
                int loverID = actor.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER);
                if (loverID != -1 && loverID != targetCharacter.id) {
                    return CRIME_SEVERITY.INFRACTION;
                }
            }
        } else if (consideredAction.associatedJobType == JOB_TYPE.DESTROY) {
            if (consideredAction.poiTarget is TileObject tileObject) {
                if (tileObject.characterOwner != null && 
                    !tileObject.IsOwnedBy(consideredAction.actor)) {
                    //only consider destroy job as infraction if target object is owned by someone else
                    return CRIME_SEVERITY.INFRACTION;    
                } else {
                    return CRIME_SEVERITY.NONE;
                }
            }
            return CRIME_SEVERITY.INFRACTION;
        } else if (consideredAction.associatedJobType == JOB_TYPE.SPREAD_RUMOR) {
            return CRIME_SEVERITY.INFRACTION;
        } else if (actionType == INTERACTION_TYPE.STEAL
            || actionType == INTERACTION_TYPE.POISON) {
            return CRIME_SEVERITY.MISDEMEANOR;
        } else if (actionType == INTERACTION_TYPE.PICK_UP) {
            if(consideredAction.poiTarget is TileObject targetTileObject) {
                if(targetTileObject.characterOwner != null && !targetTileObject.IsOwnedBy(consideredAction.actor)) {
                    return CRIME_SEVERITY.MISDEMEANOR;
                }
            }
        } else if (actionType == INTERACTION_TYPE.KNOCKOUT_CHARACTER
            || actionType == INTERACTION_TYPE.ASSAULT) {
            if(consideredAction.associatedJobType != JOB_TYPE.APPREHEND) {
                if (target is Character targetCharacter && targetCharacter.isNormalCharacter) {
                    if (!actor.IsHostileWith(targetCharacter)) {
                        return CRIME_SEVERITY.MISDEMEANOR;
                    }
                } else if (target is TileObject targetTileObject && !targetTileObject.IsOwnedBy(actor)) {
                    //added checking for gridTileLocation because targetTileObject could've been destroyed already. 
                    LocationStructure structureLocation = targetTileObject.gridTileLocation != null ? targetTileObject.structureLocation : targetTileObject.previousTile.structure;
                    if(structureLocation != null) {
                        if (structureLocation.settlementLocation != null
                        && (structureLocation.settlementLocation.locationType == LOCATION_TYPE.SETTLEMENT)) {
                            return CRIME_SEVERITY.MISDEMEANOR;
                        }
                    }
                }
            }
        } else if ((actionType == INTERACTION_TYPE.STRANGLE && actor != target)
         || actionType == INTERACTION_TYPE.RITUAL_KILLING || actionType == INTERACTION_TYPE.MURDER) {
            return CRIME_SEVERITY.SERIOUS;
        } else if (actionType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM
            || actionType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM
            || actionType == INTERACTION_TYPE.DRINK_BLOOD) {
            return CRIME_SEVERITY.HEINOUS;
        }
        return CRIME_SEVERITY.NONE;
    }
    public CRIME_SEVERITY GetCrimeTypeConsideringInterrupt(Character considerer, Character actor, Interrupt interrupt) {
        if (interrupt.type == INTERRUPT.Transform_To_Wolf
            || interrupt.type == INTERRUPT.Revert_To_Normal) {
            return CRIME_SEVERITY.HEINOUS;
        }
        return CRIME_SEVERITY.NONE;
    }
    #endregion

    #region Processes
    //public void CommitCrime(ActualGoapNode committedCrime, GoapPlanJob crimeJob, CRIME_TYPE crimeType) {
    //    committedCrime.SetAsCrime(crimeType);
    //    Messenger.Broadcast(Signals.ON_COMMIT_CRIME, committedCrime, crimeJob);
    //}
    public void ReactToCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType, CRIME_SEVERITY crimeType) {
        switch (crimeType) {
            case CRIME_SEVERITY.INFRACTION:
                ReactToInfraction(reactor, crimeCommitter, committedCrime, crimeJobType);
                break;
            case CRIME_SEVERITY.MISDEMEANOR:
                ReactToMisdemeanor(reactor, crimeCommitter, committedCrime, crimeJobType);
                break;
            case CRIME_SEVERITY.SERIOUS:
                ReactToSeriousCrime(reactor, crimeCommitter, committedCrime, crimeJobType);
                break;
            case CRIME_SEVERITY.HEINOUS:
                ReactToHeinousCrime(reactor, crimeCommitter, committedCrime, crimeJobType);
                break;
        }
    }
    public void ReactToCrime(Character reactor, Character actor, Interrupt interrupt, CRIME_SEVERITY crimeType) {
        switch (crimeType) {
            //case CRIME_TYPE.INFRACTION:
            //    ReactToInfraction(reactor, committedCrime, crimeJobType);
            //    break;
            //case CRIME_TYPE.MISDEMEANOR:
            //    ReactToMisdemeanor(reactor, committedCrime, crimeJobType);
            //    break;
            //case CRIME_TYPE.SERIOUS:
            //    ReactToSeriousCrime(reactor, committedCrime, crimeJobType);
            //    break;
            case CRIME_SEVERITY.HEINOUS:
                ReactToHeinousCrime(reactor, actor, interrupt);
                break;
        }
    }
    private void ReactToInfraction(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
        string lastStrawReason = string.Empty;
        if(committedCrime.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
            lastStrawReason = "is unfaithful";
        } else if (crimeJobType == JOB_TYPE.DESTROY) {
            lastStrawReason = "has destructive behaviour";
        } else if (crimeJobType == JOB_TYPE.SPREAD_RUMOR) {
            lastStrawReason = "is a rumormonger";
        }
        reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Infraction", -5, lastStrawReason);
    }
    private void ReactToMisdemeanor(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
        string lastStrawReason = string.Empty;
        if (committedCrime.action.goapType == INTERACTION_TYPE.STEAL) {
            lastStrawReason = "stole something";
        } else if (committedCrime.action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER || committedCrime.action.goapType == INTERACTION_TYPE.ASSAULT) {
            lastStrawReason = "attacked someone";
        } else if (committedCrime.action.goapType == INTERACTION_TYPE.POISON) {
            lastStrawReason = "attacked someone";
        } else if (committedCrime.action.goapType == INTERACTION_TYPE.BOOBY_TRAP) {
            lastStrawReason = "got caught";
        }
        reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Misdemeanor", -10, lastStrawReason);
        MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.MISDEMEANOR, committedCrime.action);
    }
    private void ReactToSeriousCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
        string lastStrawReason = string.Empty;
        if (committedCrime.action.goapType == INTERACTION_TYPE.STRANGLE) {
            lastStrawReason = "murdered someone";
        } else if (committedCrime.action.goapType == INTERACTION_TYPE.RITUAL_KILLING) {
            lastStrawReason = "is a Psychopath killer";
        }
        reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Serious Crime", -20);
        MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.SERIOUS, committedCrime.action);
    }
    private void ReactToHeinousCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
        string lastStrawReason = string.Empty;
        if (committedCrime.action.goapType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM || committedCrime.action.goapType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM) {
            lastStrawReason = "is a werewolf";
        } else if (committedCrime.action.goapType == INTERACTION_TYPE.DRINK_BLOOD) {
            lastStrawReason = "is a vampire";
        }
        reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Heinous Crime", -40);
        MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.HEINOUS, committedCrime.action);
    }
    private void ReactToHeinousCrime(Character reactor, Character actor, Interrupt interrupt) {
        string lastStrawReason = string.Empty;
        if (interrupt.type == INTERRUPT.Transform_To_Wolf || interrupt.type == INTERRUPT.Revert_To_Normal) {
            lastStrawReason = "is a werewolf";
        }
        reactor.relationshipContainer.AdjustOpinion(reactor, actor, "Heinous Crime", -40);
        MakeCharacterACriminal(actor, null, CRIME_SEVERITY.HEINOUS, interrupt);
    }
    #endregion
}

public class CrimeData {
    public CRIME_SEVERITY crimeType { get; }
    public CRIME_STATUS crimeStatus { get; private set; }
    public ICrimeable crime { get; }
    public string strCrimeType { get; }

    public Character criminal { get; }
    public IPointOfInterest target { get; }
    public Faction targetFaction { get; }
    public Character judge { get; private set; }
    public List<Character> witnesses { get; }

    public CrimeData(CRIME_SEVERITY crimeType, ICrimeable crime, Character criminal, IPointOfInterest target) {
        this.crimeType = crimeType;
        this.crime = crime;
        this.criminal = criminal;
        this.target = target;
        strCrimeType = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(this.crimeType.ToString());
        if(crimeType == CRIME_SEVERITY.SERIOUS || crimeType == CRIME_SEVERITY.HEINOUS) {
            strCrimeType += " Crime";
        }
        witnesses = new List<Character>();
        SetCrimeStatus(CRIME_STATUS.Unpunished);
    }

    #region General
    public void SetCrimeStatus(CRIME_STATUS status) {
        if(crimeStatus != status) {
            crimeStatus = status;
            if(crimeStatus == CRIME_STATUS.Unpunished || crimeStatus == CRIME_STATUS.Imprisoned) {
                criminal.SetHaUnresolvedCrime(true);
            } else {
                criminal.SetHaUnresolvedCrime(false);
            }
            //if(crimeStatus == CRIME_STATUS.Imprisoned) {
            //    CreateJudgementJob();
            //}
        }
    }
    public void SetJudge(Character character) {
        judge = character;
    }
    #endregion

    #region Witnesses
    public void AddWitness(Character character) {
        if (!witnesses.Contains(character)) {
            witnesses.Add(character);
        }
    }
    #endregion

    //#region Prisoner
    //private void CreateJudgementJob() {
    //    if (!criminal.HasJobTargetingThis(JOB_TYPE.JUDGEMENT)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGEMENT, INTERACTION_TYPE.JUDGE_CHARACTER, criminal, criminal.currentNpcSettlement);
    //        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
    //        criminal.currentNpcSettlement.AddToAvailableJobs(job);
    //    }
    //}
    //#endregion
}