using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class PlayerArchetype {
        public string name { get; protected set; }
        public PLAYER_ARCHETYPE type { get; protected set; }
        public string selectorDescription { get; protected set; }
        public List<string> minionClasses { get; protected set; }
        public List<SPELL_TYPE> spells { get; protected set; }
        public List<SPELL_TYPE> afflictions { get; protected set; }
        public List<SPELL_TYPE> playerActions { get; protected set; }
        public List<SPELL_TYPE> demonicStructuresSkills { get; protected set; }
        public List<SPELL_TYPE> minionPlayerSkills { get; protected set; }
        public List<SPELL_TYPE> summonPlayerSkills { get; protected set; }
        public List<RaceClass> monsters { get; protected set; }
        public bool canTriggerFlaw { get; protected set; }
        public bool canRemoveTraits { get; protected set; }

        public PlayerArchetype(PLAYER_ARCHETYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(type.ToString());
        }

        #region Virtuals
        public virtual bool CanAfflict(SPELL_TYPE type) {
            return afflictions.Contains(type);
        }
        public virtual bool CanDoPlayerAction(SPELL_TYPE type) {
            return playerActions.Contains(type);
        }
        public virtual bool CanSummonMinion(Minion minion) {
            return minionClasses.Contains(minion.character.characterClass.className);
        }
        public virtual bool CanBuildDemonicStructure(SPELL_TYPE type) {
            return demonicStructuresSkills.Contains(type);
        }
        public virtual bool CanCastSpell(SPELL_TYPE type) {
            return spells.Contains(type);
        }
        #endregion

        #region General
        public void SetCanTriggerFlaw(bool state) {
            canTriggerFlaw = state;
        }
        public void SetCanRemoveTraits(bool state) {
            canRemoveTraits = state;
        }
        #endregion

        #region Actions
        public void AddAction(SPELL_TYPE action) {
            if(playerActions == null) { return; }
            if (!playerActions.Contains(action)) {
                playerActions.Add(action);
                Debug.Log($"Action was added to player {action.ToString()}");
            }
        }
        public bool RemoveAction(SPELL_TYPE action) {
            if (playerActions == null) { return false; }
            bool wasRemoved = playerActions.Remove(action);
            if (wasRemoved) {
                Debug.Log($"Action was removed from player {action.ToString()}");
            }
            return wasRemoved;
        }
        #endregion

        #region Minions
        public void AddMinion(string className) {
            if (minionClasses == null) { return; }
            if (!minionClasses.Contains(className)) {
                minionClasses.Add(className);
            }
        }
        public bool RemoveMinion(string className) {
            if (minionClasses == null) { return false; }
            return minionClasses.Remove(className);
        }
        #endregion

        #region Afflictions
        public void AddAffliction(SPELL_TYPE type) {
            if (afflictions == null) { return; }
            if (!afflictions.Contains(type)) {
                afflictions.Add(type);
            }
        }
        public bool RemoveAffliction(SPELL_TYPE type) {
            if (afflictions == null) { return false; }
            return afflictions.Remove(type);
        }
        #endregion

        #region Spells
        public void AddSpell(SPELL_TYPE type) {
            if (spells == null) { return; }
            if (!spells.Contains(type)) {
                spells.Add(type);
                Messenger.Broadcast(Signals.PLAYER_GAINED_SPELL, type);
            }
        }
        public bool RemoveSpell(SPELL_TYPE type) {
            if (spells == null) { return false; }
            if (spells.Remove(type)) {
                Messenger.Broadcast(Signals.PLAYER_LOST_SPELL, type);
                return true;
            }
            return false;
        }
        #endregion

        #region Monsters
        public void AddMonster(RaceClass raceClass) {
            if (monsters == null) { return; }
            if (!monsters.Contains(raceClass)) {
                monsters.Add(raceClass);
            }
        }
        public bool RemoveMonster(RaceClass raceClass) {
            if (monsters == null) { return false; }
            for (int i = 0; i < monsters.Count; i++) {
                if (monsters[i].race == raceClass.race && monsters[i].className == raceClass.className) {
                    monsters.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public bool HasMonster(RACE race, string className) {
            if (monsters == null) { return false; }
            for (int i = 0; i < monsters.Count; i++) {
                if (monsters[i].race == race && monsters[i].className == className) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Demonic Structures
        public void AddDemonicStructure(SPELL_TYPE type) {
            if (demonicStructuresSkills == null) { return; }
            if (!demonicStructuresSkills.Contains(type)) {
                demonicStructuresSkills.Add(type);
                Debug.Log($"Demonic structure was added to player {type.ToString()}");
            }
        }
        public bool RemoveDemonicStructure(SPELL_TYPE type) {
            if (demonicStructuresSkills == null) { return false; }
            bool wasRemoved = demonicStructuresSkills.Remove(type);
            if (wasRemoved) {
                Debug.Log($"Demonic structure was removed from player {type.ToString()}");
            }
            return wasRemoved;
        }
        #endregion
    }
}

public struct RaceClass {
    public RACE race;
    public string className;

    public RaceClass(RACE race, string className) {
        this.race = race;
        this.className = className;
    }
    public override string ToString() {
        return $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(race.ToString())} {className}";
    }
}