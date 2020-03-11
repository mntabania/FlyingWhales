using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public interface ICharacter {
    //getters
    //SIDES currentSide { get; }
    float actRate { get; set; }
    int level { get; }
    //int maxHP { get; }
    int currentHP { get; }
    int currentSP { get; }
    int maxSP { get; }
    int currentRow { get; }
    int id { get; }
    int maxHP { get; }
    int attackPower { get; }
    int speed { get; }
    //int combatBaseAttack { get; set; }
    //int combatBaseSpeed { get; set; }
    //int combatBaseHP { get; set; }
    //int combatAttackFlat { get; set; }
    //int combatAttackMultiplier { get; set; }
    //int combatSpeedFlat { get; set; }
    //int combatSpeedMultiplier { get; set; }
    //int combatHPFlat { get; set; }
    //int combatHPMultiplier { get; set; }
    //int combatPowerFlat { get; set; }
    //int combatPowerMultiplier { get; set; }
    string coloredUrlName { get; }
    string urlName { get; }
    string name { get; }
    float computedPower { get; }
    bool isDead { get; }
    int experience { get; }
    int maxExperience { get; }
    GENDER gender { get; }
    ICHARACTER_TYPE icharacterType { get; }
    RACE race { get; }
    NPCSettlement specificLocation { get; }
    Faction faction { get; }
    NPCSettlement homeNpcSettlement { get; }
    CharacterRole role { get; } //Character only
    CharacterClass characterClass { get; } //Character only
    //Job job { get; } //Character only
    Minion minion { get; }
    //PairCombatStats[] pairCombatStats { get; set; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    List<Log> history { get; }
    List<Trait> normalTraits { get; }
    PortraitSettings portraitSettings { get; }
    Party ownParty { get; }
    Party currentParty { get; }
    Dictionary<STAT, float> buffs { get; }
    PlayerCharacterItem playerCharacterItem { get; }
    //CharacterToken characterToken { get; }

    //functions
    void SetName(string name);
    void Initialize();
    void Death(string cause = "normal", GoapAction deathFromAction = null, Character responsibleCharacter = null);
    void LevelUp();
    void OnRemovedFromParty();
    void OnAddedToParty();
    //void SetSide(SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustExperience(int amount);
    void LevelUp(int amount);
    void SetLevel(int amount);
    void SetOwnedParty(Party party);
    void SetCurrentParty(Party party);
    void SetHome(NPCSettlement newHome);
    void AddHistory(Log log); //Character only
    void SetMinion(Minion minion);
    bool AddTrait(Trait combatAttribute, Character responsibleCharacter = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true);
    bool IsInParty();
    bool IsInOwnParty();
    bool RemoveTrait(Trait combatAttribute, bool triggerOnRemove = true, Character removedBy = null);
    Party CreateOwnParty();
    Trait GetNormalTrait<Trait>(params string[] name);
    void ConstructBuffs();
    void SetPlayerCharacterItem(PlayerCharacterItem item);
}
