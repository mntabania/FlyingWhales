namespace Events.World_Events {
    public abstract class WorldEvent {

        public abstract void InitializeEvent();
        public abstract SaveDataWorldEvent Save();
    }

    public abstract class SaveDataWorldEvent : SaveData<WorldEvent> { }
}