public abstract class PassiveSkill {
    public abstract string name { get; }
    public abstract string description { get; }
    public abstract PASSIVE_SKILL passiveSkill { get; }

    public abstract void ActivateSkill();
}
