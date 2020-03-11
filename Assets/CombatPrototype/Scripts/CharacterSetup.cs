using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class CharacterSetup {
    public string fileName;

	public string characterClassName;
	//public string raceSettingName;
	public CHARACTER_ROLE optionalRole;
    public List<ATTRIBUTE> tags;

    private CharacterClass _charClass;
	//private RaceSetting _raceSetting;


    #region getters/setters
    public CharacterClass characterClass {
        get {
            if (_charClass == null) {
                _charClass = CharacterManager.Instance.GetCharacterClass(characterClassName);
                if(_charClass == null) {
                    throw new Exception($"There is no class with the name {characterClassName}");
                }
            }
            return _charClass;
        }
    }
	//public RaceSetting raceSetting {
//          get {
//              if (_raceSetting == null) {
//                  if (RaceManager.Instance.racesDictionary.ContainsKey(raceSettingName)) {
//                      _raceSetting = RaceManager.Instance.racesDictionary[raceSettingName];
//                  }
//                  else {
//                      throw new Exception("There is no race with the name " + raceSettingName);
//                  }
//              }
//              return _raceSetting;
//          }
//      }
    #endregion

}