public class SkillEventDispatcher {

    #region Level Up IListener
    public interface ISkillLevelUpListener {
        void OnSkillLeveledUp(SkillData p_skillData, PlayerSkillData p_playerSkillData);
    }
    #endregion
    
    private System.Action<SkillData, PlayerSkillData> _onSkillUpgraded;

    public void SubscribeToLevelUp(ISkillLevelUpListener p_listener) {
        _onSkillUpgraded += p_listener.OnSkillLeveledUp;
    }
    public void UnsubscribeToLevelUp(ISkillLevelUpListener p_listener) {
        _onSkillUpgraded -= p_listener.OnSkillLeveledUp;
    }
    public void ExecuteLevelUpEvent(SkillData p_skillData, PlayerSkillData p_playerSkillData) {
        _onSkillUpgraded?.Invoke(p_skillData, p_playerSkillData);
    }

    #region Clean Up
    public void CleanUp() {
        _onSkillUpgraded = null;
    }
    #endregion
}
