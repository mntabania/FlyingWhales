using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnderlingsComponent {
    //public List<Minion> minions { get; private set; }
    //public List<Summon> summons { get; private set; }
    public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges { get; private set; }
    public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges { get; private set; }

    public PlayerUnderlingsComponent() {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges>();
        demonUnderlingCharges = new Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges>();
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
    }
    public PlayerUnderlingsComponent(SaveDataPlayerUnderlingsComponent data) {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        monsterUnderlingCharges = data.monsterUnderlingCharges;
        demonUnderlingCharges = data.demonUnderlingCharges;
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
    }

    #region Utilities
    public void OnCharacterAddedToPlayerFaction(Character p_character) {
        if (p_character is Summon summon) {
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
        Messenger.AddListener<Minion>(PlayerSkillSignals.SUMMON_MINION, OnSummonMinion);
        Messenger.AddListener<Minion>(PlayerSkillSignals.UNSUMMON_MINION, OnUnsummonMinion);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.CHARGES_ADJUSTED, OnSkillChargesAdjusted);
    }
    private void OnSkillChargesAdjusted(SkillData data) {
        if (data is MinionPlayerSkill demonPlayerSkill) {
            SetDemonUnderlingData(demonPlayerSkill.minionType, demonPlayerSkill.charges, demonPlayerSkill.maxCharges);
        }
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
    private void OnGainPlayerMinionSkill(PLAYER_SKILL_TYPE p_skillType) {
        MinionPlayerSkill minionSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_skillType);
        SetDemonUnderlingData(minionSkill.minionType, minionSkill.charges, minionSkill.maxCharges);
    }
    #endregion

    #region Monster Underlings
    public void AddMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, int currentCharges, int maxCharges, CharacterClass p_characterClass) {
        if (!HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = new MonsterAndDemonUnderlingCharges() { monsterType = p_monsterType, currentCharges = currentCharges, maxCharges = maxCharges, characterClass = p_characterClass };
            monsterUnderlingCharges.Add(p_monsterType, m_underlingCharges);
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        }
    }
    public void AddDemonUnderlingEntry(MINION_TYPE p_demonType, int currentCharges, int maxCharges, CharacterClass p_characterClass) {
        if (!HasDemonUnderlingEntry(p_demonType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = new MonsterAndDemonUnderlingCharges() { minionType = p_demonType, currentCharges = currentCharges, maxCharges = maxCharges, characterClass = p_characterClass, isDemon = true };
            demonUnderlingCharges.Add(p_demonType, m_underlingCharges);
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
    public bool HasDemonUnderlingEntry(MINION_TYPE p_demonType) {
        return demonUnderlingCharges.ContainsKey(p_demonType);
    }
    public void AdjustMonsterUnderlingCharge(SUMMON_TYPE p_monsterType, int amount) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = m_underlingCharges.currentCharges;
            charge += amount;
            if (charge > m_underlingCharges.maxCharges) {
                charge = m_underlingCharges.maxCharges;
            }
            m_underlingCharges.currentCharges = charge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, amount, amount, GetSummonCharacterClass(p_monsterType));
        }
    }
    public void SetMonsterUnderlingCharge(SUMMON_TYPE p_monsterType, int amount) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            int charge = amount;
            if (charge > m_underlingCharges.maxCharges) {
                charge = m_underlingCharges.maxCharges;
            }
            m_underlingCharges.currentCharges = charge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, amount, amount, GetSummonCharacterClass(p_monsterType));
        }
    }
    public void AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE p_monsterType, int amount, bool adjustCurrentCharges = true) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
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
            AddMonsterUnderlingEntry(p_monsterType, adjustCurrentCharges ? amount : 0, amount, GetSummonCharacterClass(p_monsterType));
        }
    }
    public void SetMonsterUnderlingMaxCharge(SUMMON_TYPE p_monsterType, int amount, bool includeCurrentCharges = true) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
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
            AddMonsterUnderlingEntry(p_monsterType, includeCurrentCharges ? amount : 0, amount, GetSummonCharacterClass(p_monsterType));
        }
    }
    public void SetDemonUnderlingData(MINION_TYPE p_demonType, int charge, int maxCharge) {
        if (HasDemonUnderlingEntry(p_demonType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = demonUnderlingCharges[p_demonType];
            m_underlingCharges.currentCharges = charge;
            m_underlingCharges.maxCharges = maxCharge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddDemonUnderlingEntry(p_demonType, charge, maxCharge, GetMinionCharacterClass(p_demonType));
        }
    }
    #endregion

    public CharacterClass GetSummonCharacterClass(SUMMON_TYPE p_type) {
        SummonSettings ss = CharacterManager.Instance.GetSummonSettings(p_type);
        CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(ss.className);
        return cClass;
    }

    public CharacterClass GetMinionCharacterClass(MINION_TYPE p_type) {
        MinionSettings ms = CharacterManager.Instance.GetMinionSettings(p_type);
        CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(ms.className);
        return cClass;
    }

    public MonsterAndDemonUnderlingCharges GetSummonUnderlingChargesBySummonType(SUMMON_TYPE p_type) {
        return monsterUnderlingCharges[p_type];
    }

    public MonsterAndDemonUnderlingCharges GetMinionUnderlingChargesByMinionType(MINION_TYPE p_type) {
        return demonUnderlingCharges[p_type];
    }
}

public class SaveDataPlayerUnderlingsComponent : SaveData<PlayerUnderlingsComponent> {
    public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges;
    public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges;

    public override void Save(PlayerUnderlingsComponent data) {
        base.Save(data);
        monsterUnderlingCharges = data.monsterUnderlingCharges;
        demonUnderlingCharges = data.demonUnderlingCharges;
    }
    public override PlayerUnderlingsComponent Load() {
        PlayerUnderlingsComponent component = new PlayerUnderlingsComponent(this);
        return component;
    }
}

[System.Serializable]
public class MonsterAndDemonUnderlingCharges {
    public SUMMON_TYPE monsterType;
    public MINION_TYPE minionType;
    public int currentCharges;
    public int maxCharges;
    public CharacterClass characterClass;
    public bool isDemon;

    public bool hasMaxCharge => maxCharges > 0;
}