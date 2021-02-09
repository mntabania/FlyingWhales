using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaCharacterComponent : AreaComponent {

    #region Utilities
    public List<T> GetAllCharactersInsideHex<T>() where T : Character {
        return locationCharacterTracker.GetAllCharactersInsideHex<T>();
    }
    public void PopulateCharacterListInsideHexThatMeetCriteria(List<Character> p_characterList, System.Func<Character, bool> validityChecker) {
        locationCharacterTracker.PopulateCharacterListInsideHexThatMeetCriteria(p_characterList, validityChecker);
    }

    public T GetFirstCharacterInsideHexThatMeetCriteria<T>(System.Func<Character, bool> validityChecker) where T : Character {
        return locationCharacterTracker.GetFirstCharacterInsideHexThatMeetCriteria<T>(validityChecker);
    }
    public T GetRandomCharacterInsideHexThatMeetCriteria<T>(System.Func<Character, bool> validityChecker) where T : Character {
        return locationCharacterTracker.GetRandomCharacterInsideHexThatMeetCriteria<T>(validityChecker);
    }
    public int GetNumOfCharactersInsideHexThatMeetCriteria(System.Func<Character, bool> criteria) {
        return locationCharacterTracker.GetNumOfCharactersInsideHexThatMeetCriteria(criteria);
    }
    #endregion
}
