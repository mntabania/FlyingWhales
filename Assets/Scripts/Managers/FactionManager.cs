using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using Inner_Maps;
using UnityEngine.UI;
using UtilityScripts;

public class FactionManager : BaseMonoBehaviour {

    public static FactionManager Instance = null;

    public Faction neutralFaction { get; private set; }
    public Faction vagrantFaction { get; private set; }
    public Faction disguisedFaction { get; private set; }
    private Faction _undeadFaction;

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _factionEmblems;
    [SerializeField] private Sprite wildMonsterFactionEmblem;
    [SerializeField] private Sprite vagrantFactionEmblem;
    [SerializeField] private Sprite disguisedFactionEmblem;
    [SerializeField] private Sprite undeadFactionEmblem;
    [SerializeField] private Sprite playerFactionEmblem;
    
    private List<Sprite> usedEmblems = new List<Sprite>();

    public readonly string[] exclusiveIdeologyTraitRequirements = new string[] { "Worker", "Combatant", "Royalty" };
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
    #endregion

    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        neutralFaction = null;
        vagrantFaction = null;
        disguisedFaction = null;
        _undeadFaction = null;
        base.OnDestroy();
        Instance = null;
    }

    #region Faction Generation
    public void CreateWildMonsterFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Wild_Monsters);
        newFaction.SetName("Wild Monsters");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(wildMonsterFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetNeutralFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void CreateVagrantFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Vagrants);
        newFaction.SetName("Vagrants");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(vagrantFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetVagrantFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void CreateDisguisedFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Disguised);
        newFaction.SetName("Disguised");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(disguisedFactionEmblem);
        newFaction.factionType.SetAsDefault();
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        SetDisguisedFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
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
    public Faction CreateNewFaction(FACTION_TYPE factionType, string factionName = "") {
        Faction newFaction = new Faction(factionType);
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        newFaction.SetIsMajorFaction(true);
        if (factionType == FACTION_TYPE.Demons) {
            newFaction.SetEmblem(playerFactionEmblem);
            //NOTE: This is always reserved!
            newFaction.SetPathfindingTag(2);
            newFaction.SetPathfindingDoorTag(3);
        } else if (factionType == FACTION_TYPE.Undead) {
            newFaction.SetEmblem(undeadFactionEmblem);
            //NOTE: This is always reserved!
            newFaction.SetPathfindingTag(4);
            newFaction.SetPathfindingDoorTag(5);
        } else {
            newFaction.SetEmblem(GenerateFactionEmblem(newFaction));
            if (newFaction.isMajorNonPlayer) {
                //claim new tags per new MAJOR faction.
                newFaction.SetPathfindingTag(InnerMapManager.Instance.ClaimNextTag());
                newFaction.SetPathfindingDoorTag(InnerMapManager.Instance.ClaimNextTag());    
            }
        }
        CreateRelationshipsForFaction(newFaction);
        if (!string.IsNullOrEmpty(factionName)) {
            newFaction.SetName(factionName);
        }
        
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    private Faction CreateUndeadFaction() {
        Faction undead = CreateNewFaction(FACTION_TYPE.Undead, "Undead");
        undead.SetIsMajorFaction(false);
        foreach (KeyValuePair<Faction,FactionRelationship> pair in undead.relationships) {
            undead.SetRelationshipFor(pair.Key, FACTION_RELATIONSHIP_STATUS.Hostile);
        }
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
        }
        if (newFaction.isMajorNonPlayer) {
            //claim 2 tags per MAJOR non Player faction, this is so that the last tag is still accurate.
            InnerMapManager.Instance.ClaimNextTag();
            InnerMapManager.Instance.ClaimNextTag();
        }
        DatabaseManager.Instance.factionDatabase.RegisterFaction(newFaction);
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
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
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateFactionEmblem(Faction faction) {
        if(usedEmblems.Count == _factionEmblems.Count) {
            usedEmblems.Clear();
        }
        for (int i = 0; i < _factionEmblems.Count; i++) {
            Sprite currSprite = _factionEmblems[i];
            if (usedEmblems.Contains(currSprite)) {
                continue;
            }
            usedEmblems.Add(currSprite);
            return currSprite;
        }
        throw new System.Exception($"There are no more emblems for faction: {faction.name}");
    }
    public Sprite GetFactionEmblem(int emblemIndex) {
        return _factionEmblems[emblemIndex];
        //for (int i = 0; i < _emblemBGs.Count; i++) {
        //    EmblemBG currBG = _emblemBGs[i];
        //    if (currBG.id.Equals(emblemID)) {
        //        return currBG;
        //    }
        //}
        //throw new System.Exception("There is no emblem bg with id " + emblemID);
    }
    public Sprite GetFactionEmblem(string name) {
        if (wildMonsterFactionEmblem.name == name) {
            return wildMonsterFactionEmblem;
        }
        if (vagrantFactionEmblem.name == name) {
            return vagrantFactionEmblem;
        }
        if (disguisedFactionEmblem.name == name) {
            return disguisedFactionEmblem;
        }
        if (undeadFactionEmblem.name == name) {
            return undeadFactionEmblem;
        }
        if (playerFactionEmblem.name == name) {
            return playerFactionEmblem;
        }
        for (int i = 0; i < _factionEmblems.Count; i++) {
            Sprite emblem = _factionEmblems[i];
            if (emblem.name == name) {
                return emblem;
            }
        }
        return null;
    }
    public int GetFactionEmblemIndex(Sprite emblem) {
        for (int i = 0; i < _factionEmblems.Count; i++) {
            Sprite currSetting = _factionEmblems[i];
            if (currSetting == emblem) {
                return i;
            }
            //foreach (KeyValuePair<int, Sprite> kvp in currSetting.emblems) {
            //    if (kvp.Value.name == emblem.name) {
            //        return i;
            //    }
            //}
        }
        return -1;
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
    #endregion

    #region Relationships
    private void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction otherFaction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction otherFaction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
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
        //faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Hostile);
        //faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Hostile);
        //Warmonger warmonger1 = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
        //Warmonger warmonger2 = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
        //faction1.factionType.AddIdeology(warmonger1);
        //faction2.factionType.AddIdeology(warmonger2);
        if (faction1.isPlayerFaction || faction2.isPlayerFaction || 
           faction1 == neutralFaction || faction2 == neutralFaction || 
           faction1 == _undeadFaction || faction2 == _undeadFaction) {

            if ((faction1.isPlayerFaction || faction2.isPlayerFaction) &&
                (faction1 == neutralFaction || faction2 == neutralFaction)) {
                //Player faction should be neutral with Wild Monsters
                //Reference: https://trello.com/c/hqFZ1MC2/1561-player-faction-should-be-neutral-with-wild-monsters
                faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Neutral);
                faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Neutral);
            } else {
                faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Hostile);
                faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Hostile);    
            }
        }
        return newRel;
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
    public void RerollFactionRelationships(Faction faction, Character leader, bool defaultToNeutral, Action<FACTION_RELATIONSHIP_STATUS, Faction, Faction> onSetRelationshipAction = null) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                FactionRelationship factionRelationship = faction.GetRelationshipWith(otherFaction);
                if (otherFaction.isPlayerFaction) {
                    //If Demon Worshipper, friendly with player faction
                    factionRelationship.SetRelationshipStatus(faction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship) ? 
                        FACTION_RELATIONSHIP_STATUS.Friendly : FACTION_RELATIONSHIP_STATUS.Hostile);
                    onSetRelationshipAction?.Invoke(factionRelationship.relationshipStatus, faction, otherFaction);
                } else if (otherFaction.leader != null && otherFaction.leader is Character otherFactionLeader){
                    //Check each Faction Leader of other existing factions if available:
                    if (leader.relationshipContainer.IsEnemiesWith(otherFactionLeader)) {
                        //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
                        factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                    } else if (leader.relationshipContainer.IsFriendsWith(otherFactionLeader)) {
                        //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                        factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
                    } else {
                        if (defaultToNeutral) {
                            //The rest should be set as neutral
                            factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);    
                        }
                    }
                    onSetRelationshipAction?.Invoke(factionRelationship.relationshipStatus, faction, otherFaction);
                }
                
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
        if (leader.traitContainer.HasTrait("Hothead", "Treacherous", "Evil")) {
            Warmonger warmonger = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            faction.factionType.AddIdeology(warmonger);
        } else {
            Peaceful peaceful = CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            faction.factionType.AddIdeology(peaceful);
        }
    }
    public void RerollInclusiveTypeIdeology(Faction faction, Character leader) {
        if (GameUtilities.RollChance(60)) {
            Inclusive inclusive = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
            faction.factionType.AddIdeology(inclusive);
        } else {
            Exclusive exclusive = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            if (GameUtilities.RollChance(60)) {
                exclusive.SetRequirement(leader.race);
            } else {
                exclusive.SetRequirement(leader.gender);
            }
            faction.factionType.AddIdeology(exclusive);
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
    public void RevalidateFactionCrimes(Faction faction, Character leader) {
        if (leader.traitContainer.HasTrait("Vampire")) {
            faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
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
    public FACTION_TYPE GetFactionTypeForCharacter(Character character) {
        if (character.traitContainer.HasTrait("Vampire")) {
            return FACTION_TYPE.Vampire_Clan;
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
    
    public int GetActiveVillagerFactionCount() {
        int count = 0;
        for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++) {
            Faction faction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
            if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || 
                faction.factionType.type == FACTION_TYPE.Human_Empire) {
                count++;
            }
        }
        return count;
    }
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
