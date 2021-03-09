using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnderlingsComponent {
    //public List<Minion> minions { get; private set; }
    //public List<Summon> summons { get; private set; }
    public Dictionary<SUMMON_TYPE, MonsterUnderlingCharges> monsterUnderlingCharges { get; private set; }

    public PlayerUnderlingsComponent() {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterUnderlingCharges>();
    }
    public PlayerUnderlingsComponent(SaveDataPlayerUnderlingsComponent data) {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        monsterUnderlingCharges = data.monsterUnderlingCharges;
    }

    #region Utilities
    public void OnCharacterAddedToPlayerFaction(Character p_character) {
        if(p_character is Summon summon) {
            Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_SUMMON, summon);
        }
    }
    public void OnCharacterRemovedFromPlayerFaction(Character p_character) {
        if (p_character is Summon summon) {
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
        }
        if (p_character.minion != null) {
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, p_character.minion);
        }
    }
    public void OnFactionMemberDied(Character character) {
        if (character is Summon summon) {
            // RemoveSummon(summon);
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
        }
    }
    #endregion

    #region Listeners
    public void SubscribeListeners() {
        Messenger.AddListener<Minion>(SpellSignals.SUMMON_MINION, OnSummonMinion);
        Messenger.AddListener<Minion>(SpellSignals.UNSUMMON_MINION, OnUnsummonMinion);
    }
    #endregion

    //#region Summons
    //private void AddSummon(Summon summon) {
    //    if (!summons.Contains(summon)) {
    //        summons.Add(summon);
    //        Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_SUMMON, summon);
    //    }
    //}
    //private void RemoveSummon(Summon summon) {
    //    if (summons.Remove(summon)) {
    //        Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
    //    }
    //}
    //#endregion

    #region Minions
    //public void AddMinion(Minion minion) {
    //    if (!minions.Contains(minion)) {
    //        minions.Add(minion);
    //        Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_MINION, minion);
    //    }
    //}
    //public void RemoveMinion(Minion minion) {
    //    if (minions.Remove(minion)) {
    //        Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, minion);
    //    }
    //}
    private void OnSummonMinion(Minion minion) {
        //AddMinion(minion);
        Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_MINION, minion);
    }
    private void OnUnsummonMinion(Minion minion) {
        //RemoveMinion(minion);
        Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, minion);
    }
    #endregion

    #region Monster Underlings
    public void AddMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, int currentCharges, int maxCharges) {
        if (!HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterUnderlingCharges m_underlingCharges = new MonsterUnderlingCharges() { monsterType = p_monsterType, currentCharges = currentCharges, maxCharges = maxCharges };
            monsterUnderlingCharges.Add(p_monsterType, m_underlingCharges);
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        }
    }
    //public void RemoveMonsterUnderlingEntry(SUMMON_TYPE p_monsterType) {
    //    if (HasMonsterUnderlingEntry(p_monsterType)) {
    //        MonsterUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
    //        monsterUnderlingCharges.Remove(p_monsterType);
    //        Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
    //    }
    //}
    public bool HasMonsterUnderlingEntry(SUMMON_TYPE p_monsterType) {
        return monsterUnderlingCharges.ContainsKey(p_monsterType);
    }
    public void AdjustMonsterUnderlingCharge(SUMMON_TYPE p_monsterType, int amount) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = m_underlingCharges.currentCharges;
            charge += amount;
            if (charge > m_underlingCharges.maxCharges) {
                charge = m_underlingCharges.maxCharges;
            }
            m_underlingCharges.currentCharges = charge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, amount, amount);
        }
    }
    public void SetMonsterUnderlingCharge(SUMMON_TYPE p_monsterType, int amount) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = amount;
            if (charge > m_underlingCharges.maxCharges) {
                charge = m_underlingCharges.maxCharges;
            }
            m_underlingCharges.currentCharges = charge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, amount, amount);
        }
    }
    public void AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE p_monsterType, int amount, bool adjustCurrentCharges = true) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = m_underlingCharges.maxCharges;
            charge += amount;
            if (charge < 0) {
                charge = 0;
            }
            m_underlingCharges.maxCharges = charge;
            if (adjustCurrentCharges) {
                AdjustMonsterUnderlingCharge(p_monsterType, amount);
            } else {
                Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
            }
        } else {
            AddMonsterUnderlingEntry(p_monsterType, adjustCurrentCharges? amount : 0, amount);
        }
    }
    public void SetMonsterUnderlingMaxCharge(SUMMON_TYPE p_monsterType, int amount, bool includeCurrentCharges = true) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = amount;
            if (charge < 0) {
                charge = 0;
            }
            m_underlingCharges.maxCharges = charge;
            if (includeCurrentCharges) {
                m_underlingCharges.currentCharges = charge;
            }
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, includeCurrentCharges ? amount : 0, amount);
        }
    }
    #endregion
}

public class SaveDataPlayerUnderlingsComponent : SaveData<PlayerUnderlingsComponent> {
    public Dictionary<SUMMON_TYPE, MonsterUnderlingCharges> monsterUnderlingCharges;

    public override void Save(PlayerUnderlingsComponent data) {
        base.Save(data);
        monsterUnderlingCharges = data.monsterUnderlingCharges;
    }
    public override PlayerUnderlingsComponent Load() {
        PlayerUnderlingsComponent component = new PlayerUnderlingsComponent(this);
        return component;
    }
}


public class MonsterUnderlingCharges {
    public SUMMON_TYPE monsterType;
    public int currentCharges;
    public int maxCharges;

    public bool hasMaxCharge => maxCharges > 0;
}