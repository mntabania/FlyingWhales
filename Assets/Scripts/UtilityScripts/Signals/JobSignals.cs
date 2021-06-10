public static class JobSignals {
    public static string STARTED_PERFORMING_ACTION = "OnActionPerformed";
    public static string SCREAM_FOR_HELP = "OnScreamForHelp"; //Parameters (Character characterThatScreamed)
    public static string CHARACTER_WILL_DO_JOB = "OnCharacterRecievedPlan"; //Parameters (Character, GoapPlan)
    public static string CHARACTER_DID_ACTION_SUCCESSFULLY = "OnCharacterDidActionSuccessfully"; //Parameters (Character, ActualGoapNode)
    public static string CHARACTER_FINISHED_ACTION = "OnCharacterFinishedAction"; //Parameters (Character, GoapAction, String result)
    public static string CHARACTER_DOING_ACTION = "OnCharacterDoingAction"; //Parameters (Character, GoapAction)
    public static string AFTER_ACTION_STATE_SET = "OnAfterActionStateSet"; //Parameters (Character, GoapAction, GoapActionState)
    public static string CHECK_JOB_APPLICABILITY = "OnCheckJobApplicability"; //Parameters (JOB_TYPE, IPointOfInterest)
    public static string CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING = "OnCheckAllJobsTargetingApplicability"; //Parameters (IPointOfInterest)
    public static string CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE = "OnCheckJobApplicabilityOfAllJobsOfType"; //Parameters (JOB_TYPE)
    /// <summary>
    /// Parameters (JobQueueItem, Character)
    /// </summary>
    public static string JOB_REMOVED_FROM_QUEUE = "OnJobRemovedFromQueue";
    /// <summary>
    /// Parameters (JobQueueItem, Character)
    /// </summary>
    public static string JOB_ADDED_TO_QUEUE = "OnJobAddedToQueue";
    /// <summary>
    /// Parameters (LocationStructure, Character, GoapPlanJob)
    /// </summary>
    public static string DEMONIC_STRUCTURE_DISCOVERED = "DemonicStructureDiscovered";
    /// <summary>
    /// Parameters: Character assumer, Character target, IPointOfInterest poi
    /// </summary>
    public static string CHARACTER_ASSUMED = "OnCharacterAssumed";
    public static string JOB_REMOVED_FROM_JOB_BOARD = "OnJobRemovedFromJobBoard";

    public static string ON_FINISH_PRAYING = "OnFinishPraying";
}