using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using Factions.Faction_Succession;
using Inner_Maps;
using Object_Pools;
using UnityEngine.UI;
using UtilityScripts;
using Traits;

public class FactionManager : BaseMonoBehaviour {

    public static FactionManager Instance = null;

    public Faction neutralFaction { get; private set; }
    public Faction vagrantFaction { get; private set; }
    public Faction disguisedFaction { get; private set; }
    public Faction ratmenFaction { get; private set; }
    private Faction _undeadFaction;

    // [Space(10)]
    // [Header("Visuals")]
    // [SerializeField] private List<Sprite> _factionEmblems;
    // [SerializeField] private Sprite wildMonsterFactionEmblem;
    // [SerializeField] private Sprite vagrantFactionEmblem;
    // [SerializeField] private Sprite disguisedFactionEmblem;
    // [SerializeField] private Sprite undeadFactionEmblem;
    // [SerializeField] private Sprite playerFactionEmblem;
    // [SerializeField] private Sprite cultFactionEmblem;
    // [SerializeField] private Sprite ratmenFactionEmblem;

    [Space(10)]
    [Header("Character Name Colors")]
    public Color factionNameColor;
    private string _factionNameColorHex;
    
    private Dictionary<FACTION_SUCCESSION_TYPE, FactionSuccession> _factionSuccessions = new Dictionary<FACTION_SUCCESSION_TYPE, FactionSuccession>();

    //public readonly string[] exclusiveIdeologyTraitRequirements = new string[] { "Worker", "Combatant", "Royalty" };
    public readonly FACTION_IDEOLOGY[][] categorizedFactionIdeologies = new FACTION_IDEOLOGY[][] { 
        new FACTION_IDEOLOGY[] { FACTION_IDEOLOGY.Inclusive }, //, FACTION_IDEOLOGY.EXCLUSIVE
        new FACTION_IDEOLOGY[] { FACTION_IDEOLOGY.Warmonger, FACTION_IDEOLOGY.Peaceful },
        new FACTION_IDEOLOGY[] { FACTION_IDEOLOGY.Nature_Worship, FACTION_IDEOLOGY.Divine_Worship, FACTION_IDEOLOGY.Demon_Worship },
    };

    #region getters
    public Faction undeadFaction {
        get {
            if (_undeadFaction == null) {
                _undeadFaction = CreateUndeadFaction();
            }
            return _undeadFaction;
        }
    }
    public List<Faction> allFactions => DatabaseManager.Instance.factionDatabase.allFactionsList;
    public int maxActiveVillagerFactions => WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxActiveFactionsForMapSize();
    #endregion

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        _factionNameColorHex = ColorUtility.ToHtmlStringRGB(factionNameColor);
        ConstructFactionSuccessionTypes();
    }
    protected override void OnDestroy() {
        neutralFaction = null;
        vagrantFaction = null;
        disguisedFaction = null;
        _undeadFaction = null;
        _factionSuccessions?.Clear();
        _factionSuccessions = null;
        base.OnDestroy();
        Instance = null;
    }

    #region Faction Generation
    public void CreateWildMonsterFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Wild_Monsters);
        newFaction.SetName("Wild Monsters");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(FactionEmblemRandomizer.wildMonsterFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetNeutralFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
    }
    public void CreateVagrantFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Vagrants);
        newFaction.SetName("Vagrants");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(FactionEmblemRandomizer.vagrantFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetVagrantFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
        newFaction.isInfoUnlocked = true;
    }
    public void CreateDisguisedFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Disguised);
        newFaction.SetName("Disguised");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(FactionEmblemRandomizer.disguisedFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetDisguisedFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
    }
    public void CreateRatmenFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Ratmen);
        newFaction.SetName("Ratmen");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(FactionEmblemRandomizer.ratmenFactionEmblem);
        newFaction.factionType.SetAsDefault();
        newFaction.SetPathfindingTag(InnerMapManager.Ratmen_Faction);
        newFaction.SetPathfindingDoorTag(InnerMapManager.Ratmen_Faction_Doors);
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetRatmenFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
    }
    private void SetNeutralFaction(Faction faction) {
        neutralFaction = faction;
    }
    private void SetVagrantFaction(Faction faction) {
        vagrantFaction = faction;
    }
    private void SetDisguisedFaction(Faction faction) {
        disguisedFaction = faction;
    }
    private void SetUndeadFaction(Faction faction) {
        _undeadFaction = faction;
    }
    private void SetRatmenFaction(Faction faction) {
        ratmenFaction = faction;
    }
    public Faction CreateNewFaction(FACTION_TYPE factionType, string factionName = "", Sprite factionEmblem = null, RACE race = RACE.NONE) {
        Faction newFaction = new Faction(factionType, race);
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        newFaction.SetIsMajorFaction(true);
        if (factionEmblem == null) {
            DetermineFactionEmblem(newFaction);    
        } else {
          newFaction.SetEmblem(factionEmblem);  
        }
        DetermineFactionPathfindingTags(newFaction);
        if (!string.IsNullOrEmpty(factionName)) {
            newFaction.SetName(factionName);
        }
        CreateRelationshipsForFaction(newFaction);
        
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
        }
        
        if (newFaction.race.IsSapient()) {
            if (newFaction.factionType.type == FACTION_TYPE.Undead) {
                newFaction.isInfoUnlocked = true;
            } else {
                newFaction.isInfoUnlocked = false;
            }
        }
        return newFaction;
    }
    private void DetermineFactionEmblem(Faction faction) {
        FACTION_TYPE factionType = faction.factionType.type;
        if (factionType == FACTION_TYPE.Demons) {
            faction.SetEmblem(FactionEmblemRandomizer.playerFactionEmblem);
        } else if (factionType == FACTION_TYPE.Undead) {
            faction.SetEmblem(FactionEmblemRandomizer.undeadFactionEmblem);
        } else if (factionType == FACTION_TYPE.Ratmen) {
            faction.SetEmblem(FactionEmblemRandomizer.ratmenFactionEmblem);
        } else if (factionType == FACTION_TYPE.Demon_Cult && DatabaseManager.Instance.factionDatabase.allFactionsList.Count(f => f.factionType.type == FACTION_TYPE.Demon_Cult) == 1) {
            //only set cult faction emblem on first cult faction.
            faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
        } else {
            Sprite factionEmblem = FactionEmblemRandomizer.GetUnusedFactionEmblem();
            faction.SetEmblem(factionEmblem);
            FactionEmblemRandomizer.SetEmblemAsUsed(factionEmblem);
        }
    }
    private void DetermineFactionPathfindingTags(Faction faction) {
        FACTION_TYPE factionType = faction.factionType.type;
        if (factionType == FACTION_TYPE.Demons) {
            //NOTE: This is always reserved!
            faction.SetPathfindingTag(InnerMapManager.Demonic_Faction);
            faction.SetPathfindingDoorTag(InnerMapManager.Demonic_Faction_Doors);
        } else if (factionType == FACTION_TYPE.Undead) {
            //NOTE: This is always reserved!
            faction.SetPathfindingTag(InnerMapManager.Undead_Faction);
            faction.SetPathfindingDoorTag(InnerMapManager.Undead_Faction_Doors);
        } else if (factionType == FACTION_TYPE.Ratmen) {
            //NOTE: This is always reserved!
            faction.SetPathfindingTag(InnerMapManager.Ratmen_Faction);
            faction.SetPathfindingDoorTag(InnerMapManager.Ratmen_Faction_Doors);
        } else {
            if (faction.isMajorNonPlayer) {
                PathfindingTagPair pathfindingTagPair = InnerMapManager.Instance.ClaimNextPathfindingTagPair(); 
                //claim new tags per new MAJOR faction.
                faction.SetPathfindingTag(pathfindingTagPair.groundTag);
                faction.SetPathfindingDoorTag(pathfindingTagPair.doorsTag);    
            }
        }
    }
    private Faction CreateUndeadFaction() {
        Faction undead = CreateNewFaction(FACTION_TYPE.Undead, "Undead");
        undead.SetIsMajorFaction(false);
        CreateRelationshipsForFaction(undead);
        //foreach (KeyValuePair<Faction,FactionRelationship> pair in undead.relationships) {
        //    undead.SetRelationshipFor(pair.Key, FACTION_RELATIONSHIP_STATUS.Hostile);
        //}
        return undead;
    }
    public Faction CreateNewFaction(SaveDataFaction data) {
        Faction newFaction = new Faction(data);
        if(data.factionType.type == FACTION_TYPE.Disguised) {
            SetDisguisedFaction(newFaction);
        } else if (data.factionType.type == FACTION_TYPE.Undead) {
            SetUndeadFaction(newFaction);
        } else if (data.factionType.type == FACTION_TYPE.Wild_Monsters) {
            SetNeutralFaction(newFaction);
        } else if (data.factionType.type == FACTION_TYPE.Vagrants) {
            SetVagrantFaction(newFaction);
        } else if (data.factionType.type == FACTION_TYPE.Ratmen) {
            SetRatmenFaction(newFaction);
        }
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(FactionSignals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public void DeleteFaction(Faction faction) {
        //for (int i = 0; i < faction.ownedRegions.Count; i++) {
        //    NPCSettlement ownedArea = faction.ownedRegions[i];
        //    LandmarkManager.Instance.UnownArea(ownedArea);
        //}
        //RemoveRelationshipsWith(faction);
        //Messenger.Broadcast(Signals.FACTION_DELETED, faction);
        //allFactions.Remove(faction);
    }
    public Faction GetRandomMajorNonPlayerFaction() {
        return DatabaseManager.Instance.factionDatabase.GetRandomMajorNonPlayerFaction();
    }
    public bool LeaveFaction(Character character) {
        Faction targetFaction = null;
        if(character.minion != null) {
            targetFaction = PlayerManager.Instance.player.playerFaction;
        } else if (character is Summon summon) {
            targetFaction = summon.defaultFaction;
        } else {
            targetFaction = vagrantFaction;
        }

        return character.ChangeFactionTo(targetFaction);
    }
    #endregion

    #region Emblem
    public Sprite GetFactionEmblem(SaveDataFaction p_data) {
        if (p_data.factionType.type == FACTION_TYPE.Wild_Monsters) {
            return FactionEmblemRandomizer.wildMonsterFactionEmblem;
        }
        if (p_data.factionType.type == FACTION_TYPE.Vagrants) {
            return FactionEmblemRandomizer.vagrantFactionEmblem;
        }
        if (p_data.factionType.type == FACTION_TYPE.Disguised) {
            return FactionEmblemRandomizer.disguisedFactionEmblem;
        }
        if (p_data.factionType.type == FACTION_TYPE.Undead) {
            return FactionEmblemRandomizer.undeadFactionEmblem;
        }
        if (p_data.factionType.type == FACTION_TYPE.Demons) {
            return FactionEmblemRandomizer.playerFactionEmblem;
        }
        if (p_data.factionType.type == FACTION_TYPE.Ratmen) {
            return FactionEmblemRandomizer.ratmenFactionEmblem;
        }
        if (p_data.emblemName == FactionEmblemRandomizer.cultFactionEmblem.name) {
            return FactionEmblemRandomizer.cultFactionEmblem;
        }
        for (int i = 0; i < FactionEmblemRandomizer.allEmblems.Count; i++) {
            Sprite emblem = FactionEmblemRandomizer.allEmblems[i];
            if (emblem.name == p_data.emblemName) {
                return emblem;
            }
        }
        return null;
    }
    #endregion

    #region Utilities
    public Faction GetFactionBasedOnID(int id) {
        return DatabaseManager.Instance.factionDatabase.GetFactionBasedOnID(id);
    }
    public Faction GetFactionByPersistentID(string id) {
        return DatabaseManager.Instance.factionDatabase.GetFactionByPersistentID(id);
    }
    public Faction GetFactionBasedOnName(string name) {
        return DatabaseManager.Instance.factionDatabase.GetFactionBasedOnName(name);
    }
    public List<Faction> GetMajorFactionWithRace(RACE race) {
        return DatabaseManager.Instance.factionDatabase.GetMajorFactionWithRace(race);
    }
    public string GetFactionNameColorHex() {
        return _factionNameColorHex;
    }
    public Faction GetDefaultFactionForMonster(SUMMON_TYPE summonType) {
        switch (summonType) {
            case SUMMON_TYPE.Ghost:
            case SUMMON_TYPE.Skeleton:
            case SUMMON_TYPE.Vengeful_Ghost:
            case SUMMON_TYPE.Revenant:
                return undeadFaction;
            default:
                return neutralFaction;
        }
    }
    public int GetActiveVillagerFactionCount() {
        int count = 0;
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction faction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if (faction.isMajorNonPlayer && !faction.isDisbanded) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Relationships
    private void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction otherFaction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if(otherFaction.id != faction.id) {
                //only create relationships for the following factions:
                // - Villager factions that are not yet disbanded
                // - Non-Villager factions
                // - The Player Faction
                if ((otherFaction.isMajorNonPlayer && !otherFaction.isDisbanded) || !otherFaction.isMajorNonPlayer || otherFaction.isPlayerFaction) {
                    CreateNewRelationshipBetween(otherFaction, faction);    
                }
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction otherFaction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
                faction.RemoveRelationshipWith(otherFaction);
            }
        }
    }
    /// <summary>
    /// Create a new relationship between 2 factions,
    /// then add add a reference to that relationship, to both of the factions.
    /// </summary>
    /// <param name="faction1">First faction</param>
    /// <param name="faction2">Other faction</param>
    /// <returns>Created relationship</returns>
    public FactionRelationship CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);

        FACTION_RELATIONSHIP_STATUS status = GetInitialFactionRelationshipStatus(faction1, faction2);
        faction1.SetRelationshipFor(faction2, status);
        faction2.SetRelationshipFor(faction1, status);

#if DEBUG_LOG
        Debug.Log($"Created new relationship between {faction1.name} and {faction2.name}");
#endif
        
        //if (faction1.isPlayerFaction || faction2.isPlayerFaction || 
        //   faction1.factionType.type == FACTION_TYPE.Wild_Monsters || faction2.factionType.type == FACTION_TYPE.Wild_Monsters || 
        //   faction1.factionType.type == FACTION_TYPE.Undead || faction2.factionType.type == FACTION_TYPE.Undead ||
        //   faction1.factionType.type == FACTION_TYPE.Ratmen || faction2.factionType.type == FACTION_TYPE.Ratmen) {

        //    if ((faction1.isPlayerFaction || faction2.isPlayerFaction) &&
        //        (faction1 == neutralFaction || faction2 == neutralFaction)) {

        //        faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Neutral);
        //        faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Neutral);
        //    } else {
        //        faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Hostile);
        //        faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Hostile);    
        //    }
        //}
        return newRel;
    }
    private FACTION_RELATIONSHIP_STATUS GetInitialFactionRelationshipStatus(Faction faction1, Faction faction2) {
        if(faction1.isPlayerFaction || faction2.isPlayerFaction) {
            //Player faction should be neutral with Wild Monsters
            //Reference: https://trello.com/c/hqFZ1MC2/1561-player-faction-should-be-neutral-with-wild-monsters
            if (faction1.factionType.type == FACTION_TYPE.Wild_Monsters || faction2.factionType.type == FACTION_TYPE.Wild_Monsters) {
                return FACTION_RELATIONSHIP_STATUS.Neutral;
            } else if (faction1.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Demon_Cult) {
                return FACTION_RELATIONSHIP_STATUS.Friendly;
            } else {
                return FACTION_RELATIONSHIP_STATUS.Hostile;
            }
        } else if (faction1.factionType.type == FACTION_TYPE.Wild_Monsters || faction2.factionType.type == FACTION_TYPE.Wild_Monsters) {
            //Player faction should be neutral with Wild Monsters
            //Reference: https://trello.com/c/hqFZ1MC2/1561-player-faction-should-be-neutral-with-wild-monsters
            if (faction1.isPlayerFaction || faction2.isPlayerFaction) {
                return FACTION_RELATIONSHIP_STATUS.Neutral;
            } else {
                return FACTION_RELATIONSHIP_STATUS.Hostile;
            }
        } else if (faction1.factionType.type == FACTION_TYPE.Undead || faction2.factionType.type == FACTION_TYPE.Undead) {
            if (faction1.factionType.type == FACTION_TYPE.Ratmen || faction2.factionType.type == FACTION_TYPE.Ratmen) {
                return FACTION_RELATIONSHIP_STATUS.Neutral;
            } else {
                return FACTION_RELATIONSHIP_STATUS.Hostile;
            }
        } else if (faction1.factionType.type == FACTION_TYPE.Ratmen || faction2.factionType.type == FACTION_TYPE.Ratmen) {
            if (faction1.factionType.type == FACTION_TYPE.Undead || faction2.factionType.type == FACTION_TYPE.Undead) {
                return FACTION_RELATIONSHIP_STATUS.Neutral;
            } else {
                return FACTION_RELATIONSHIP_STATUS.Hostile;
            }
        }
        return FACTION_RELATIONSHIP_STATUS.Neutral;
    }
    /// <summary>
    /// Utility Function for getting the relationship between 2 factions,
    /// this just adds a checking for data consistency if, the 2 factions have the
    /// same reference to their relationship.
    /// NOTE: This is probably more performance intensive because of the additional checking.
    /// User can opt to use each factions GetRelationshipWith() instead.
    /// </summary>
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception($"{faction1.name} does not have the same relationship object as {faction2.name}!");
    }
    //public int GetAverageFactionLevel() {
    //    int activeFactionsCount = allFactions.Where(x => x.isActive).Count();
    //    int totalFactionLvl = allFactions.Where(x => x.isActive).Sum(x => x.level);
    //    return totalFactionLvl / activeFactionsCount;
    //}
    public void RerollFactionRelationships(Faction faction, Character leader, bool defaultToNeutral, bool logRelationshipChangeFromLeaderRelationship) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                FactionRelationship factionRelationship = faction.GetRelationshipWith(otherFaction);
                if (factionRelationship == null) { continue; }
                FACTION_RELATIONSHIP_STATUS newStatus = factionRelationship.relationshipStatus;
                if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship)) {
                    if (otherFaction.isPlayerFaction || otherFaction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship)) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Friendly;
                    }
                } else {
                    //if other faction is player faction and this faction is not a demon worshipper, revert relationship with player faction to hostile.
                    if (otherFaction.isPlayerFaction) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
                    }
                }
                
                if (otherFaction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                    //If the other faction is a Vampire Clan
                    //And this faction is a Vampire Clan - Neutral, but if this facton is Lycan Clan - Hostile
                    if (faction.factionType.type == FACTION_TYPE.Lycan_Clan) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
                    } else if (faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Neutral;
                    }
                }
                if (otherFaction.factionType.type == FACTION_TYPE.Lycan_Clan) {
                    //If the other faction is a Lycan Clan
                    //And this faction is a Lycan Clan - Neutral, but if this facton is Vampire Clan - Hostile
                    if (faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
                    } else if (faction.factionType.type == FACTION_TYPE.Lycan_Clan) {
                        newStatus = FACTION_RELATIONSHIP_STATUS.Neutral;
                    }
                }
                if (otherFaction.leader != null && otherFaction.leader is Character otherFactionLeader) {
                    if (leader.relationshipContainer.IsEnemiesWith(otherFactionLeader)) {
                        //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
                        factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                        if (logRelationshipChangeFromLeaderRelationship) {
                            LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS.Hostile, faction, otherFaction);    
                        }
                    } else if (leader.relationshipContainer.IsFriendsWith(otherFactionLeader)) {
                        //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                        factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
                        if (logRelationshipChangeFromLeaderRelationship) {
                            LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS.Friendly, faction, otherFaction);
                        }
                    } else {
                        // if (defaultToNeutral) {
                        //     //The rest should be set as neutral
                        //     factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);    
                        // }
                        factionRelationship.SetRelationshipStatus(newStatus);
                    }    
                } else {
                    
                    factionRelationship.SetRelationshipStatus(newStatus);
                }
            }
        }
    }
    private void LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS status, Faction faction, Faction otherFaction) {
        if (!otherFaction.isPlayerFaction) {
            //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
            if (status == FACTION_RELATIONSHIP_STATUS.Hostile) {
                Log dislikeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "dislike_leader", null, LOG_TAG.Major);
                dislikeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                dislikeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_war", null, LOG_TAG.Major);
                log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                log.AddToFillers(dislikeLog.fillers);
                log.AddToFillers(null, dislikeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                log.AddLogToDatabase();    
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                
                LogPool.Release(dislikeLog);
            } else if (status == FACTION_RELATIONSHIP_STATUS.Friendly) {
                //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                Log likeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "like_leader", null, LOG_TAG.Major);
                likeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                likeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_peace", null, LOG_TAG.Major);
                log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                log.AddToFillers(likeLog.fillers);
                log.AddToFillers(null, likeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                
                LogPool.Release(likeLog);
            }    
        }
    }
    #endregion

    #region Faction Ideologies
    public T CreateIdeology<T>(FACTION_IDEOLOGY ideologyType) where T : FactionIdeology {
        string ideologyStr = ideologyType.ToString();
        var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(ideologyStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            T data = System.Activator.CreateInstance(type) as T;
            return data;
        } else {
            throw new System.Exception($"{ideologyStr} has no data!");
        }
    }
    public void RerollPeaceTypeIdeology(Faction faction, Character leader) {
        if (leader.traitContainer.HasTrait("Hothead", "Treacherous", "Evil", "Cultist")) {
            Warmonger warmonger = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            faction.factionType.AddIdeology(warmonger);
            return;
        } else if (leader.traitContainer.HasTrait("Vampire")) {
            Vampire vampireTrait = leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (!vampireTrait.dislikedBeingVampire) {
                Warmonger warmonger = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
                faction.factionType.AddIdeology(warmonger);
                return;
            }
        } else if (leader.traitContainer.HasTrait("Lycanthrope")) {
            //TODO: Checking if desires being lycan, see above checking for vampire
            Warmonger warmonger = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            faction.factionType.AddIdeology(warmonger);
            return;
        }
        Peaceful peaceful = CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
        faction.factionType.AddIdeology(peaceful);
    }
    public void RerollInclusiveTypeIdeology(Faction faction, Character leader) {
        if (faction.factionType.type == FACTION_TYPE.Demon_Cult) {
            //If Demon Cult, 100% chance Demon Worshipper Exclusive

            //Remove first the existing Exclusive ideology so it can be replaced with a new one that has a diff requirement
            faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive);

            Exclusive exclusive = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RELIGION.Demon_Worship);
            faction.factionType.AddIdeology(exclusive);
        } else if (faction.factionType.type == FACTION_TYPE.Undead) {
            Inclusive inclusive = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
            faction.factionType.AddIdeology(inclusive);
        } else {
            if (GameUtilities.RollChance(60)) {
                Inclusive inclusive = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
                faction.factionType.AddIdeology(inclusive);
            } else {
                //Remove first the existing Exclusive ideology so it can be replaced with a new one that has a diff requirement
                faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive);

                Exclusive exclusive = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
                if(faction.factionType.type == FACTION_TYPE.Vampire_Clan && GameUtilities.RollChance(35)) {
                    exclusive.SetRequirement("Vampire");
                } else if (faction.factionType.type == FACTION_TYPE.Lycan_Clan && GameUtilities.RollChance(35)) {
                    exclusive.SetRequirement("Lycanthrope");
                } else if (GameUtilities.RollChance(60)) {
                    exclusive.SetRequirement(leader.race);
                } else {
                    exclusive.SetRequirement(leader.gender);
                }
                faction.factionType.AddIdeology(exclusive);
            }    
        } 
        
    }
    public void RerollReligionTypeIdeology(Faction faction, Character leader) {
        if (leader.traitContainer.HasTrait("Cultist")) {
            DemonWorship inclusive = CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
            faction.factionType.AddIdeology(inclusive);
        } else if (leader.race == RACE.ELVES) {
            NatureWorship natureWorship = CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
            faction.factionType.AddIdeology(natureWorship);
        } else if (leader.race == RACE.HUMANS) {
            DivineWorship divineWorship = CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
            faction.factionType.AddIdeology(divineWorship);
        }
    }
    public void RerollFactionLeaderTraitIdeology(Faction faction, Character leader) {
        bool shouldRevereVampires = false;
        bool shouldRevereWerewolves = false;
        bool shouldHateVampires = false;
        bool shouldHateWerewolves = false;

        if (leader.traitContainer.HasTrait("Vampire")) {
            Vampire vampire = leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if(!vampire.dislikedBeingVampire && GameUtilities.RollChance(50)) {
                shouldRevereVampires = true;
            }
        }
        if (leader.isLycanthrope && !leader.lycanData.dislikesBeingLycan && GameUtilities.RollChance(50)) {
            shouldRevereWerewolves = true;
        }

        if (leader.traitContainer.HasTrait("Hemophiliac")) {
            shouldRevereVampires = true;
        } else if (leader.traitContainer.HasTrait("Hemophobic")) {
            shouldHateVampires = true;
        }
        if (leader.traitContainer.HasTrait("Lycanthrope")) {
            if (!leader.lycanData.dislikesBeingLycan && GameUtilities.RollChance(50)) {
                shouldRevereWerewolves = true;
            }
        }
        if (leader.traitContainer.HasTrait("Lycanphiliac")) {
            shouldRevereWerewolves = true;
        } else if (leader.traitContainer.HasTrait("Lycanphobic")) {
            shouldHateWerewolves = true;
        }
        if (shouldRevereVampires) {
            faction.factionType.AddIdeology(FACTION_IDEOLOGY.Reveres_Vampires);
        } else if (shouldHateVampires) {
            faction.factionType.AddIdeology(FACTION_IDEOLOGY.Hates_Vampires);
        }
        if (shouldRevereWerewolves) {
            faction.factionType.AddIdeology(FACTION_IDEOLOGY.Reveres_Werewolves);
        } else if (shouldHateWerewolves) {
            faction.factionType.AddIdeology(FACTION_IDEOLOGY.Hates_Werewolves);
        }
    }
    public void RevalidateFactionCrimes(Faction faction, Character leader) {
        //religion based crimes
        if (faction.factionType.type == FACTION_TYPE.Demon_Cult || leader.traitContainer.HasTrait("Cultist")) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Demon_Worship);
            faction.factionType.AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Serious);
            faction.factionType.AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Serious);
            faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Divine_Worship);
            faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Nature_Worship);
        } else if (leader.religionComponent.religion == RELIGION.Divine_Worship) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Divine_Worship);
            faction.factionType.AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Serious);
        } else if (leader.religionComponent.religion == RELIGION.Nature_Worship) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
            faction.factionType.AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Serious);
        } else if (leader.religionComponent.religion == RELIGION.Demon_Worship) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Demon_Worship);
        }
        //vampire based crimes
        if (leader.traitContainer.HasTrait("Vampire")) {
            Vampire vampire = leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (!vampire.dislikedBeingVampire) {
                faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);    
            }
        } else if (leader.traitContainer.HasTrait("Hemophiliac")) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
        } else if (leader.traitContainer.HasTrait("Hemophobic")) {
            faction.factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
        }
        //lycanthrope based crimes
        if ((leader.lycanData != null && !leader.lycanData.dislikesBeingLycan) || leader.traitContainer.HasTrait("Lycanphiliac")) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Werewolf);
        } else if (leader.traitContainer.HasTrait("Lycanphobic")) {
            faction.factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
        }
        //Kleptomaniac based crimes
        if (leader.traitContainer.HasTrait("Kleptomaniac")) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Theft);
        } else {
            faction.factionType.AddCrime(CRIME_TYPE.Theft, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Theft));
        }
        //Evil based crimes
        if (leader.traitContainer.HasTrait("Evil", "Psychopath")) {
            CRIME_TYPE crimeType = faction.factionType.GetRandomNonReligionSeriousCrime();
            if (crimeType != CRIME_TYPE.None) {
                faction.factionType.RemoveCrime(crimeType);    
            }
        }
    }
    #endregion

    #region Faction Type
    public FactionType CreateFactionType(FACTION_TYPE factionType) {
        string enumStr = factionType.ToString();
        var typeName = $"Factions.Faction_Types.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(enumStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            FactionType data = Activator.CreateInstance(type) as FactionType;
            return data;
        } else {
            throw new Exception($"{typeName} has no data!");
        }
    }
    public FactionType CreateFactionType(FACTION_TYPE factionType, SaveDataFactionType saveData) {
        string enumStr = factionType.ToString();
        var typeName = $"Factions.Faction_Types.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(enumStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            FactionType data = Activator.CreateInstance(type, saveData) as FactionType;
            return data;
        } else {
            throw new Exception($"{typeName} has no data!");
        }
    }
    public FACTION_TYPE GetFactionTypeForCharacter(Character character) {
        if (character.characterClass.className == "Cult Leader") {
            return FACTION_TYPE.Demon_Cult;
        } else if (character.traitContainer.HasTrait("Vampire") && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM).currentLevel >= 3) {
            return FACTION_TYPE.Vampire_Clan;
        } else if (character.isLycanthrope && character.lycanData.isMaster
            && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LYCANTHROPY).currentLevel >= 3) {
            return FACTION_TYPE.Lycan_Clan;
        } else if (character.traitContainer.HasTrait("Cultist")) {
            return FACTION_TYPE.Demon_Cult;
        } 
        return GetFactionTypeForRace(character.race);
    }
    public FACTION_TYPE GetFactionTypeForRace(RACE race) {
        switch (race) {
            case RACE.HUMANS:
                return FACTION_TYPE.Human_Empire;
            case RACE.ELVES:
                return FACTION_TYPE.Elven_Kingdom;
            default:
                //will always default to human empire for now, so if any other race will create a faction
                //it is expected that it will have the Human Empire faction type.
                return FACTION_TYPE.Human_Empire;
        }
    }
    #endregion

    #region Faction Succession
    private void ConstructFactionSuccessionTypes() {
        FACTION_SUCCESSION_TYPE[] types = CollectionUtilities.GetEnumValues<FACTION_SUCCESSION_TYPE>();
        for (int i = 0; i < types.Length; i++) {
            FACTION_SUCCESSION_TYPE type = types[i];
            if(type == FACTION_SUCCESSION_TYPE.None) {
                _factionSuccessions.Add(type, new FactionSuccession(type));
            } else {
                _factionSuccessions.Add(type, CreateFactionSuccession(type));
            }
        }
    }
    public FactionSuccession CreateFactionSuccession(FACTION_SUCCESSION_TYPE successionType) {
        string enumStr = successionType.ToString();
        var typeName = $"Factions.Faction_Succession.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(enumStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            FactionSuccession data = Activator.CreateInstance(type) as FactionSuccession;
            return data;
        } else {
            throw new Exception($"{typeName} has no data!");
        }
    }
    public FactionSuccession GetFactionSuccession (FACTION_SUCCESSION_TYPE type) {
        if (_factionSuccessions.ContainsKey(type)) {
            return _factionSuccessions[type];
        }
        return null;
    }
    #endregion
    
    
}

[System.Serializable]
public class FactionEmblemSetting {
    public FactionEmblemDictionary emblems;

    public Sprite GetSpriteForSize(Image image) {
        if (image.rectTransform.sizeDelta.x <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
    public Sprite GetSpriteForSize(int size) {
        if (size <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
}
