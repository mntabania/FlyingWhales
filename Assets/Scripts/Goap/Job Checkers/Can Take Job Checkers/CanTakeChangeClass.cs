namespace Goap.Job_Checkers {
    public class CanTakeChangeClass : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Change_Class;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            bool canTakeJob = character.isNormalCharacter && character.classComponent.canChangeClass && character.classComponent.shouldChangeClass;
            if (canTakeJob) {
                if (jobQueueItem is GoapPlanJob goapPlanJob) {
                    OtherData[] otherData = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.CHANGE_CLASS);
                    string classToChangeTo = string.Empty;
                    for (int i = 0; i < otherData.Length; i++) {
                        OtherData data = otherData[i];
                        if (data is StringOtherData stringOtherData) {
                            classToChangeTo = stringOtherData.str;
                            break;
                        }
                    }
                    if (character.characterClass.className == classToChangeTo) {
                        //if characters class is already the target class to change to, then do not allow this character to take the job
                        return false;
                    }
                    if (!character.classComponent.HasAbleClass(classToChangeTo)) {
                        return false;
                    }
                    CharacterClass classToChangeToInstance = CharacterManager.Instance.GetCharacterClass(classToChangeTo);
                    if (classToChangeToInstance.IsCombatant() && character.characterClass.IsCombatant()) {
                        //If character is already combatant, it should not change class if the class to change to is also combatant
                        //https://trello.com/c/Ql1ACBvN/4948-change-class-to-combatant-update
                        return false;
                    }
                }
                return true;
            }
            return false;
            //bool canTakeJob = !character.characterClass.IsSpecialClass() && character.isNormalCharacter;
            //if (canTakeJob) {
            //    if (jobQueueItem.originalOwner is NPCSettlement npcSettlement) {
            //        if (npcSettlement.settlementClassTracker.neededClasses.Contains(character.characterClass.className)) {
            //            if (!npcSettlement.settlementClassTracker.HasExcessOfClass(character.characterClass.className)) {
            //                //if class of current character is needed by the settlement, and there is NOT an excess of it, then do not allow this character to take the job.
            //                return false;    
            //            }
            //        }
            //    }
            //    if (jobQueueItem is GoapPlanJob goapPlanJob) {
            //        OtherData[] otherData = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.CHANGE_CLASS);
            //        string classToChangeTo = string.Empty;
            //        for (int i = 0; i < otherData.Length; i++) {
            //            OtherData data = otherData[i];
            //            if (data is StringOtherData stringOtherData) {
            //                classToChangeTo = stringOtherData.str;
            //                break;
            //            }
            //        }
            //        if (character.characterClass.className == classToChangeTo) {
            //            //if characters class is already the target class to change to, then do not allow this character to take the job
            //            return false;
            //        }
            //    }
            //    return true;
            //}
            //return false;
        }
    }
}