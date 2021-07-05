using System.Collections;
using System.Collections.Generic;
using System;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Interrupts;
using Crime_System;
using UtilityScripts;
using Locations.Settlements;
using Object_Pools;

public class CrimeManager : BaseMonoBehaviour {
    public static CrimeManager Instance;

    private Dictionary<CRIME_SEVERITY, CrimeSeverity> _crimeSeverities;
    private Dictionary<CRIME_TYPE, CrimeType> _crimeTypes;

    // Use this for initialization
    void Awake () {
        Instance = this;
	}
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }

    #region General
    public void Initialize() {
        ConstructCrimeSeverities();
        ConstructCrimeTypes();
    }
    private void ConstructCrimeSeverities() {
        _crimeSeverities = new Dictionary<CRIME_SEVERITY, CrimeSeverity>();
        CRIME_SEVERITY[] enumValues = CollectionUtilities.GetEnumValues<CRIME_SEVERITY>();
        for (int i = 0; i < enumValues.Length; i++) {
            CRIME_SEVERITY severity = enumValues[i];
            var typeName = $"Crime_System.{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(severity.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                CrimeSeverity data = Activator.CreateInstance(type) as CrimeSeverity;
                _crimeSeverities.Add(severity, data);
            } else {
                Debug.LogWarning($"Crime Severity {typeName} has no data!");
            }
        }
    }
    private void ConstructCrimeTypes() {
        _crimeTypes = new Dictionary<CRIME_TYPE, CrimeType>();
        CRIME_TYPE[] enumValues = CollectionUtilities.GetEnumValues<CRIME_TYPE>();
        for (int i = 0; i < enumValues.Length; i++) {
            CRIME_TYPE crimeType = enumValues[i];
            var typeName = $"Crime_System.{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(crimeType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                CrimeType data = Activator.CreateInstance(type) as CrimeType;
                _crimeTypes.Add(crimeType, data);
            } else {
                Debug.LogWarning($"Crime Type {typeName} has no data!");
            }
        }
    }
    private bool ShouldCreateCrimeStatus(CRIME_SEVERITY severity) {
        if(severity == CRIME_SEVERITY.Infraction || severity == CRIME_SEVERITY.None || severity == CRIME_SEVERITY.Unapplicable) {
            return false;
        }
        return true;
    }
    #endregion

    #region Character
    public void MakeCharacterACriminal(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character witness, Character criminal, IPointOfInterest target, Faction targetFaction, REACTION_STATUS reactionStatus, Criminal criminalTrait) {
        if(criminalTrait == null) {
            criminal.traitContainer.AddTrait(criminal, "Criminal");
        }
        CrimeData existingCrimeData = criminal.crimeComponent.GetCrimeDataOf(crime);
        if (existingCrimeData == null) {
            //If crime is added, and crime is interrupt, do not object pool the interrupt because the data will be cleared out and that will cause problems in the crime data
            if(crime is InterruptHolder interrupt) {
                interrupt.SetShouldNotBeObjectPooled(true);
            }
            existingCrimeData = criminal.crimeComponent.AddCrime(crimeType, crimeSeverity, crime, criminal, criminalTrait, target, targetFaction, reactionStatus);
            CrimeType crimeTypeObj = existingCrimeData.crimeTypeObj;

            if(crime is ActualGoapNode action && action.isAssumption) {
                //Do not log accuse text
            } else {
                Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", "become_criminal", null, LogUtilities.Criminal_Tags);
                addLog.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddToFillers(null, crimeTypeObj.accuseText, LOG_IDENTIFIER.STRING_1);
                addLog.AddToFillers(witness, witness.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                addLog.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(criminal, addLog);
                LogPool.Release(addLog);
            }
            Messenger.Broadcast(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, criminal, crimeType, witness);
        }

        ProcessWitnessCrime(witness, existingCrimeData);
    }
    private void ProcessWitnessCrime(Character p_witness, CrimeData p_crimeData) {
        if (!p_crimeData.IsWitness(p_witness)) {
            p_crimeData.AddWitness(p_witness);

            bool willDecideWantedOrNot = false;
            if (p_witness.isNormalCharacter && p_witness.faction != null && p_witness.faction.isMajorNonPlayer) {
                if (p_witness.isFactionLeader || p_witness.isSettlementRuler) {
                    if (!p_crimeData.IsWantedBy(p_witness.faction)) {
                        //Decide whether to switch to wanted by the faction or not
                        willDecideWantedOrNot = true;
                        WantedOrNotDecisionMaking(p_witness, p_crimeData.criminal, p_witness.faction, p_crimeData, p_crimeData.crimeSeverity);
                    }
                }
            }
            if (!willDecideWantedOrNot) {
                if (!p_crimeData.isRemoved) {
                    if (!p_witness.crimeComponent.IsReported(p_crimeData)) {
                        p_witness.jobComponent.TryCreateReportCrimeJob(p_crimeData.criminal, p_crimeData.target, p_crimeData, p_crimeData.crime);
                    }
                }
            }
        }
    }

    //Returns true if wanted, false if not
    public bool WantedOrNotDecisionMaking(Character authority, Character criminal, Faction authorityFaction, CrimeData crimeData, CRIME_SEVERITY crimeSeverity) {
        string opinionLabel = authority.relationshipContainer.GetOpinionLabel(criminal);
        string key = string.Empty;
        if (crimeSeverity == CRIME_SEVERITY.Heinous) {
            key = "wanted";
        } else if (opinionLabel == RelationshipManager.Close_Friend) {
            if (crimeSeverity == CRIME_SEVERITY.Serious) {
                key = GameUtilities.RollChance(75) ? "wanted" : "not_wanted";
            } else {
                key = "not_wanted";    
            }
        } else if ((authority.relationshipContainer.IsFamilyMember(criminal) || authority.relationshipContainer.HasRelationshipWith(criminal, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                   && !authority.relationshipContainer.IsEnemiesWith(criminal)) {
            if (crimeSeverity == CRIME_SEVERITY.Serious) {
                key = GameUtilities.RollChance(75) ? "wanted" : "not_wanted";
            } else {
                key = "not_wanted";    
            }
        } else if (opinionLabel == RelationshipManager.Friend) {
            if(UnityEngine.Random.Range(0, 100) < authority.relationshipContainer.GetTotalOpinion(criminal)) {
                key = "not_wanted";
            } else {
                key = "wanted";
            }
        } else {
            key = "wanted";
        }

        if(key == "wanted") {
            crimeData.AddFactionThatConsidersWanted(authorityFaction);
        }

        if (key != string.Empty) {
            Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", key, null, LogUtilities.Declare_Wanted_Tags);
            addLog.AddToFillers(authority, authority.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            addLog.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            addLog.AddToFillers(null, crimeData.crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
            addLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFrom(criminal, addLog);
            LogPool.Release(addLog);
        }
        return key == "wanted";
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
                    return CRIME_SEVERITY.Infraction;
                }
            }
        } else if (consideredAction.associatedJobType == JOB_TYPE.DESTROY) {
            if (consideredAction.poiTarget is TileObject tileObject) {
                if (tileObject.characterOwner != null && 
                    !tileObject.IsOwnedBy(consideredAction.actor)) {
                    //only consider destroy job as infraction if target object is owned by someone else
                    return CRIME_SEVERITY.Infraction;    
                } else {
                    return CRIME_SEVERITY.None;
                }
            }
            return CRIME_SEVERITY.Infraction;
        } else if (consideredAction.associatedJobType == JOB_TYPE.SPREAD_RUMOR) {
            return CRIME_SEVERITY.Infraction;
        } else if (actionType == INTERACTION_TYPE.STEAL
            || actionType == INTERACTION_TYPE.STEAL_ANYTHING
            || actionType == INTERACTION_TYPE.POISON
            || actionType == INTERACTION_TYPE.PICKPOCKET
            || actionType == INTERACTION_TYPE.STEAL_COINS) {
            return CRIME_SEVERITY.Misdemeanor;
        } else if (actionType == INTERACTION_TYPE.PICK_UP) {
            if(consideredAction.poiTarget is TileObject targetTileObject) {
                if(targetTileObject.characterOwner != null && !targetTileObject.IsOwnedBy(consideredAction.actor)) {
                    return CRIME_SEVERITY.Misdemeanor;
                }
            }
        } else if (actionType == INTERACTION_TYPE.KNOCKOUT_CHARACTER
            || actionType == INTERACTION_TYPE.ASSAULT) {
            if(consideredAction.associatedJobType != JOB_TYPE.APPREHEND) {
                if (target is Character targetCharacter && targetCharacter.isNormalCharacter) {
                    if (!actor.IsHostileWith(targetCharacter)) {
                        return CRIME_SEVERITY.Misdemeanor;
                    }
                } else if (target is TileObject targetTileObject && !targetTileObject.IsOwnedBy(actor)) {
                    //added checking for gridTileLocation because targetTileObject could've been destroyed already. 
                    if(targetTileObject.gridTileLocation != null) {
                        BaseSettlement settlement = null;
                        if (targetTileObject.gridTileLocation.IsPartOfSettlement(out settlement)) {
                            if(settlement.locationType == LOCATION_TYPE.VILLAGE) {
                                return CRIME_SEVERITY.Misdemeanor;
                            }
                        }
                    }
                }
            }
        } else if ((actionType == INTERACTION_TYPE.STRANGLE && actor != target)
         || actionType == INTERACTION_TYPE.RITUAL_KILLING || actionType == INTERACTION_TYPE.MURDER) {
            return CRIME_SEVERITY.Serious;
        } else if (actionType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM
            || actionType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM
            || actionType == INTERACTION_TYPE.DRINK_BLOOD) {
            return CRIME_SEVERITY.Heinous;
        }
        return CRIME_SEVERITY.None;
    }
    public CRIME_SEVERITY GetCrimeTypeConsideringInterrupt(Character considerer, Character actor, Interrupt interrupt) {
        if (interrupt.type == INTERRUPT.Transform_To_Wolf
            || interrupt.type == INTERRUPT.Revert_To_Normal) {
            return CRIME_SEVERITY.Heinous;
        }
        return CRIME_SEVERITY.None;
    }
    #endregion

    #region Processes
    //public void CommitCrime(ActualGoapNode committedCrime, GoapPlanJob crimeJob, CRIME_TYPE crimeType) {
    //    committedCrime.SetAsCrime(crimeType);
    //    Messenger.Broadcast(Signals.ON_COMMIT_CRIME, committedCrime, crimeJob);
    //}
    public void ReactToCrime(Character witness, Character actor, IPointOfInterest target, Faction targetFaction, CRIME_TYPE crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) {
        if(witness == actor) {
            //Will not react if witness is the actor
            return;
        }
        if (crimeType != CRIME_TYPE.Unset) {
            if(crimeType != CRIME_TYPE.None) {
                bool hasAlreadyReacted = actor.crimeComponent.IsCrimeAlreadyWitnessedBy(witness, crime);
                if (hasAlreadyReacted) {
                    //Will only react to crime once
                    return;
                }
                Criminal existingCriminalTrait = null;
                if (actor.traitContainer.HasTrait("Criminal")) {
                    existingCriminalTrait = actor.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
                }

                CrimeType crimeTypeObj = GetCrimeType(crimeType);

                //Check if personal decision on crime severity takes precendence over faction's decision
                //Default is YES
                CRIME_SEVERITY finalCrimeSeverity = GetCrimeSeverity(witness, actor, target, crimeType);

                if(finalCrimeSeverity != CRIME_SEVERITY.None && finalCrimeSeverity != CRIME_SEVERITY.Unapplicable) {
                    CrimeSeverity crimeSeverityObj = GetCrimeSeverity(finalCrimeSeverity);
                    string emotions = crimeSeverityObj.EffectAndReaction(witness, actor, target, crimeTypeObj, crime, reactionStatus);
                    if (emotions != string.Empty) {
                        if (!CharacterManager.Instance.EmotionsChecker(emotions)) {
#if DEBUG_LOG
                            string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                            error += $"\n-Witness: {witness}";
                            error += $"\n-Action: {crime.name}";
                            error += $"\n-Actor: {actor.name}";
                            error += $"\n-Target: {target.nameWithID}";
                            witness.logComponent.PrintLogErrorIfActive(error);
#endif
                        } else {
                            //add log of emotions felt
                            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", "emotions_crime_" + reactionStatus.ToString().ToLower(), null, LogUtilities.Life_Changes_Crimes_Tags);
                            if (reactionStatus == REACTION_STATUS.INFORMED) {
                                log.AddTag(LOG_TAG.Informed);
                            } else if (reactionStatus == REACTION_STATUS.WITNESSED) {
                                log.AddTag(LOG_TAG.Witnessed);
                            }
                            log.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotions, 2), LOG_IDENTIFIER.STRING_1);
                            log.AddLogToDatabase(true);
                        }
                    }

                    if (actor.isDead) {
                        //Should not process crime further if criminal is already dead
                        //This means that the witness will still emotion react to the dead criminal but will no longer add criminal trait/report crime/make the criminal wanted
                        return;
                    }
                    //Witness should check if the actor already has an active crime data against the target with same crime type
                    //If actor already has one, the witness should not create another crime data against the actor, he must only be added to the witness list
                    //Example: If actor attacked character B (Assault crime), then character A witnessed it, character A will create a crime data that "actor did an assault crime against character B"
                    //Now, if actor attacked character B again while still having the assault crime against him, and character C witnessed it, instead of creating another crime data that "actor did an assault crime against character B"
                    //Character C will just be added as a witness to the crime data that character A previously created since that crime is still active
                    //The reason for this is so that we can reduce the number of crime data created if it is just the same crime against the same person
                    //Additional note: also added case if crime is Vampire/Werewolf, crime data should return any crime of the same crime type
                    CrimeData existingActiveCrimeData = actor.crimeComponent.GetExistingActiveCrimeData(target, crimeType);
                    if(existingActiveCrimeData != null) {
                        ProcessWitnessCrime(witness, existingActiveCrimeData);
                    } else {
                        if (ShouldCreateCrimeStatus(finalCrimeSeverity)) {
                            MakeCharacterACriminal(crimeType, finalCrimeSeverity, crime, witness, actor, target, targetFaction, reactionStatus, existingCriminalTrait);
                        }
                    }
                }
            }
        }
    }
    public CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType) {
        if (!actor.isNormalCharacter) {
            //Non villagers should not be criminals that is why the severity is None
            return CRIME_SEVERITY.None;
        }
        CrimeType crimeTypeObj = GetCrimeType(crimeType);
        CRIME_SEVERITY factionCrimeSeverity = CRIME_SEVERITY.Unapplicable;
        if (witness.faction != null) {
            factionCrimeSeverity = witness.faction.GetCrimeSeverity(actor, target, crimeType);
        }
        CRIME_SEVERITY witnessCrimeSeverity = crimeTypeObj.GetCrimeSeverity(witness, actor, target);
        CRIME_SEVERITY finalCrimeSeverity = witnessCrimeSeverity;
        //Only use faction crime severity if the personal witness crime severity is Unapplicable because if it is None, it means that the witness does not consider it a crime
        if (witnessCrimeSeverity == CRIME_SEVERITY.Unapplicable) {
            finalCrimeSeverity = factionCrimeSeverity;
        }
        return finalCrimeSeverity;
    }
    public bool IsConsideredACrimeByCharacter(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType) {
        CRIME_SEVERITY crimeSeverity = GetCrimeSeverity(witness, actor, target, crimeType);
        return crimeSeverity != CRIME_SEVERITY.None && crimeSeverity != CRIME_SEVERITY.Unapplicable;
    }

    //public void ReactToCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType, CRIME_SEVERITY crimeType) {
    //    switch (crimeType) {
    //        case CRIME_SEVERITY.Infraction:
    //            ReactToInfraction(reactor, crimeCommitter, committedCrime, crimeJobType);
    //            break;
    //        case CRIME_SEVERITY.Misdemeanor:
    //            ReactToMisdemeanor(reactor, crimeCommitter, committedCrime, crimeJobType);
    //            break;
    //        case CRIME_SEVERITY.Serious:
    //            ReactToSeriousCrime(reactor, crimeCommitter, committedCrime, crimeJobType);
    //            break;
    //        case CRIME_SEVERITY.Heinous:
    //            ReactToHeinousCrime(reactor, crimeCommitter, committedCrime, crimeJobType);
    //            break;
    //    }
    //}
    //public void ReactToCrime(Character reactor, Character actor, Interrupt interrupt, CRIME_SEVERITY crimeType) {
    //    switch (crimeType) {
    //        //case CRIME_TYPE.INFRACTION:
    //        //    ReactToInfraction(reactor, committedCrime, crimeJobType);
    //        //    break;
    //        //case CRIME_TYPE.MISDEMEANOR:
    //        //    ReactToMisdemeanor(reactor, committedCrime, crimeJobType);
    //        //    break;
    //        //case CRIME_TYPE.SERIOUS:
    //        //    ReactToSeriousCrime(reactor, committedCrime, crimeJobType);
    //        //    break;
    //        case CRIME_SEVERITY.Heinous:
    //            ReactToHeinousCrime(reactor, actor, interrupt);
    //            break;
    //    }
    //}
    //private void ReactToInfraction(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
    //    string lastStrawReason = string.Empty;
    //    if(committedCrime.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
    //        lastStrawReason = "is unfaithful";
    //    } else if (crimeJobType == JOB_TYPE.DESTROY) {
    //        lastStrawReason = "has destructive behaviour";
    //    } else if (crimeJobType == JOB_TYPE.SPREAD_RUMOR) {
    //        lastStrawReason = "is a rumormonger";
    //    }
    //    reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Infraction", -5, lastStrawReason);
    //}
    //private void ReactToMisdemeanor(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
    //    string lastStrawReason = string.Empty;
    //    if (committedCrime.action.goapType == INTERACTION_TYPE.STEAL) {
    //        lastStrawReason = "stole something";
    //    } else if (committedCrime.action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER || committedCrime.action.goapType == INTERACTION_TYPE.ASSAULT) {
    //        lastStrawReason = "attacked someone";
    //    } else if (committedCrime.action.goapType == INTERACTION_TYPE.POISON) {
    //        lastStrawReason = "attacked someone";
    //    } else if (committedCrime.action.goapType == INTERACTION_TYPE.BOOBY_TRAP) {
    //        lastStrawReason = "got caught";
    //    }
    //    reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Misdemeanor", -10, lastStrawReason);
    //    MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.Misdemeanor, committedCrime);
    //}
    //private void ReactToSeriousCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
    //    string lastStrawReason = string.Empty;
    //    if (committedCrime.action.goapType == INTERACTION_TYPE.STRANGLE) {
    //        lastStrawReason = "murdered someone";
    //    } else if (committedCrime.action.goapType == INTERACTION_TYPE.RITUAL_KILLING) {
    //        lastStrawReason = "is a Psychopath killer";
    //    }
    //    reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Serious Crime", -20);
    //    MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.Serious, committedCrime);
    //}
    //private void ReactToHeinousCrime(Character reactor, Character crimeCommitter, ActualGoapNode committedCrime, JOB_TYPE crimeJobType) {
    //    string lastStrawReason = string.Empty;
    //    if (committedCrime.action.goapType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM || committedCrime.action.goapType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM) {
    //        lastStrawReason = "is a werewolf";
    //    } else if (committedCrime.action.goapType == INTERACTION_TYPE.DRINK_BLOOD) {
    //        lastStrawReason = "is a vampire";
    //    }
    //    reactor.relationshipContainer.AdjustOpinion(reactor, crimeCommitter, "Heinous Crime", -40);
    //    MakeCharacterACriminal(crimeCommitter, committedCrime.target, CRIME_SEVERITY.Heinous, committedCrime.action);
    //}
    //private void ReactToHeinousCrime(Character reactor, Character actor, Interrupt interrupt) {
    //    string lastStrawReason = string.Empty;
    //    if (interrupt.type == INTERRUPT.Transform_To_Wolf || interrupt.type == INTERRUPT.Revert_To_Normal) {
    //        lastStrawReason = "is a werewolf";
    //    }
    //    reactor.relationshipContainer.AdjustOpinion(reactor, actor, "Heinous Crime", -40);
    //    MakeCharacterACriminal(actor, null, CRIME_SEVERITY.Heinous, interrupt);
    //}
#endregion

#region Crime Severity
    public CrimeSeverity GetCrimeSeverity(CRIME_SEVERITY severityType) {
        if (_crimeSeverities.ContainsKey(severityType)) {
            return _crimeSeverities[severityType];
        }
        return null;
    }
    public CrimeType GetCrimeType(CRIME_TYPE crimeType) {
        if (_crimeTypes.ContainsKey(crimeType)) {
            return _crimeTypes[crimeType];
        }
        return null;
    }
#endregion
}

public class CrimeData : ISavable {
    public string persistentID { get; private set; }
    public CRIME_SEVERITY crimeSeverity { get; private set; }
    public CRIME_TYPE crimeType { get; private set; }
    public CRIME_STATUS crimeStatus { get; private set; }
    public ICrimeable crime { get; private set; }

    public Character criminal { get; private set; }
    public IPointOfInterest target { get; private set; }
    public Faction targetFaction { get; private set; }
    public Character judge { get; private set; }
    public List<Character> witnesses { get; private set; }
    public List<Faction> factionsThatConsidersWanted { get; private set; }
    public bool isRemoved { get; private set; }

#region getters
    public CrimeType crimeTypeObj => CrimeManager.Instance.GetCrimeType(crimeType);
    public OBJECT_TYPE objectType => OBJECT_TYPE.Crime;
    public System.Type serializedData => typeof(SaveDataCrimeData);
#endregion

    public CrimeData(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character criminal, IPointOfInterest target, Faction targetFaction) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.crimeType = crimeType;
        this.crimeSeverity = crimeSeverity;
        SetCrime(crime);
        this.criminal = criminal;
        this.target = target;
        this.targetFaction = targetFaction;
        witnesses = new List<Character>();
        factionsThatConsidersWanted = new List<Faction>();
        SetCrimeStatus(CRIME_STATUS.Unpunished);
        SubscribeToListeners();
        DatabaseManager.Instance.crimeDatabase.AddCrime(this);
    }
    public CrimeData(SaveDataCrimeData data) {
        witnesses = new List<Character>();
        factionsThatConsidersWanted = new List<Faction>();

        persistentID = data.persistentID;
        crimeSeverity = data.crimeSeverity;
        crimeType = data.crimeType;
        crimeStatus = data.crimeStatus;
        isRemoved = data.isRemoved;

        if (!isRemoved) {
            SubscribeToListeners();
        }
    }

    private void SetCrime(ICrimeable crime) {
        this.crime = crime;

        //if (crime is InterruptHolder interrupt) {
        //    //If a crime being stored is an interrupt, we need to clone it and store the clone instead of the original crime
        //    //The reason for this is that the original crime will be object pooled after being done,
        //    //So we need more something permanent
        //    InterruptHolder clonedInterrupt = ObjectPoolManager.Instance.CreateNewInterrupt();
        //    clonedInterrupt.Initialize(interrupt.interrupt, interrupt.actor, interrupt.target, interrupt.identifier, interrupt.reason);
        //    clonedInterrupt.SetEffectLog(interrupt.effectLog);
        //    this.crime = crime;
        //} else {
        //    this.crime = crime;
        //}
    }


#region Listeners
    private void SubscribeToListeners() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void UnsubscribeFromListeners() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (isRemoved) {
            return;
        }
        if (IsWitness(character)) {
            if (!HasWanted()) {
                if (AreAllWitnessesDead()) {
                    criminal.crimeComponent.RemoveCrime(this);

                    Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", "dead_witnesses", null, LogUtilities.Life_Changes_Crimes_Tags);
                    addLog.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(null, crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
                    addLog.AddLogToDatabase(true);
                }
            }
        }
    }
#endregion

#region General
    public void SetCrimeStatus(CRIME_STATUS status) {
        if(crimeStatus != status) {
            crimeStatus = status;
            if(crimeStatus == CRIME_STATUS.Unpunished) {
                criminal.SetHasUnresolvedCrime(true);
            } else {
                criminal.SetHasUnresolvedCrime(false);
            }
            //if(crimeStatus == CRIME_STATUS.Imprisoned) {
            //    CreateJudgementJob();
            //}
        }
    }
    public void SetJudge(Character character) {
        judge = character;
    }
    public string GetCrimeDataDescription() {
        string desc = crimeTypeObj.name + " - " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(crimeStatus.ToString());
        desc += "\n     Wanted at: " + GetFactionsThatConsidersWantedAsText();
        desc += "\n     Witnesses: " + GetWitnessesAsText();
        return desc;
    }
    private string GetFactionsThatConsidersWantedAsText() {
        string text = "None";
        if(factionsThatConsidersWanted.Count > 0) {
            text = string.Empty;
            for (int i = 0; i < factionsThatConsidersWanted.Count; i++) {
                if(i > 0) {
                    text += ", ";
                }
                text += factionsThatConsidersWanted[i].nameWithColor;
            }
        }
        return text;
    }
    private string GetWitnessesAsText() {
        string text = "None";
        if (witnesses.Count > 0) {
            text = string.Empty;
            for (int i = 0; i < witnesses.Count; i++) {
                if (i > 0) {
                    text += ", ";
                }
                text += witnesses[i].name;
            }
        }
        return text;
    }
    public void OnCrimeAdded() { }
    public void OnCrimeRemoved() {
        isRemoved = true;
        UnsubscribeFromListeners();
        //IMPORTANT NOTE: This has inconsistency since we are removing the crime in the witness list but the crime does not remove the witness from the list
        //This is because we still need the list of witnesses in previous crimes but the witness should not report the witnessed crime
        //for (int i = 0; i < witnesses.Count; i++) {
        //    witnesses[i].crimeComponent.RemoveWitnessedCrime(this);
        //}
    }
#endregion

#region Witnesses
    public bool IsWitness(Character character) {
        return witnesses.Contains(character);
    }
    public void AddWitness(Character character) {
        witnesses.Add(character);
        character.crimeComponent.AddWitnessedCrime(this);
    }
    private bool AreAllWitnessesDead() {
        if(witnesses.Count > 0) {
            for (int i = 0; i < witnesses.Count; i++) {
                if (!witnesses[i].isDead) {
                    return false;
                }
            }
            return true;
        }
        //If there are no witnesses do not consider this true
        return false;
    }
#endregion

#region Faction
    public void AddFactionThatConsidersWanted(Faction faction) {
        if (!factionsThatConsidersWanted.Contains(faction)) {
            factionsThatConsidersWanted.Add(faction);

            if(criminal.homeSettlement != null && criminal.partyComponent.hasParty && criminal.homeSettlement.owner == faction && !criminal.isDead) {
                criminal.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, criminal, "Wanted criminal");
            }

            if (criminal.isSettlementRuler) {
                if(criminal.homeSettlement.owner == faction) {
                    criminal.homeSettlement.SetRuler(null);
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_settlement_ruler", null, LogUtilities.Life_Changes_Crimes_Tags);
                    log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    criminal.logComponent.RegisterLog(log, true);
                }
            }

            if (faction.leader == criminal) {
                faction.SetLeader(null);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_faction_leader", null, LogUtilities.Life_Changes_Crimes_Tags);
                log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                criminal.logComponent.RegisterLog(log, true);
            }
            if (target is Character targetCharacter && crime is ActualGoapNode crimeAction) {
                CRIME_SEVERITY severityOfCrime = faction.GetCrimeSeverity(criminal, target, crimeType);
                faction.CheckForWar(criminal.faction, severityOfCrime, criminal, targetCharacter, crimeAction);    
            }
            
            if(crimeType == CRIME_TYPE.Vampire) {
                //when a Vampire becomes Wanted in a Faction, all current members of the Faction will immediately know of his Vampirism
                Traits.Vampire vampire = criminal.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
                if (vampire != null) {
                    for (int i = 0; i < faction.characters.Count; i++) {
                        Character factionMember = faction.characters[i];
                        if(factionMember != criminal) {
                            vampire.AddAwareCharacter(factionMember);
                        }
                    }
                }
            } else if (crimeType == CRIME_TYPE.Werewolf) {
                LycanthropeData lycanthropeData = criminal.lycanData;
                if (lycanthropeData != null) {
                    for (int i = 0; i < faction.characters.Count; i++) {
                        Character factionMember = faction.characters[i];
                        if(factionMember != criminal) {
                            lycanthropeData.AddAwareCharacter(factionMember);
                        }
                    }
                }
            }

            Messenger.Broadcast(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, faction, criminal);
        }
    }
    public bool IsWantedBy(Faction faction) {
        return factionsThatConsidersWanted.Contains(faction);
    }
    public bool HasWanted() {
        return factionsThatConsidersWanted.Count > 0;
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataCrimeData data) {
        if (!string.IsNullOrEmpty(data.crime)) {
            if(data.crimableType == CRIMABLE_TYPE.Action) {
                crime = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.crime);
            } else if (data.crimableType == CRIMABLE_TYPE.Interrupt) {
                crime = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.crime);
            }
        }
        if (!string.IsNullOrEmpty(data.criminal)) {
            criminal = CharacterManager.Instance.GetCharacterByPersistentID(data.criminal);
        }
        if (!string.IsNullOrEmpty(data.target)) {
            if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                target = CharacterManager.Instance.GetCharacterByPersistentID(data.target);
            } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                target = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.target);
            }
        }
        if (!string.IsNullOrEmpty(data.targetFaction)) {
            targetFaction = FactionManager.Instance.GetFactionByPersistentID(data.targetFaction);
        }
        if (!string.IsNullOrEmpty(data.judge)) {
            judge = CharacterManager.Instance.GetCharacterByPersistentID(data.judge);
        }
        for (int i = 0; i < data.witnesses.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.witnesses[i]);
            if (character != null) {
                witnesses.Add(character);
            }
        }
        for (int i = 0; i < data.factionsThatConsidersWanted.Count; i++) {
            Faction targetFaction = FactionManager.Instance.GetFactionByPersistentID(data.factionsThatConsidersWanted[i]);
            factionsThatConsidersWanted.Add(targetFaction);
        }
    }
#endregion
}

[System.Serializable]
public class SaveDataCrimeData : SaveData<CrimeData>, ISavableCounterpart {
    public string persistentID { get; set; }
    public CRIME_SEVERITY crimeSeverity;
    public CRIME_TYPE crimeType;
    public CRIME_STATUS crimeStatus;
    public bool isRemoved;


    public string crime;
    public CRIMABLE_TYPE crimableType;
    public string criminal;
    public string target;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public string targetFaction;
    public string judge;
    public List<string> witnesses;
    public List<string> factionsThatConsidersWanted;

#region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Crime;
#endregion

#region Overrides
    public override void Save(CrimeData data) {
        persistentID = data.persistentID;
        crimeSeverity = data.crimeSeverity;
        crimeType = data.crimeType;
        crimeStatus = data.crimeStatus;
        isRemoved = data.isRemoved;

        if (data.crime != null) {
            crime = data.crime.persistentID;
            crimableType = data.crime.crimableType;
            if (data.crime is ActualGoapNode action) {
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(action);
            } else if (data.crime is InterruptHolder interrupt) {
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(interrupt);
            }
        }
        if (data.criminal != null) {
            criminal = data.criminal.persistentID;
        }
        if (data.target != null) {
            target = data.target.persistentID;
            targetPOIType = data.target.poiType;
        }
        if (data.targetFaction != null) {
            targetFaction = data.targetFaction.persistentID;
        }
        if (data.judge != null) {
            judge = data.judge.persistentID;
        }
        witnesses = new List<string>();
        for (int i = 0; i < data.witnesses.Count; i++) {
            witnesses.Add(data.witnesses[i].persistentID);
        }
        factionsThatConsidersWanted = new List<string>();
        for (int i = 0; i < data.factionsThatConsidersWanted.Count; i++) {
            factionsThatConsidersWanted.Add(data.factionsThatConsidersWanted[i].persistentID);
        }
    }

    public override CrimeData Load() {
        CrimeData interrupt = new CrimeData(this);
        return interrupt;
    }
#endregion
}