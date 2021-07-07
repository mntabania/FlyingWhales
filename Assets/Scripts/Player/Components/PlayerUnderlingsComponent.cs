using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class PlayerUnderlingsComponent {
    //public List<Minion> minions { get; private set; }
    //public List<Summon> summons { get; private set; }
    public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges { get; private set; }
    public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges { get; private set; }
    public readonly int cooldown;
    public PlayerUnderlingsComponent() {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        cooldown = GameManager.Instance.GetTicksBasedOnHour(12);
        monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges>();
        demonUnderlingCharges = new Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges>();
    }
    public PlayerUnderlingsComponent(SaveDataPlayerUnderlingsComponent data) {
        //minions = new List<Minion>();
        //summons = new List<Summon>();
        cooldown = GameManager.Instance.GetTicksBasedOnHour(12);
        monsterUnderlingCharges = data.monsterUnderlingCharges;
        demonUnderlingCharges = data.demonUnderlingCharges;
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
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
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
    public void AddMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, int currentCharges, int maxCharges, string p_characterClassName) {
        if (!HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = new MonsterAndDemonUnderlingCharges(p_monsterType, currentCharges, maxCharges, p_characterClassName);
            monsterUnderlingCharges.Add(p_monsterType, m_underlingCharges);
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        }
    }
    public void AddDemonUnderlingEntry(MINION_TYPE p_demonType, int currentCharges, int maxCharges, string p_characterClassName) {
        if (!HasDemonUnderlingEntry(p_demonType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = new MonsterAndDemonUnderlingCharges(p_demonType, currentCharges, maxCharges, p_characterClassName);
            demonUnderlingCharges.Add(p_demonType, m_underlingCharges);
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        }
    }
    public void GainMonsterUnderlingMaxChargesFromKennel(SUMMON_TYPE summonType, int amount) {
        Assert.IsTrue(amount > 0);
        //Related Task: https://trello.com/c/Bj13iHJW/4317-kennel-monster-transfer-exploit
        AdjustMonsterUnderlingMaxCharge(summonType, amount, false);
        if (HasMonsterUnderlingEntry(summonType, out var monsterUnderling)) {
            monsterUnderling.StartMonsterReplenish();
        }
    }
    public void LoseMonsterUnderlingMaxChargesFromKennel(SUMMON_TYPE summonType, int amount) {
        Assert.IsTrue(amount < 0);
        //Related Task: https://trello.com/c/Bj13iHJW/4317-kennel-monster-transfer-exploit
        //Expected behaviour is that current charges will be clamped by max charges
        //Example: 4/6 charges before adjustment, should become 3/3 after losing max charges.
        //Only denominator will be reduced.
        AdjustMonsterUnderlingMaxCharge(summonType, amount, false);
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
    private bool HasMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, out MonsterAndDemonUnderlingCharges p_monsterAndDemonUnderlingCharges) {
        if (monsterUnderlingCharges.ContainsKey(p_monsterType)) {
            p_monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
            return true;
        }
        p_monsterAndDemonUnderlingCharges = null;
        return false;
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
            } else if (charge < 0) {
                charge = 0;
            }
            m_underlingCharges.currentCharges = charge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddMonsterUnderlingEntry(p_monsterType, amount, amount, CharacterManager.Instance.GetSummonSettings(p_monsterType).className);
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
            AddMonsterUnderlingEntry(p_monsterType, amount, amount, CharacterManager.Instance.GetSummonSettings(p_monsterType).className);
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
                if (amount < 0) { m_underlingCharges.OnLoseMaxCharges(); }
            } else {
                if (amount < 0) { m_underlingCharges.OnLoseMaxCharges(); }
                Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
            }
        } else {
            AddMonsterUnderlingEntry(p_monsterType, adjustCurrentCharges ? amount : 0, amount, CharacterManager.Instance.GetSummonSettings(p_monsterType).className);
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
            AddMonsterUnderlingEntry(p_monsterType, includeCurrentCharges ? amount : 0, amount, CharacterManager.Instance.GetSummonSettings(p_monsterType).className);
        }
    }
    public void SetDemonUnderlingData(MINION_TYPE p_demonType, int charge, int maxCharge) {
        if (HasDemonUnderlingEntry(p_demonType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = demonUnderlingCharges[p_demonType];
            m_underlingCharges.currentCharges = charge;
            m_underlingCharges.maxCharges = maxCharge;
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        } else {
            AddDemonUnderlingEntry(p_demonType, charge, maxCharge, CharacterManager.Instance.GetMinionSettings(p_demonType).className);
        }
    }
    public void DecreaseMonsterUnderlingCharge(SUMMON_TYPE p_monsterType) {
        if (HasMonsterUnderlingEntry(p_monsterType)) {
            MonsterAndDemonUnderlingCharges m_underlingCharges = monsterUnderlingCharges[p_monsterType];
            m_underlingCharges.currentCharges--;
            if (m_underlingCharges.currentCharges < 0) {
                m_underlingCharges.currentCharges = 0;
            }
            m_underlingCharges.StartMonsterReplenish();
            Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, m_underlingCharges);
        }
    }
    public MonsterAndDemonUnderlingCharges GetSummonUnderlingChargesBySummonType(SUMMON_TYPE p_type) {
        return monsterUnderlingCharges[p_type];
    }
    public MonsterAndDemonUnderlingCharges GetMinionUnderlingChargesByMinionType(MINION_TYPE p_type) {
        return demonUnderlingCharges[p_type];
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataPlayerUnderlingsComponent data) {
        foreach (MonsterAndDemonUnderlingCharges item in monsterUnderlingCharges.Values) {
            item.LoadMonsterReplenish();
        }
    }
    #endregion
}

public class SaveDataPlayerUnderlingsComponent : SaveData<PlayerUnderlingsComponent> {
    public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges;
    public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges;

    public override void Save(PlayerUnderlingsComponent data) {
        base.Save(data);
        monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges>();
        foreach (var kvp in data.monsterUnderlingCharges) {
            monsterUnderlingCharges.Add(kvp.Key, kvp.Value);
        }
        demonUnderlingCharges = new Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges>();
        foreach (var kvp in data.demonUnderlingCharges) {
            demonUnderlingCharges.Add(kvp.Key, kvp.Value);
        }
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
    public string characterClassName;
    public bool isDemon;
    public bool isReplenishing;
    //public GameDate replenishDate;
    public int currentCooldownTick;


    public bool hasMaxCharge => maxCharges > 0;
    public int cooldown => PlayerManager.Instance.player.underlingsComponent.cooldown;

    public MonsterAndDemonUnderlingCharges(SUMMON_TYPE p_monsterType, int p_currentCharges, int p_maxCharges, string p_characterClassName) {
        monsterType = p_monsterType;
        currentCharges = p_currentCharges;
        maxCharges = p_maxCharges;
        characterClassName = p_characterClassName;
    }
    public MonsterAndDemonUnderlingCharges(MINION_TYPE p_minionType, int p_currentCharges, int p_maxCharges, string p_characterClassName) {
        minionType = p_minionType;
        currentCharges = p_currentCharges;
        maxCharges = p_maxCharges;
        characterClassName = p_characterClassName;
        isDemon = true;
    }
    
    #region Monster Replenish
    public void StartMonsterReplenish() {
        if (!isReplenishing) {
            isReplenishing = true;
            currentCooldownTick = 0;
            Messenger.AddListener(Signals.TICK_STARTED, PerTickReplenish);
            Messenger.Broadcast(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, this);
            //replenishDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
            //SchedulingManager.Instance.AddEntry(replenishDate, DoneMonsterReplenish, null);
        }
    }
    public void OnLoseMaxCharges() {
        //clamp current charges to max charges when max charges are lost
        currentCharges = Mathf.Clamp(currentCharges, 0, maxCharges);
        if (!hasMaxCharge) {
            //Related task: (Kennel Monster transfer exploit) https://trello.com/c/Bj13iHJW/4317-kennel-monster-transfer-exploit
            //lost max charges and max charges is less than or equal to 0
            //if underlings are replenishing, stop it.
            CancelMonsterReplenish();
        }
    }
    private void PerTickReplenish() {
        currentCooldownTick++;
        Messenger.Broadcast(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, this);
        if (currentCooldownTick >= cooldown) {
            Messenger.Broadcast(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, this);
            DoneMonsterReplenish();
        }
    }
    private void DoneMonsterReplenish() {
        if (isReplenishing) {
            currentCooldownTick = 0;
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickReplenish);
            isReplenishing = false;
            Messenger.Broadcast(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, this);
            ReplenishCharges();
        }
    }
    private void CancelMonsterReplenish() {
        if (isReplenishing) {
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}Cancelling monster replenish of {monsterType.ToString()} since player no longer has max charges for it.");
#endif
            currentCooldownTick = 0;
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickReplenish);
            isReplenishing = false;
            Messenger.Broadcast(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, this);
        }
    }
    private void ReplenishCharges() {
        if (PlayerManager.Instance.player.underlingsComponent.HasMonsterUnderlingEntry(monsterType)) {
            int chargesToReplenish = GetChargesToReplenishFor(monsterType);
            if (chargesToReplenish > 0) {
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(monsterType, chargesToReplenish);
                if (currentCharges < maxCharges && hasMaxCharge) {
                    StartMonsterReplenish();
                }
            }
        }
    }
    private int GetChargesToReplenishFor(SUMMON_TYPE p_monsterType) {
        int count = 0;
        for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++) {
            DemonicStructure structure = PlayerManager.Instance.player.playerSettlement.allStructures[i] as DemonicStructure;
            if (structure != null && structure.housedMonsterType == p_monsterType) {
                count++;
            }
        }
        return count;
    }
#endregion

#region Loading
    public void LoadMonsterReplenish() {
        if (isReplenishing) {
            //Messenger.Broadcast(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, this);
            Messenger.AddListener(Signals.TICK_STARTED, PerTickReplenish);
        }
    }
#endregion
}