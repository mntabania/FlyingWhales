using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public interface ICharacter {
    //getters
    Combat currentCombat { get; set; }
    SIDES currentSide { get; }
    int actRate { get; set; }
    int strength { get; }
    int intelligence { get; }
    int agility { get; }
    int baseAgility { get; } //Subject for removal
    int level { get; }
    int currentHP { get; }
    int maxHP { get; }
    int currentRow { get; }
    int id { get; }
    int currentSP { get; }
    string coloredUrlName { get; }
    string name { get; }
    float critChance { get; }
    float critDamage { get; }
    bool isDead { get; }
    GENDER gender { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    List<Skill> skills { get; }
    List<BodyPart> bodyParts { get; }

    //functions
    void SetSide(ECS.SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustHP(int amount);
    void FaintOrDeath();
    void ResetToFullHP();
    void ResetToFullSP();
    int GetPDef(ICharacter enemy);
    int GetMDef(ICharacter enemy);
}
