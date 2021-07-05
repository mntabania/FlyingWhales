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
            if (p_character.partyComponent.currentParty.isActive && p_character.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Night_Patrol) {
                SetSchedule(CharacterManager.Instance.GetDailySchedule<NightPartyMemberSchedule>());
            } else {
                SetSchedule(CharacterManager.Instance.GetDailySchedule<PartyMemberSchedule>());
            }
        } else if (p_character.traitContainer.HasTrait("Nocturnal")) {
            SetSchedule(CharacterManager.Instance.GetDailySchedule<NocturnalSchedule>());
        } else {
            SetSchedule(CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>());    
        }
    }
    private void SetSchedule(DailySchedule p_schedule) {
        if (schedule != p_schedule) {
            DAILY_SCHEDULE scheduleTypeBeforeChangingSchedule = schedule.GetScheduleType(GameManager.Instance.currentTick);
            DAILY_SCHEDULE scheduleTypeAfterChangingSchedule = p_schedule.GetScheduleType(GameManager.Instance.currentTick);
            schedule = p_schedule;
            if (scheduleTypeBeforeChangingSchedule == DAILY_SCHEDULE.Sleep && scheduleTypeAfterChangingSchedule != DAILY_SCHEDULE.Sleep) {
                if (owner.currentJob != null && (owner.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || owner.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                    owner.currentJob.CancelJob("Time to wake up");
                }
            }
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
    public void OnPartyAcceptedQuest(Character p_character, PartyQuest p_quest) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
    }
    public void OnPartyEndQuest(Character p_character, PartyQuest p_quest) {
        Assert.IsTrue(p_character == owner);
        UpdateDailySchedule(p_character);
    }
    public void OnHourStarted(Character p_character) {
        GameDate previousDate = GameManager.Instance.Today();
        previousDate.ReduceTicks(1);
        DAILY_SCHEDULE previousSchedule = schedule.GetScheduleType(previousDate.tick);
        DAILY_SCHEDULE currentSchedule = schedule.GetScheduleType(GameManager.Instance.currentTick);
        if (previousSchedule == DAILY_SCHEDULE.Sleep && currentSchedule != DAILY_SCHEDULE.Sleep) {
            //wake up character
            if (p_character.currentJob != null && (p_character.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || p_character.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                p_character.currentJob.CancelJob("Time to wake up");
            }
        }
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