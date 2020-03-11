using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveUtilities {

    #region Character States
    /// <summary>
    /// Convenience function to create a new Save Data instance for a character state.
    /// NOTE: This does not place the actual values of the state, it only creates an instance.
    /// </summary>
    /// <param name="characterState">The state to create the save data for.</param>
    /// <returns>New save data instance.</returns>
    public static SaveDataCharacterState CreateCharacterStateSaveDataInstance(CharacterState characterState) {
        if (characterState.characterState.HasUniqueSaveData()) {
            string suffix = typeof(SaveDataCharacterState).ToString(); //this is for convenience of renaming the class. nothing more
            string wholeTypeName = characterState.GetType().ToString() + suffix;
            System.Type systemType = System.Type.GetType(wholeTypeName);
            return System.Activator.CreateInstance(systemType) as SaveDataCharacterState;
        } else {
            return new SaveDataCharacterState();
        }
    }
    #endregion
}
