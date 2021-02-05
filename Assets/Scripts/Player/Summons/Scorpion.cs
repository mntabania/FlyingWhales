using Inner_Maps;
using Traits;

public class Scorpion : Summon {

    public const string ClassName = "Scorpion";

    public override string raceClassName => "Scorpion";
    public override System.Type serializedData => typeof(SaveDataScorpion);

    public Character heldCharacter { get; private set; }
    public bool hasPulledForTheDay { get; private set; }
    public GameDate nextPullDate { get; private set; }

    public Scorpion() : base(SUMMON_TYPE.Scorpion, ClassName, RACE.SCORPION, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Scorpion(string className) : base(SUMMON_TYPE.Scorpion, className, RACE.SCORPION, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Scorpion(SaveDataScorpion data) : base(data) {
        hasPulledForTheDay = data.hasPulledForTheDay;
        nextPullDate = data.nextPullDate;
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Scorpion_Behaviour);
    }
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        if (data is SaveDataScorpion savedData) {
            if (!string.IsNullOrEmpty(savedData.heldCharacter)) {
                heldCharacter = CharacterManager.Instance.GetCharacterByPersistentID(savedData.heldCharacter);
            }
            if (hasPulledForTheDay) {
                SchedulingManager.Instance.AddEntry(nextPullDate, () => SetHasPulledForTheDay(false), this);
            }
        }
    }
    protected override void OnHourStarted() {
        base.OnHourStarted();
        if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 || 
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 18) {
            jobQueue.CancelAllJobs();
        }
    }
    #endregion

    public void SetHeldCharacter(Character p_character) {
        heldCharacter = p_character;
    }
    public void SetHasPulledForTheDay(bool p_state) {
        if(hasPulledForTheDay != p_state) {
            hasPulledForTheDay = p_state;
            if (hasPulledForTheDay) {
                nextPullDate = GameManager.Instance.Today().AddDays(1);
                SchedulingManager.Instance.AddEntry(nextPullDate, () => SetHasPulledForTheDay(false), this);
            }
        }
    }
}

[System.Serializable]
public class SaveDataScorpion : SaveDataSummon {
    public string heldCharacter;
    public bool hasPulledForTheDay;
    public GameDate nextPullDate;

    public override void Save(Character data) {
        base.Save(data);
        if (data is Scorpion summon) {
            if(summon.heldCharacter != null) {
                heldCharacter = summon.heldCharacter.persistentID;
            }
            hasPulledForTheDay = summon.hasPulledForTheDay;
            nextPullDate = summon.nextPullDate;
        }
    }
}