public class ReligionComponent : CharacterComponent {
    
    public RELIGION religion { get; private set; }
    
    public ReligionComponent() {
        religion = RELIGION.None;
    }
    public ReligionComponent(SaveDataReligionComponent data) {
        religion = data.religion;
    }

    #region Initialization
    public void Initialize() {
        SetDefaultReligion();
    }
    public void SubscribeListeners() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, OnCharacterAllianceChanged);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnCharacterFactionSet);
    }
    public void UnsubscribeListeners() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, OnCharacterAllianceChanged);
        Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnCharacterFactionSet);
    }
    #endregion

    #region Listeners
    private void OnCharacterAllianceChanged(Character character) {
        if (character == owner && character.isAlliedWithPlayer) {
            SetReligion(RELIGION.Demon_Worship);
        }
    }
    private void OnCharacterFactionSet(Character character) {
        if (character == owner && character.faction != null && character.faction.factionType.type == FACTION_TYPE.Undead) {
            SetReligion(RELIGION.None);
        }
    }
    #endregion

    #region Main
    private void SetReligion(RELIGION religion) {
        this.religion = religion;
    }
    private void SetDefaultReligion() {
        switch (owner.race) {
            case RACE.HUMANS:
                SetReligion(RELIGION.Divine_Worship);
                break;
            case RACE.ELVES:
                SetReligion(RELIGION.Nature_Worship);
                break;
            default:
                SetReligion(RELIGION.None);
                break;
        }
    }
    public static RELIGION GetDefaultReligionForRace(RACE race) {
        switch (race) {
            case RACE.HUMANS:
                return RELIGION.Divine_Worship;
            case RACE.ELVES:
                return RELIGION.Nature_Worship;
            default:
                return RELIGION.None;
        }
    }
    #endregion
    
}

public class SaveDataReligionComponent : SaveData<ReligionComponent> {

    public RELIGION religion;

    #region Overrides
    public override void Save(ReligionComponent data) {
        religion = data.religion;
    }
    public override ReligionComponent Load() {
        ReligionComponent component = new ReligionComponent(this);
        return component;
    }
    #endregion
}