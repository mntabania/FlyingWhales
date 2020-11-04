using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public interface ILocation {
    int id { get; }
	LOCATION_IDENTIFIER locIdentifier { get; }
	HexTile tileLocation { get; }
    string locationName { get; }
    List<Character> charactersAtLocation { get; }

    void AddCharacterToLocation(Character character);
	void RemoveCharacterFromLocation(Character character);
    void ReplaceCharacterAtLocation(Character characterToReplace, Character characterToAdd);

    void ScheduleCombatCheck();
    void UnScheduleCombatCheck();
    void CheckForCombat();
    bool HasCombatInitializers();
    void PairUpCombats();
    List<Character> GetCharactersByCombatPriority();
    //void CheckAttackingGroupsCombat();
    //void CheckPatrollingGroupsCombat();
    bool HasHostilities();
    bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false);
    bool HasHostileCharactersWith(ECS.Character character);
    //List<Character> GetAttackingGroups();
    //List<Character> GetPatrollingGroups();
    List<Character> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, Character except = null);
    void StartCombatBetween(Character combatant1, Character combatant2);
    //void StartCombatAtLocation();
    //bool CombatAtLocation();
    //Character GetCombatEnemy(Character combatInitializer);
    //void SetCurrentCombat(ECS.CombatPrototype combat);
    //int CharactersCount(bool includeHostile = false);
    void ContinueDailyActions();

	ECS.Character GetCharacterAtLocationByID (int id, bool includeTraces = false);
	//Party GetPartyAtLocationByLeaderID (int id);
}
