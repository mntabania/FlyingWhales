public abstract class Animal : Summon {

    public bool isShearable { set; get; }

    public Animal(SUMMON_TYPE summonType, string className, RACE race) : base(summonType, className, race, UtilityScripts.Utilities.GetRandomGender()) { }
    public Animal(SaveDataSummon data) : base(data) { }

	public override void SubscribeToSignals() {
		base.SubscribeToSignals();
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
	}

	public override void UnsubscribeSignals() {
		base.UnsubscribeSignals();
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
	}

	void OnDayStarted() {
		if (!isDead) {
			isShearable = true;
		}
		
	}
}
