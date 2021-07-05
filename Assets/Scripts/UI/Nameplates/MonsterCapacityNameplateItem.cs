using UnityEngine;

public class MonsterCapacityNameplateItem : NameplateItem<MonsterCapacity> {

    [Header("Monster Capacity Attributes")]
    [SerializeField] private CharacterPortrait classPortrait;

    private SUMMON_TYPE _summonType;

    #region getters
    public SUMMON_TYPE summonType => _summonType;
    #endregion
    
    public override void SetObject(MonsterCapacity o) {
        base.SetObject(o);
        _summonType = o.summonType;
        mainLbl.text = o.strSummonType;
        subLbl.text = $"{o.remainingCharges.ToString()}/{o.maxCapacity.ToString()}";
    }
    public void UpdateData(MonsterCapacity p_monsterCapacity) {
        mainLbl.text = p_monsterCapacity.strSummonType;
        subLbl.text = $"{p_monsterCapacity.remainingCharges.ToString()}/{p_monsterCapacity.maxCapacity.ToString()}";
        classPortrait.GeneratePortrait(CharacterManager.Instance.GeneratePortrait(p_monsterCapacity.monsterRace, GENDER.MALE, p_monsterCapacity.strMonsterClass, false));
    }
}
