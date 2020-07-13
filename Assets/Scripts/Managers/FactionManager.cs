using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using UnityEngine.UI;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    public List<Faction> allFactions = new List<Faction>();
    public Faction neutralFaction { get; private set; }
    public Faction vagrantFaction { get; private set; }
    public Faction disguisedFaction { get; private set; }
    private Faction _undeadFaction;

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _factionEmblems;

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
    #endregion

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    public void CreateWildMonsterFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Wild_Monsters);
        newFaction.SetName("Wild Monsters");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        newFaction.factionType.SetAsDefault();
        allFactions.Add(newFaction);
        SetNeutralFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void CreateVagrantFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Vagrants);
        newFaction.SetName("Vagrants");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        newFaction.factionType.SetAsDefault();
        allFactions.Add(newFaction);
        SetVagrantFaction(newFaction);
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void CreateDisguisedFaction() {
        Faction newFaction = new Faction(FACTION_TYPE.Disguised);
        newFaction.SetName("Disguised");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        newFaction.factionType.SetAsDefault();
        allFactions.Add(newFaction);
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
    public Faction CreateNewFaction(FACTION_TYPE factionType, string factionName = "") {
        Faction newFaction = new Faction(factionType);
        allFactions.Add(newFaction);
        CreateRelationshipsForFaction(newFaction);
        if (!string.IsNullOrEmpty(factionName)) {
            newFaction.SetName(factionName);
        }
        newFaction.SetIsMajorFaction(true);
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
        if(data.name == "Neutral") {
            SetNeutralFaction(newFaction);
        }
        allFactions.Add(newFaction);
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
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    public List<Faction> GetMajorFactionWithRace(RACE race) {
        List<Faction> factions = null;
        for (int i = 0; i < allFactions.Count; i++) {
            Faction faction = allFactions[i];
            if (faction.race == race && faction.isMajorFaction) {
                if (factions == null) {
                    factions = new List<Faction>();
                }
                factions.Add(faction);
            }
        }
        return factions;
    }
    #endregion

    #region Relationships
    private void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
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
        if(faction1.isPlayerFaction || faction2.isPlayerFaction || 
           faction1 == neutralFaction || faction2 == neutralFaction || 
           faction1 == _undeadFaction || faction2 == _undeadFaction) {
            faction1.SetRelationshipFor(faction2, FACTION_RELATIONSHIP_STATUS.Hostile);
            faction2.SetRelationshipFor(faction1, FACTION_RELATIONSHIP_STATUS.Hostile);
        }
        return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
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
    #endregion

    public FACTION_TYPE GetFactionTypeForRace(RACE race) {
        switch (race) {
            case RACE.HUMANS:
                return FACTION_TYPE.Human_Empire;
            case RACE.ELVES:
                return FACTION_TYPE.Elven_Kingdom;
            default:
                return FACTION_TYPE.Human_Empire;
        }
    }
    public int GetActiveVillagerFactionCount() {
        int count = 0;
        for (int i = 0; i < allFactions.Count; i++) {
            Faction faction = allFactions[i];
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
