namespace Goap.Unique_Action_Data {
    /// <summary>
    /// This is the base class for all action unique data.
    /// This class is mainly used for storage of unique variables per action.
    /// i.e. If a poisoned healing potion was used for curing a character, or if a poisoned food pile was used when trying to feed a character.
    /// Basically anything unique per action that we want to store, we put here.
    /// This is stored per ActualGoapNode.
    /// Naming convention is: Action Enum + UAD (i.e. CureCharacterUAD)
    /// </summary>
    public abstract class UniqueActionData {
        
        public abstract SaveDataUniqueActionData Save();
    }
    
    #region Save Data
    public abstract class SaveDataUniqueActionData : SaveData<UniqueActionData> { }
    #endregion
}

