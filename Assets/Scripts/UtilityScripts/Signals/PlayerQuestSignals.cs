public static class PlayerQuestSignals {
    /// <summary>
    /// Parameters: QuestStep completedStep
    /// </summary>
    public static string QUEST_STEP_COMPLETED = "QuestStepCompleted";
    /// <summary>
    /// Parameters: QuestStep failedStep
    /// </summary>
    public static string QUEST_STEP_FAILED = "OnQuestStepFailed";
    /// <summary>
    /// Parameters: QuestStepCollection completedCollection
    /// </summary>
    public static string STEP_COLLECTION_COMPLETED = "StepCollectionCompleted";
    /// <summary>
    /// Parameters: List[Character]
    /// </summary>
    public static string ANGELS_ATTACKING_DEMONIC_STRUCTURE = "OnAngelsAttackingDemonicStructure";
    /// <summary>
    /// Parameters: Quest
    /// </summary>
    public static string QUEST_ACTIVATED = "OnQuestActivated";
    /// <summary>
    /// Parameters: Quest
    /// </summary>
    public static string QUEST_DEACTIVATED = "OnQuestDeactivated";
    /// <summary>
    /// Parameters: QuestItem
    /// </summary>
    public static string QUEST_STEP_HOVERED = "OnQuestStepHovered";
    /// <summary>
    /// Parameters: QuestItem
    /// </summary>
    public static string QUEST_STEP_HOVERED_OUT = "OnQuestStepHoveredOut";
    /// <summary>
    /// Parameters: QuestStep
    /// </summary>
    public static string QUEST_STEP_ACTIVATED = "OnQuestStepActivated";
    /// <summary>
    /// Parameters: TutorialQuest completedQuest
    /// </summary>
    public static string TUTORIAL_QUEST_COMPLETED = "TutorialQuestCompleted";
    /// <summary>
    /// Parameters (TutorialQuestCriteria)
    /// </summary>
    public static string QUEST_CRITERIA_MET = "OnTutorialQuestCriteriaMet";
    /// <summary>
    /// Parameters (TutorialQuestCriteria)
    /// </summary>
    public static string QUEST_CRITERIA_UNMET = "OnTutorialQuestCriteriaUnMet";
    public static string FINISHED_IMPORTANT_TUTORIALS = "FinishedImportantTutorials";
}