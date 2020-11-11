﻿public static class FactionSignals {
    public static string FACTION_CREATED = "OnFactionCreated"; //Parameters (Faction createdFaction)
    public static string CHARACTER_ADDED_TO_FACTION = "OnCharacterAddedToFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string CHARACTER_REMOVED_FROM_FACTION = "OnCharacterRemovedFromFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string FACTION_SET = "OnFactionSet"; //Parameters (Character characterThatSetFaction)
    public static string FACTION_LEADER_DIED = "OnFactionLeaderDied"; //Parameters (Faction affectedFaction)
    public static string FACTION_OWNED_SETTLEMENT_ADDED = "OnFactionOwnedAreaAdded"; //Parameters (Faction affectedFaction, NPCSettlement addedArea)
    public static string FACTION_OWNED_SETTLEMENT_REMOVED = "OnFactionOwnedAreaRemoved"; //Parameters (Faction affectedFaction, NPCSettlement removedArea)
    public static string FACTION_RELATIONSHIP_CHANGED = "OnFactionRelationshipChanged"; //Parameters (FactionRelationship rel)
    public static string FACTION_ACTIVE_CHANGED = "OnFactionActiveChanged"; //Parameters (Faction affectedFaction)
    public static string CHANGE_FACTION_RELATIONSHIP = "OnChangeFactionRelationship";
    public static string CREATE_FACTION_INTERRUPT = "OnCreateFactionInterrupt"; //Parameters (Faction createdFaction, Character creator)
    /// <summary>
    /// Parameters (Faction faction)
    /// </summary>
    public static string FACTION_IDEOLOGIES_CHANGED = "OnFactionIdeologiesChanged";
    /// <summary>
    /// Parameters (Faction faction)
    /// </summary>
    public static string FACTION_CRIMES_CHANGED = "OnFactionCrimesChanged";
}