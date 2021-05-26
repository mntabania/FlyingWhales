using Traits;
using UnityEngine.Assertions;

public class DailyScheduleComponent : CharacterComponent {
    public DailySchedule schedule { get; private set; }

    public DailyScheduleComponent() {
        schedule = CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>();
    }

    public void LoadReferences(SaveDataDailyScheduleComponent p_data) {
        schedule = CharacterManager.Instance.GetDailySchedule(p_data.scheduleType);
    }
    
    private void UpdateDailySchedule(Character p_character) {
        if (p_character.partyComponent.hasParty) {
            schedule = CharacterManager.Instance.GetDailySchedule<PartyMemberSchedule>();
        } else if (p_character.traitContainer.HasTrait("Nocturnal")) {
            schedule = CharacterManager.Instance.GetDailySchedule<NocturnalSchedule>();
        } else {
            schedule = CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>();    
        }
    }

    #region Listeners
    public void OnCharacterGainedNocturnal(Character p_character) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
        
    }
    public void OnCharacterLostNocturnal(Character p_character) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
        
    }
    public void OnCharacterJoinedParty(Character p_character) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
    }
    public void OnCharacterLeftParty(Character p_character) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
    }
    #endregion
    
}

#region Save Data
public class SaveDataDailyScheduleComponent : SaveData<DailyScheduleComponent> {
    public System.Type scheduleType;
    public override void Save(DailyScheduleComponent data) {
        base.Save(data);
        scheduleType = data.schedule.GetType();
    }
    public override DailyScheduleComponent Load() {
        return new DailyScheduleComponent();
    }
}
#endregion