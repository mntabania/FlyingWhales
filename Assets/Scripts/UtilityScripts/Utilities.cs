using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class Utilities : MonoBehaviour {
	public static System.Random rng = new System.Random(); 
    public static int lastRegionID = 0;
	public static int lastKingdomColorIndex = 0;
    public static int lastFactionColorIndex = 0;
	public static int lastAlliancePoolID = 0;
	public static int lastWarfareID = 0;
    public static int lastLogID = 0;
    public static int lastLandmarkID = 0;
	public static int lastFactionID = 0;
    public static int lastCharacterID = 0;
    public static int lastQuestID = 0;
    public static int lastItemID = 0;
    public static int lastMonsterID = 0;
    public static float defenseBuff = 1.20f;
	public static int defaultCityHP = 300;

	public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;
    public static string dataPath {
        get {
#if UNITY_EDITOR
            return Application.dataPath + "/Resources/Data/";
#elif UNITY_STANDALONE
            return Application.streamingAssetsPath + "/Data/";
#endif
        }
    }

    public static string worldConfigsSavePath { get { return Application.persistentDataPath + "/Saves/"; } }
    public static string worldConfigFileExt { get { return ".worldConfig"; } }



    /*
	 * Set unique id
	 * */
    public static int SetID<T>(T obj){
        if(obj is Region) {
            lastRegionID += 1;
            return lastRegionID;
        } else if (obj is Log) {
            lastLogID += 1;
            return lastLogID;
		} else if (obj is BaseLandmark) {
            lastLandmarkID += 1;
            return lastLandmarkID;
		} else if (obj is Faction) {
			lastFactionID += 1;
			return lastFactionID;
        } else if(obj is ECS.Character) {
            lastCharacterID += 1;
            return lastCharacterID;
        } else if (obj is Quest) {
            lastQuestID += 1;
            return lastQuestID;
        } else if (obj is ECS.Item) {
            lastItemID += 1;
            return lastItemID;
        } else if (obj is Monster) {
            lastMonsterID += 1;
            return lastMonsterID;
        }
        return 0;
	}
    public static int SetID<T>(T obj, int idToUse) {
        if (obj is Region) {
            lastRegionID = idToUse;
        } else if (obj is Log) {
            lastLogID = idToUse;
        } else if (obj is BaseLandmark) {
            lastLandmarkID = idToUse;
        } else if (obj is Faction) {
            lastFactionID = idToUse;
        } else if (obj is ECS.Character) {
            lastCharacterID = idToUse;
        } else if (obj is Quest) {
            lastQuestID = idToUse;
        } else if (obj is ECS.Item) {
            lastItemID = idToUse;
        }
        return idToUse;
    }

    public static Color GetColorForFaction() {
        Color chosenColor = factionColorCycle[lastFactionColorIndex];
        lastFactionColorIndex += 1;
        if (lastFactionColorIndex >= factionColorCycle.Length) {
            lastFactionColorIndex = 0;
        }
        return chosenColor;
    }

	public static T[] GetEnumValues<T>() where T : struct {
		if (!typeof(T).IsEnum) {
			throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
		}
		return (T[])Enum.GetValues(typeof(T));
	}
    
    public static List<BIOMES> biomeLayering = new List<BIOMES>() {
        BIOMES.GRASSLAND,
        //BIOMES.WOODLAND,
        BIOMES.TUNDRA,
        BIOMES.FOREST,
        BIOMES.DESERT,
        BIOMES.SNOW
    };

#region Pathfinding
	public static List<Point> EvenNeighbours {
		get {
			return new List<Point> {
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 0),
				new Point(0, -1),
				new Point(-1, -1),
				new Point(-1, 0),

			};
		}
	}

	public static List<Point> OddNeighbours {
		get {
			return new List<Point> {
				new Point(0, 1),
				new Point(1, 1),
				new Point(1, 0),
				new Point(1, -1),
				new Point(0, -1),
				new Point(-1, 0),
			};
		}
	}
#endregion

	public static List<T> Shuffle<T>(List<T> list)  
	{
		List<T> newList = new List<T>(list);
		int n = newList.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = newList[k];  
			newList[k] = newList[n];  
			newList[n] = value;  
		} 
		return newList;
	}

	public static T[] GetComponentsInDirectChildren<T>(GameObject gameObject){
		int indexer = 0;

		foreach (Transform transform in gameObject.transform){
			if (transform.GetComponent<T>() != null){
				indexer++;
			}
		}

		T[] returnArray = new T[indexer];

		indexer = 0;

		foreach (Transform transform in gameObject.transform){
			if (transform.GetComponent<T>() != null){
				returnArray[indexer++] = transform.GetComponent<T>();
			}
		}

		return returnArray;
	}

	public static Color[] factionColorCycle = new Color[] {
		new Color32(0xDB, 0x00, 0x00, 0x91), // Red DB000091
		new Color32(0x00, 0x51, 0xF3, 0x91), // Blue 0051F391
		new Color32(0xFF, 0xFF, 0x00, 0x91), // Yellow FFFF00A0
		new Color32(0xFF, 0xFF, 0xFF, 0x91), // White FFFFFF91
		new Color32(0x78, 0xFF, 0x2B, 0x91), // Mint Green 78FF2B91
		new Color32(0xF9, 0x5B, 0xCD, 0x91), // Pink F95BCD91
		new Color32(0x1D, 0x1D, 0x1D, 0x91), // Black 1D1D1D91
		new Color32(0x0F, 0xDD, 0xF0, 0xA0), // Cyan 0FDDF0A0
		new Color32(0xFF, 0x8E, 0x00, 0xA0), // Orange FF8E00A3
		new Color32(0x8D, 0x12, 0xCE, 0x91), // Violet 8D12CE91
		new Color32(0x0E, 0x77, 0x1B, 0x91), // Dark Green 0E771B91
		new Color32(0x8A, 0x07, 0x07, 0x94), // Dark Red 8A070794
		new Color32(0x03, 0x21, 0x8E, 0xA0), // Dark Blue 03218EA0
		new Color32(0xA6, 0x56, 0x00, 0xB9), // Brown A65600B9 
		new Color32(0x8A, 0x80, 0xFD, 0x94), // Light Violet 8A80FD91
		new Color32(0xBC, 0xFF, 0x00, 0xA6) // Yellow Green BCFF00A6
	};

    public static Dictionary<BIOMES, Color> biomeColor = new Dictionary<BIOMES, Color>() {
        {BIOMES.GRASSLAND, new Color(34f/255f, 139f/255f, 34f/255f)},
        {BIOMES.BARE, new Color(106f/255f, 108f/255f, 59f/255f)},
        {BIOMES.DESERT, new Color(93f/255f, 79f/255f, 69f/255f)},
        {BIOMES.FOREST, new Color(34f/255f, 139f/255f, 34f/255f)},
        {BIOMES.SNOW, new Color(255f/255f, 255f/255f, 255f/255f)},
        {BIOMES.TUNDRA, new Color(106f/255f, 108f/255f, 59f/255f)},
        //{BIOMES.WOODLAND, new Color(34f/255f, 139f/255f, 34f/255f)}
    };

    public static WeightedDictionary<MATERIAL> GetMaterialWeights() {
        WeightedDictionary<MATERIAL> materialWeights = new WeightedDictionary<MATERIAL>();
        MATERIAL[] allMaterials = GetEnumValues<MATERIAL>();
        for (int i = 0; i < allMaterials.Length; i++) {
            MATERIAL currMat = allMaterials[i];
            if (currMat != MATERIAL.NONE) {
                materialWeights.AddElement(currMat, MaterialManager.Instance.materialsLookup[currMat].weight);
            }
        }
        return materialWeights;
    }
    //	public static string StringReplacer(string text, LogFiller[] logFillers){
    //		List<int> specificWordIndexes = new List<int> ();
    //		string newText = text;
    //		bool hasPeriod = newText.EndsWith (".");
    //		if (!string.IsNullOrEmpty (newText)) {
    //			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ','});
    //			for (int i = 0; i < words.Length; i++) {
    //				if (words [i].Contains ("(%")) {
    //					specificWordIndexes.Add (i);
    //				}else if(words [i].Contains ("(*")){
    //					string strIndex = Utilities.GetStringBetweenTwoChars (words [i], '-', '-');
    //					int index = 0;
    //					bool isIndex = int.TryParse (strIndex, out index);
    //					if(isIndex){
    //						words [i] = Utilities.PronounReplacer (words [i], logFillers [index].obj);
    //					}
    //				}
    //			}
    //			if(specificWordIndexes.Count == logFillers.Length){
    //				for (int i = 0; i < logFillers.Length; i++) {
    //					string replacedWord = Utilities.CustomStringReplacer (words [specificWordIndexes [i]], logFillers [i], i);
    //					if(!string.IsNullOrEmpty(replacedWord)){
    //						words [specificWordIndexes [i]] = replacedWord;
    //					}
    //				}
    //			}
    //			newText = string.Empty;
    //			for (int i = 0; i < words.Length; i++) {
    //				newText += words [i];
    //			}
    //			newText = newText.Trim (' ');
    //		}
    //
    //		return newText;
    //	}
    //	public static string LogReplacer(Log log){
    //		List<int> specificWordIndexes = new List<int> ();
    //		string newText = LocalizationManager.Instance.GetLocalizedValue (log.category, log.file, log.key);
    //		bool hasPeriod = newText.EndsWith (".");
    //		if (!string.IsNullOrEmpty (newText)) {
    //			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ','});
    //			for (int i = 0; i < words.Length; i++) {
    //				if (words [i].Contains ("(%")) {
    //					specificWordIndexes.Add (i);
    //				}else if(words [i].Contains ("(*")){
    //					string strIndex = Utilities.GetStringBetweenTwoChars (words [i], '-', '-');
    //					int index = 0;
    //					bool isIndex = int.TryParse (strIndex, out index);
    //					if(isIndex){
    //						words [i] = Utilities.PronounReplacer (words [i], log.fillers [index].obj);
    //					}
    //				}
    //			}
    //			if(specificWordIndexes.Count == log.fillers.Count){
    //				for (int i = 0; i < log.fillers.Count; i++) {
    //					string replacedWord = Utilities.CustomStringReplacer (words [specificWordIndexes [i]], log.fillers [i], i);
    //					if(!string.IsNullOrEmpty(replacedWord)){
    //						words [specificWordIndexes [i]] = replacedWord;
    //					}
    //				}
    //			}
    //			newText = string.Empty;
    //			for (int i = 0; i < words.Length; i++) {
    //				newText += words [i];
    //			}
    //			newText = newText.Trim (' ');
    //		}
    //
    //		return newText;
    //	}
    public static string LogReplacer(Log log){
		if(log == null){
			return string.Empty;
		}
		string replacedWord = string.Empty;
		List<int> specificWordIndexes = new List<int> ();
		string newText = LocalizationManager.Instance.GetLocalizedValue (log.category, log.file, log.key);
		bool hasPeriod = newText.EndsWith (".");

		if (!string.IsNullOrEmpty (newText)) {
			string[] words = Utilities.SplitAndKeepDelimiters(newText, new char[]{' ', '.', ',', '\'', '!'});
			for (int i = 0; i < words.Length; i++) {
				replacedWord = string.Empty;
				if (words [i].StartsWith ("%") && (words[i].EndsWith("%") || words[i].EndsWith("@"))) { //OBJECT
					replacedWord = Utilities.CustomStringReplacer (words[i], ref log.fillers);
				}else if(words [i].StartsWith ("%") && (words[i].EndsWith("a") || words[i].EndsWith("b"))){ //PRONOUN
					replacedWord = Utilities.CustomPronounReplacer (words[i], log.fillers);
				}
				if(!string.IsNullOrEmpty(replacedWord)){
					words[i] = replacedWord;
				}
			}
			newText = string.Empty;
			for (int i = 0; i < words.Length; i++) {
				newText += words[i];
			}
			newText = newText.Trim (' ');
		}

		return newText;
	}
	public static string CustomPronounReplacer(string wordToBeReplaced, List<LogFiller> objectLog){
		LOG_IDENTIFIER identifier = Utilities.logIdentifiers [wordToBeReplaced.Substring(1, 2)];
		string wordToReplace = string.Empty;
//		string value = wordToBeReplaced.Substring(1, 2);
		string strIdentifier = identifier.ToString ();
		string pronouns = Utilities.GetPronoun(strIdentifier.Last (), wordToBeReplaced.Last());

		LOG_IDENTIFIER logIdentifier = LOG_IDENTIFIER.ACTIVE_CHARACTER;
		if(strIdentifier.Contains("FACTION_LEADER_1")){
			logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_1;
		}else if(strIdentifier.Contains("FACTION_LEADER_2")){
			logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_2;
		}else if(strIdentifier.Contains("TARGET_CHARACTER")){
			logIdentifier = LOG_IDENTIFIER.TARGET_CHARACTER;
		}else if(strIdentifier.Contains("FACTION_LEADER_3")){
			logIdentifier = LOG_IDENTIFIER.FACTION_LEADER_3;
		}
		for(int i = 0; i < objectLog.Count; i++){
			if(objectLog[i].identifier == logIdentifier){
				wordToReplace = Utilities.PronounReplacer (pronouns, objectLog [i].obj);
				break;
			}
		}

		return wordToReplace;

	}
	public static string CustomStringReplacer(string wordToBeReplaced, ref List<LogFiller> objectLog){
		string wordToReplace = string.Empty;
		string strLogIdentifier = wordToBeReplaced.Remove(0,1);
		strLogIdentifier = strLogIdentifier.Remove((strLogIdentifier.Length - 1), 1);
		LOG_IDENTIFIER identifier = Utilities.logIdentifiers[strLogIdentifier];
		if(wordToBeReplaced.EndsWith("@")){
			for(int i = 0; i < objectLog.Count; i++){
				if(objectLog[i].identifier == identifier){
					//if (objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_1 || objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_2){
					//	if(objectLog [i].obj is Kingdom){
					//		Kingdom kingdom = (Kingdom)objectLog [i].obj;
					//		Citizen randomGovernor = kingdom.GetRandomGovernorFromKingdom ();
					//		objectLog [i] = new LogFiller(randomGovernor, randomGovernor.name, objectLog[i].identifier);
					//	}
					//}
					wordToReplace = "<link=" + '"' + i.ToString() + '"' + "><b>" + objectLog[i].value + "</b></link>";
					break;
				}
			}
		}else if(wordToBeReplaced.EndsWith("%")){
			for(int i = 0; i < objectLog.Count; i++){
				if(objectLog[i].identifier == identifier){
					//if (objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_1 || objectLog[i].identifier == LOG_IDENTIFIER.RANDOM_GOVERNOR_2){
					//	if(objectLog [i].obj is Kingdom){
					//		Kingdom kingdom = (Kingdom)objectLog [i].obj;
					//		Citizen randomGovernor = kingdom.GetRandomGovernorFromKingdom ();
					//		objectLog [i] = new LogFiller(randomGovernor, randomGovernor.name, objectLog[i].identifier);
					//	}
					//}
					wordToReplace = objectLog[i].value;
					break;
				}
			}
		}

		return wordToReplace;

	}
    //	public static string CustomStringReplacer(string wordToBeReplaced, LogFiller objectLog, int index){
    //		string wordToReplace = string.Empty;
    //		string value = string.Empty;
    //
    //		if(wordToBeReplaced.Contains("@")){
    //			wordToReplace = "[url=" + index.ToString() + "][b]" + objectLog.value + "[/b][/url]";
    //		}else{
    //			wordToReplace = objectLog.value;
    //		}
    //
    //		return wordToReplace;
    //
    //	}

    public static Color darkGreen = new Color(0f / 255f, 100f / 255f, 0f / 255f);
    public static Color lightGreen = new Color(124f / 255f, 252f / 255f, 0f / 255f);
    public static Color darkRed = new Color(139f / 255f, 0f / 255f, 0f / 255f);
    public static Color lightRed = new Color(255f / 255f, 0f / 255f, 0f / 255f);

    public static Color GetColorForTrait(TRAIT trait) {
        switch (trait) {
            case TRAIT.CHARISMATIC:
                return darkGreen;
            case TRAIT.REPULSIVE:
                return darkRed;
            case TRAIT.SMART:
                return darkGreen;
            case TRAIT.DUMB:
                return darkRed;
            case TRAIT.EFFICIENT:
                return darkGreen;
            case TRAIT.INEPT:
                return darkRed;
            case TRAIT.HOSTILE:
                return darkRed;
            //case TRAIT.MILITANT:
            //    return lightRed;
            case TRAIT.PACIFIST:
                return darkGreen;
            default:
                return Color.white;
        }
    }

	public static string GetPronoun(string type, string caseIdentifier){
		if(type == "S"){
			if(caseIdentifier == "a"){
				return "He/She";
			}
			return "he/she";
		}else if(type == "O"){
			if(caseIdentifier == "a"){
				return "Him/Her";
			}
			return "him/her";
		}else if(type == "P"){
			if(caseIdentifier == "a"){
				return "His/Her";
			}
			return "his/her";
		}else if(type == "R"){
			if(caseIdentifier == "a"){
				return "Himself/Herself";
			}
			return "himself/herself";
		}
		return string.Empty;
	}
	public static string GetPronoun(char type, char caseIdentifier){
		if(type == 'S'){
			if(caseIdentifier == 'a'){
				return "He/She";
			}
			return "he/she";
		}else if(type == 'O'){
			if(caseIdentifier == 'a'){
				return "Him/Her";
			}
			return "him/her";
		}else if(type == 'P'){
			if(caseIdentifier == 'a'){
				return "His/Her";
			}
			return "his/her";
		}else if(type == 'R'){
			if(caseIdentifier == 'a'){
				return "Himself/Herself";
			}
			return "himself/herself";
		}
		return string.Empty;
	}
	public static Dictionary<string, LOG_IDENTIFIER> logIdentifiers = new Dictionary<string, LOG_IDENTIFIER> () {
		{"00", LOG_IDENTIFIER.ACTIVE_CHARACTER},
		{"01", LOG_IDENTIFIER.FACTION_1},
		{"02", LOG_IDENTIFIER.FACTION_LEADER_1},
		{"04", LOG_IDENTIFIER.LANDMARK_1},
        {"05", LOG_IDENTIFIER.PARTY_1},
		//{"06", LOG_IDENTIFIER.RANDOM_CITY_1},
		//{"07", LOG_IDENTIFIER.RANDOM_GOVERNOR_1},
		{"10", LOG_IDENTIFIER.TARGET_CHARACTER},
		{"11", LOG_IDENTIFIER.FACTION_2},
		{"12", LOG_IDENTIFIER.FACTION_LEADER_2},
		//{"13", LOG_IDENTIFIER.KING_2_SPOUSE},
		{"14", LOG_IDENTIFIER.LANDMARK_2},
        {"15", LOG_IDENTIFIER.PARTY_2},
		//{"16", LOG_IDENTIFIER.RANDOM_CITY_2},
		//{"17", LOG_IDENTIFIER.RANDOM_GOVERNOR_2},
		{"20", LOG_IDENTIFIER.CHARACTER_3},
		{"21", LOG_IDENTIFIER.FACTION_3},
		{"22", LOG_IDENTIFIER.FACTION_LEADER_3},
		//{"23", LOG_IDENTIFIER.KING_3_SPOUSE},
		{"24", LOG_IDENTIFIER.LANDMARK_3},
        {"25", LOG_IDENTIFIER.PARTY_3},
		//{"26", LOG_IDENTIFIER.RANDOM_CITY_3},
		//{"27", LOG_IDENTIFIER.RANDOM_GOVERNOR_3},
		{"81", LOG_IDENTIFIER.ACTION_DESCRIPTION},
        {"82", LOG_IDENTIFIER.QUEST_NAME},
		{"83", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S},
		{"84", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O},
		{"85", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P},
		{"86", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R},
		{"87", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S},
		{"88", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O},
		{"89", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P},
		{"90", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R},
		{"91", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S},
		{"92", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O},
		{"93", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P},
		{"94", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R},
		{"95", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S},
		{"96", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O},
		{"97", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P},
		{"98", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R},
		//{"99", LOG_IDENTIFIER.SECESSION_CITIES},
		{"100", LOG_IDENTIFIER.TASK},
		{"101", LOG_IDENTIFIER.DATE},
		{"102", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S},
		{"103", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O},
		{"104", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P},
		{"105", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R},
		{"106", LOG_IDENTIFIER.OTHER},
        {"107", LOG_IDENTIFIER.ITEM_1},
        {"108", LOG_IDENTIFIER.ITEM_2},
        {"109", LOG_IDENTIFIER.ITEM_3},
        {"110", LOG_IDENTIFIER.COMBAT},
		//{"111", LOG_IDENTIFIER.PARTY_NAME},
	};
    
	public static string PronounReplacer(string word, object genderSubject){
//		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
		string[] pronouns = word.Split ('/');
        if (genderSubject is ECS.Character) {
            GENDER gender = ((ECS.Character)genderSubject).gender;
            if (gender == GENDER.MALE) {
                if (pronouns.Length > 0) {
                    if (!string.IsNullOrEmpty(pronouns[0])) {
                        return pronouns[0];
                    }
                }
            } else {
                if (pronouns.Length > 1) {
                    if (!string.IsNullOrEmpty(pronouns[0])) {
                        return pronouns[1];
                    }
                }
            }
        }
        return string.Empty;
	}
//	public static string PronounReplacer(string word, object genderSubject){
//		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
//		string[] pronouns = pronoun.Split ('/');
//
//		if(genderSubject is Citizen){
//			GENDER gender = ((Citizen)genderSubject).gender;
//			if(gender == GENDER.MALE){
//				if(pronouns.Length > 0){
//					if(!string.IsNullOrEmpty(pronouns[0])){
//						return pronouns [0];
//					}
//				}
//			}else{
//				if (pronouns.Length > 1) {
//					if (!string.IsNullOrEmpty (pronouns [0])) {
//						return pronouns [1];
//					}
//				}
//			}
//
//
//		}
//		return string.Empty;
//	}
	public static string GetStringBetweenTwoChars (string word, char first, char last){
		int indexFirst = word.IndexOf (first);
		int indexLast = word.LastIndexOf (last);

		if(indexFirst == -1 || indexLast == -1){
			return string.Empty;
		}
		indexFirst += 1;
		if(indexFirst >= word.Length){
			return string.Empty;
		}

		return word.Substring (indexFirst, (indexLast - indexFirst));
	}
	public static List<string> GetAllWordsInAString(string wordToFind, string text){
		List<string> words = new List<string> ();
		string word = string.Empty;
		int index = 0;
		int wordCount = 0;
		int startingIndex = index;
		while(index != -1){
			index = text.IndexOf (wordToFind, startingIndex);
			if(index != -1){
				startingIndex = index + 1;
				if(startingIndex > text.Length - 1){
					startingIndex = text.Length - 1;
				}

				wordCount = 0;
				for(int i = index; i < text.Length; i++){
					if(text[i] != ' '){
						wordCount += 1;
					}else{
						break;
					}
				}
				word = text.Substring (index, wordCount);
				words.Add (word);
			}

		}
		return words;
	}
	public static string[] SplitAndKeepDelimiters(string s, params char[] delimiters){
		var parts = new List<string>();
		if (!string.IsNullOrEmpty(s))
		{
			int iFirst = 0;
			do
			{
				int iLast = s.IndexOfAny(delimiters, iFirst);
				if (iLast >= 0)
				{
					if (iLast > iFirst)
						parts.Add(s.Substring(iFirst, iLast - iFirst)); //part before the delimiter
					parts.Add(new string(s[iLast], 1));//the delimiter
					iFirst = iLast + 1;
					continue;
				}

				//No delimiters were found, but at least one character remains. Add the rest and stop.
				parts.Add(s.Substring(iFirst, s.Length - iFirst));
				break;

			} while (iFirst < s.Length);
		}

		return parts.ToArray();
	}

	public static void SetSpriteSortingLayer(SpriteRenderer sprite, string layerName){
		sprite.sortingLayerName = layerName;
	}

    public static void SetLayerRecursively(GameObject go, int layerNumber) {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layerNumber;
        }
    }

    public static string FirstLetterToUpperCase(string s) {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("There is no first letter");

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    public static string NormalizeString(string s) {
        s = s.ToLower();
        string[] words = s.Split('_');
        string normalizedString = Utilities.FirstLetterToUpperCase(words.First());
        for (int i = 1; i < words.Length; i++) {
            normalizedString += " " + words[i];
        }
        return normalizedString;
    }
	public static string NormalizeStringUpperCaseFirstLetters(string s) {
		s = s.ToLower();
		string[] words = s.Split('_');
		string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
		for (int i = 1; i < words.Length; i++) {
			normalizedString += " " + Utilities.FirstLetterToUpperCase(words[i]);
		}
		return normalizedString;
	}

    public static bool IsCurrentDayMultipleOf(int multiple) {
        if ((GameManager.Instance.days % multiple) == 0) {
            return true;
        }
        return false;
    }

    public static void ListRemoveRange<T>(List<T> sourceList, List<T> itemsToRemove) {
        for (int i = 0; i < itemsToRemove.Count; i++) {
            T currItem = itemsToRemove[i];
            sourceList.Remove(currItem);
        }
    }

    public static List<HexTile> MergeHexLists(List<HexTile> list1, List<HexTile> list2) {
        Dictionary<int, HexTile> dict = list2.ToDictionary(h => h.id, v => v);
        foreach (HexTile h in list1) {
            dict[h.id] = h;
        }
        return dict.Values.ToList();
    }

    public static Dictionary<WEIGHTED_ACTION, int> MergeWeightedActionDictionaries(Dictionary<WEIGHTED_ACTION, int> dict1, Dictionary<WEIGHTED_ACTION, int> dict2) {
        Dictionary<WEIGHTED_ACTION, int> mergedDict = new Dictionary<WEIGHTED_ACTION, int>();
        foreach (KeyValuePair<WEIGHTED_ACTION, int> kvp in dict1) {
            WEIGHTED_ACTION currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict2.ContainsKey(currKey)) {
                currValue += dict2[currKey];
            }
            mergedDict.Add(currKey, currValue);
        }
        foreach (KeyValuePair<WEIGHTED_ACTION, int> kvp in dict2) {
            WEIGHTED_ACTION currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict1.ContainsKey(currKey)) {
                currValue += dict1[currKey];
            }
            if (!mergedDict.ContainsKey(currKey)) {
                mergedDict.Add(currKey, currValue);
            }
            
        }
        return mergedDict;
    }

    public static Dictionary<T, int> MergeWeightedActionDictionaries<T>(Dictionary<T, int> dict1, Dictionary<T, int> dict2) {
        Dictionary<T, int> mergedDict = new Dictionary<T, int>();
        foreach (KeyValuePair<T, int> kvp in dict1) {
            T currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict2.ContainsKey(currKey)) {
                currValue += dict2[currKey];
            }
            mergedDict.Add(currKey, currValue);
        }
        foreach (KeyValuePair<T, int> kvp in dict2) {
            T currKey = kvp.Key;
            int currValue = kvp.Value;
            if (dict1.ContainsKey(currKey)) {
                currValue += dict1[currKey];
            }
            if (!mergedDict.ContainsKey(currKey)) {
                mergedDict.Add(currKey, currValue);
            }

        }
        return mergedDict;
    }

    public static Dictionary<T, Dictionary<T, int>> MergeWeightedActionDictionaries<T>(Dictionary<T, Dictionary<T, int>> dict1, Dictionary<T, Dictionary<T, int>> dict2) {
        Dictionary<T, Dictionary<T, int>> mergedDict = new Dictionary<T, Dictionary<T, int>>();
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict1) {
            T currKey = kvp.Key;
            Dictionary<T, int> currValue = kvp.Value;
            if (dict2.ContainsKey(currKey)) {
                currValue = MergeWeightedActionDictionaries<T>(currValue, dict2[currKey]);
            }
            mergedDict.Add(currKey, currValue);
        }
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict2) {
            T currKey = kvp.Key;
            Dictionary<T, int> currValue = kvp.Value;
            if (!mergedDict.ContainsKey(currKey)) {
                if (dict1.ContainsKey(currKey)) {
                    currValue = MergeWeightedActionDictionaries<T>(currValue, dict1[currKey]);
                }
                mergedDict.Add(currKey, currValue);
            }
        }
        return mergedDict;
    }

    public static T PickRandomElementWithWeights<T>(Dictionary<T, int> weights) {
        int totalOfAllWeights = GetTotalOfWeights(weights);
		int chance = rng.Next(0, totalOfAllWeights);
        int upperBound = 0;
        int lowerBound = 0;
        foreach (KeyValuePair<T, int> kvp in weights) {
            T currElementType = kvp.Key;
            int weightOfCurrElement = kvp.Value;
            if(weightOfCurrElement <= 0) {
                continue;
            }
            upperBound += weightOfCurrElement;
            if(chance >= lowerBound && chance < upperBound) {
                return currElementType;
            }
            lowerBound = upperBound;
        }
        throw new Exception("Could not pick element in weights");
    }

    /*
     * This will return an array that has 2 elements 
     * of the same type. The 1st element is the first key in the dictionary, and the
     * 2nd element is the second key.
     * */
    public static T[] PickRandomElementWithWeights<T>(Dictionary<T, Dictionary<T,int>> weights) {
        int totalOfAllWeights = GetTotalOfWeights(weights);
        int chance = UnityEngine.Random.Range(0, totalOfAllWeights);
        int upperBound = 0;
        int lowerBound = 0;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            T currElementType = kvp.Key;
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                T otherElement = pair.Key;
                int weightOfOtherElement = pair.Value;
                upperBound += weightOfOtherElement;
                if (chance >= lowerBound && chance < upperBound) {
                    return new T[] { currElementType, otherElement };
                }
                lowerBound = upperBound;
            }
        }
        throw new Exception("Could not pick element in weights");
    }

    public static int GetTotalOfWeights<T>(Dictionary<T, Dictionary<T, int>> weights) {
        int totalOfAllWeights = 0;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                totalOfAllWeights += pair.Value;
            }
        }
        return totalOfAllWeights;
    }

    public static int GetTotalOfWeights<T>(Dictionary<T, int> weights) {
        return weights.Sum(x => x.Value);
    }

    public static string GetWeightsSummary<T>(Dictionary<T, int> weights, string title = "Weights Summary: ") {
        string actionWeightsSummary = title;
        foreach (KeyValuePair<T, int> kvp in weights) {
            T key = kvp.Key;
            int value = kvp.Value;
            //if(key is Kingdom) {
            //    actionWeightsSummary += "\n" + ((Kingdom)((object)key)).name + " - " + kvp.Value.ToString();
            //} else if(key is AlliancePool) {
            //    actionWeightsSummary += "\n" + ((AlliancePool)((object)key)).name + " - " + kvp.Value.ToString();
            //} else 
            if (key is ECS.Character) {
                actionWeightsSummary += "\n" + (key as ECS.Character).name + " - " + kvp.Value.ToString();
            } else if (key is BaseLandmark) {
                actionWeightsSummary += "\n" + (key as BaseLandmark).landmarkName + " - " + kvp.Value.ToString();
            } else {
                actionWeightsSummary += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
            }
            
        }
        return actionWeightsSummary;
    }

    public static string GetWeightsSummary<T>(Dictionary<T, Dictionary<T, int> >weights, string title = "Weights Summary: ") {
        string actionWeightsSummary = title;
        foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
            actionWeightsSummary += "\n" + kvp.Key.ToString() + " : ";
            foreach (KeyValuePair<T, int> pair in kvp.Value) {
                actionWeightsSummary += "\n     " + pair.Key.ToString() + " - " + pair.Value.ToString();
            }
        }
        return actionWeightsSummary;
    }

    //public static string GetWeightsSummary(Dictionary<Kingdom, int> weights, string title = "Weights Summary: ") {
    //    string actionWeightsSummary = title;
    //    foreach (KeyValuePair<Kingdom, int> kvp in weights) {
    //        actionWeightsSummary += "\n" + kvp.Key.name + " - " + kvp.Value.ToString();
    //    }
    //    return actionWeightsSummary;
    //}

    //public static string GetWeightsSummary(Dictionary<Kingdom, Dictionary<Kingdom, int>> weights, string title = "Weights Summary: ") {
    //    string actionWeightsSummary = title;
    //    foreach (KeyValuePair<Kingdom, Dictionary<Kingdom, int>> kvp in weights) {
    //        actionWeightsSummary += "\n" + kvp.Key.name + " : ";
    //        foreach (KeyValuePair<Kingdom, int> pair in kvp.Value) {
    //            actionWeightsSummary += "\n     " + pair.Key.name + " - " + pair.Value.ToString();
    //        }
    //    }
    //    return actionWeightsSummary;
    //}

    public static string GetDateString(GameDate date) {
        return NormalizeString(((MONTH)date.month).ToString()) + " " + date.day.ToString() + ", " + date.year.ToString();
    }

    /*
     * <summary>
     * Get a random integer given a minimum and maximum range and a minimum and maximum 
     * mean to simulate a bell curve.
     * </summary>
     * <param name="min"> The distributions minimum value [inclusive]</param>
     * <param name="max"> The distributions maximum value [inclusive]</param>
     * <param name="minMean"> The distributions minimum mean value [minimum value for bell curve]</param>
     * <param name="maxMean"> The distributions maximum mean value [maximum value for bell curve]</param>
     * */
    public static int BellCurveRandomRange(int min, int max, int minMean, int maxMean) {
        float rand = UnityEngine.Random.value;

        if (rand <= .3f)
            return UnityEngine.Random.Range(min, minMean);
        if (rand <= .8f)
            return UnityEngine.Random.Range(minMean, maxMean);

        return UnityEngine.Random.Range(maxMean, max);
    }

    public static List<T> Intersect<T> (List<T> firstList, List<T> secondList){
		List<T> newList = new List<T> ();
		for (int i = 0; i < firstList.Count; i++) {
			for (int j = 0; j < secondList.Count; j++) {
				if (firstList[i].Equals(secondList[j])) {
					newList.Add (firstList [i]);
					break;
				}
			}
		}
		return newList;
	}
	public static List<T> Union<T> (List<T> firstList, List<T> secondList){
		bool hasMatched = false;
		List<T> newList = new List<T> ();
		for (int i = 0; i < firstList.Count; i++) {
			newList.Add (firstList [i]);
		}
		for (int i = 0; i < secondList.Count; i++) {
			hasMatched = false;
			for (int j = 0; j < firstList.Count; j++) {
				if (secondList [i].Equals(firstList [j])) {
					hasMatched = true;
					break;
				}
			}
			if(!hasMatched){
				newList.Add (secondList [i]);
			}
		}
		return newList;
	}

    public static GENDER GetRandomGender() {
        if(UnityEngine.Random.Range(0, 2) == 0) {
            return GENDER.MALE;
        }
        return GENDER.FEMALE;
    }

#region Resources
    public static MATERIAL ConvertLandmarkTypeToMaterial(LANDMARK_TYPE landmarkType) {
        try {
            MATERIAL mat = (MATERIAL) System.Enum.Parse(typeof(MATERIAL), landmarkType.ToString(), true);
            return mat;
        } catch {
            return MATERIAL.NONE;
        }
    }
    public static LANDMARK_TYPE ConvertMaterialToLandmarkType(MATERIAL material) {
        try {
            LANDMARK_TYPE landmarkType = (LANDMARK_TYPE)System.Enum.Parse(typeof(LANDMARK_TYPE), material.ToString(), true);
            return landmarkType;
        } catch {
            throw new Exception("THERE IS NO LANDMARK TYPE FOR MATERIAL " + material.ToString());
        }
    }
    public static RESOURCE GetResourceTypeByObjectType(SPECIFIC_OBJECT_TYPE objectType) {
        //switch (objectType) {
        //    case SPECIFIC_OBJECT_TYPE.ELVEN_RESIDENCES:
        //    return RESOURCE.ELF_CIVILIAN;
        //    case SPECIFIC_OBJECT_TYPE.HUMAN_RESIDENCES:
        //    return RESOURCE.HUMAN_CIVILIAN;
        //    case SPECIFIC_OBJECT_TYPE.OAK_WOODS:
        //    return RESOURCE.OAK;
        //    case SPECIFIC_OBJECT_TYPE.IRON_MINE:
        //    return RESOURCE.IRON;
        //}
        return RESOURCE.NONE;
    }
#endregion

#region Landmarks
   // public static BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
   //     switch (landmarkType) {
   //         case LANDMARK_TYPE.CLAY:
   //         case LANDMARK_TYPE.LIMESTONE:
   //         case LANDMARK_TYPE.GRANITE:
   //         case LANDMARK_TYPE.MARBLE:
   //         case LANDMARK_TYPE.SILK:
   //         case LANDMARK_TYPE.COTTON:
   //         case LANDMARK_TYPE.FLAX:
   //         case LANDMARK_TYPE.CORN:
   //         case LANDMARK_TYPE.RICE:
   //         case LANDMARK_TYPE.PIGMEAT:
   //         case LANDMARK_TYPE.COWMEAT:
   //         case LANDMARK_TYPE.GOATHIDE:
   //         case LANDMARK_TYPE.DEERHIDE:
   //         case LANDMARK_TYPE.BEHEMOTHHIDE:
   //         case LANDMARK_TYPE.OAK:
   //         case LANDMARK_TYPE.YEW:
   //         case LANDMARK_TYPE.EBONY:
   //         case LANDMARK_TYPE.IRON:
   //         case LANDMARK_TYPE.COBALT:
   //         case LANDMARK_TYPE.MITHRIL:
   //             return BASE_LANDMARK_TYPE.RESOURCE;
   //         case LANDMARK_TYPE.ANCIENT_RUIN:
   //         case LANDMARK_TYPE.VAMPIRE_TOMB:
   //         case LANDMARK_TYPE.ANCIENT_REACTOR:
   //         case LANDMARK_TYPE.CAVE:
   //         case LANDMARK_TYPE.WILDLANDS:
			//case LANDMARK_TYPE.RITUAL_STONES:
   //             return BASE_LANDMARK_TYPE.DUNGEON;
			//case LANDMARK_TYPE.CITY:
			//case LANDMARK_TYPE.GOBLIN_CAMP:
   //         case LANDMARK_TYPE.HUT:
			//case LANDMARK_TYPE.CRATER:
   //             return BASE_LANDMARK_TYPE.SETTLEMENT;
   //         default:
   //             return BASE_LANDMARK_TYPE.NONE;
   //     }
   // }
#endregion

    public static bool DoesFileExist(string path) {
        return System.IO.File.Exists(path);
    }

#region Combat Prototype
    public static ECS.IBodyPart.ATTRIBUTE GetNeededAttributeForArmor(ECS.Armor armor) {
        switch (armor.armorBodyType) {
            case "Head":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_HEAD_ARMOR;
            case "Torso":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_TORSO_ARMOR;
            case "Tail":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_TAIL_ARMOR;
            case "Arm":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_ARM_ARMOR;
            case "Hand":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_HAND_ARMOR;
            case "Leg":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_LEG_ARMOR;
			case "Hip":
				return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_HIP_ARMOR;
            case "Feet":
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_FOOT_ARMOR;
            default:
                return ECS.IBodyPart.ATTRIBUTE.CAN_EQUIP_TORSO_ARMOR;
        }
    }
#endregion

	public static MATERIAL_CATEGORY GetMaterialCategory(MATERIAL material){
		if(material == MATERIAL.IRON || material == MATERIAL.COBALT || material == MATERIAL.MITHRIL){
			return MATERIAL_CATEGORY.METAL;
		}else if(material == MATERIAL.OAK || material == MATERIAL.YEW || material == MATERIAL.EBONY){
			return MATERIAL_CATEGORY.WOOD;
		}else if(material == MATERIAL.CLAY || material == MATERIAL.LIMESTONE || material == MATERIAL.MARBLE || material == MATERIAL.GRANITE){
			return MATERIAL_CATEGORY.STONE;
		}else if(material == MATERIAL.SILK || material == MATERIAL.COTTON || material == MATERIAL.FLAX){
			return MATERIAL_CATEGORY.CLOTH;
		}else if(material == MATERIAL.CORN || material == MATERIAL.RICE){
			return MATERIAL_CATEGORY.PLANT;
		}else if(material == MATERIAL.PIGMEAT || material == MATERIAL.COWMEAT){
			return MATERIAL_CATEGORY.MEAT;
		}else if(material == MATERIAL.GOATHIDE || material == MATERIAL.DEERHIDE || material == MATERIAL.BEHEMOTHHIDE){
			return MATERIAL_CATEGORY.LEATHER;
		}
		return MATERIAL_CATEGORY.NONE;
	}

#region Characters
    public static TECHNOLOGY GetTechnologyForCharacterClass(CHARACTER_CLASS charClass) {
        switch (charClass) {
            case CHARACTER_CLASS.ARCHER:
                return TECHNOLOGY.ARCHER_CLASS;
            case CHARACTER_CLASS.SPEARMAN:
                return TECHNOLOGY.SPEARMAN_CLASS;
            case CHARACTER_CLASS.SWORDSMAN:
                return TECHNOLOGY.SWORDSMAN_CLASS;
            case CHARACTER_CLASS.ROGUE:
                return TECHNOLOGY.ROGUE_CLASS;
            case CHARACTER_CLASS.WILDLING:
                return TECHNOLOGY.WILDLING_CLASS;
            case CHARACTER_CLASS.MAGE:
                return TECHNOLOGY.MAGE_CLASS;
            case CHARACTER_CLASS.BARBARIAN:
                return TECHNOLOGY.BARBARIAN_CLASS;
            case CHARACTER_CLASS.RANGER:
                return TECHNOLOGY.RANGER_CLASS;
            case CHARACTER_CLASS.KNIGHT:
                return TECHNOLOGY.KNIGHT_CLASS;
            case CHARACTER_CLASS.BATTLEMAGE:
                return TECHNOLOGY.BATTLEMAGE_CLASS;
            case CHARACTER_CLASS.ARCANIST:
                return TECHNOLOGY.ARCANIST_CLASS;
            case CHARACTER_CLASS.NIGHTBLADE:
                return TECHNOLOGY.NIGHTBLADE_CLASS;
            case CHARACTER_CLASS.SCOUT:
                return TECHNOLOGY.SCOUT_CLASS;
        }
        return TECHNOLOGY.NONE;
    }
    //This is the list of armor, set by priority, change if needed
    public static List<ARMOR_TYPE> orderedArmorTypes = new List<ARMOR_TYPE>() {
            ARMOR_TYPE.SHIRT,
            ARMOR_TYPE.LEGGINGS,
            ARMOR_TYPE.HELMET,
            ARMOR_TYPE.BOOT,
            ARMOR_TYPE.BRACER
    };
    public static TECHNOLOGY GetTechnologyForEquipment(EQUIPMENT_TYPE equipmentType) {
        switch (equipmentType) {
            case EQUIPMENT_TYPE.SWORD:
                return TECHNOLOGY.SWORD_MAKING;
            case EQUIPMENT_TYPE.DAGGER:
                return TECHNOLOGY.DAGGER_MAKING;
            case EQUIPMENT_TYPE.SPEAR:
                return TECHNOLOGY.SPEAR_MAKING;
            case EQUIPMENT_TYPE.BOW:
                return TECHNOLOGY.BOW_MAKING;
            case EQUIPMENT_TYPE.STAFF:
                return TECHNOLOGY.STAFF_MAKING;
            case EQUIPMENT_TYPE.AXE:
                return TECHNOLOGY.AXE_MAKING;
            case EQUIPMENT_TYPE.SHIRT:
                return TECHNOLOGY.CHEST_ARMOR_MAKING;
            case EQUIPMENT_TYPE.BRACER:
                return TECHNOLOGY.GLOVE_MAKING;
            case EQUIPMENT_TYPE.HELMET:
                return TECHNOLOGY.HELMET_MAKING;
            case EQUIPMENT_TYPE.LEGGINGS:
                return TECHNOLOGY.LEGGINGS_MAKING;
            case EQUIPMENT_TYPE.BOOT:
                return TECHNOLOGY.BOOT_MAKING;
            default:
                throw new Exception("There is no technology for " + equipmentType.ToString());
        }
    }
#endregion

    public static WeightedDictionary<ARMOR_TYPE> weightedArmorTypes;
    public static WeightedDictionary<ARMOR_TYPE> GetWeightedArmorTypes() {
        if(weightedArmorTypes == null) {
            weightedArmorTypes = new WeightedDictionary<ARMOR_TYPE>();
            weightedArmorTypes.AddElement(ARMOR_TYPE.SHIRT, 100);
            weightedArmorTypes.AddElement(ARMOR_TYPE.LEGGINGS, 80);
            weightedArmorTypes.AddElement(ARMOR_TYPE.HELMET, 60);
            weightedArmorTypes.AddElement(ARMOR_TYPE.BRACER, 40);
            weightedArmorTypes.AddElement(ARMOR_TYPE.BOOT, 20);
        }
        return weightedArmorTypes;
    }

	public static string GetNormalizedSingularRace(RACE race){
		switch(race){
		case RACE.HUMANS:
			return "Human";
		case RACE.ELVES:
			return "Elf";
		case RACE.MINGONS:
			return "Mingon";
		case RACE.CROMADS:
			return "Cromad";
		case RACE.GOBLIN:
			return "Goblin";
		case RACE.TROLL:
			return "Troll";
		case RACE.DRAGON:
			return "Dragon";
		default:
			return Utilities.NormalizeString (race.ToString ());
		}
	}

	public static bool IsRoleClassless(CHARACTER_ROLE role){
		if(role == CHARACTER_ROLE.WORKER){
			return true;
		}
		return false;
	}

    public static ITEM_TYPE GetItemTypeOfEquipment(EQUIPMENT_TYPE equipmentType) {
        switch (equipmentType) {
            case EQUIPMENT_TYPE.SWORD:
            case EQUIPMENT_TYPE.DAGGER:
            case EQUIPMENT_TYPE.SPEAR:
            case EQUIPMENT_TYPE.BOW:
            case EQUIPMENT_TYPE.STAFF:
            case EQUIPMENT_TYPE.AXE:
                return ITEM_TYPE.WEAPON;
            case EQUIPMENT_TYPE.SHIRT:
            case EQUIPMENT_TYPE.BRACER:
            case EQUIPMENT_TYPE.HELMET:
            case EQUIPMENT_TYPE.LEGGINGS:
            case EQUIPMENT_TYPE.BOOT:
                return ITEM_TYPE.ARMOR;
            default:
                return ITEM_TYPE.WEAPON;
        }
    }

    public static void LogDictionary<T, V>(Dictionary<T, V> dict) {
        string log = string.Empty;
        foreach (KeyValuePair<T, V> kvp in dict) {
            log += kvp.Key.ToString() + " - " + kvp.Value.ToString();
        }
        Debug.Log("Dictionary: " + log);
    }

    #region Character Tags
	public static int GetTagWorldGenChance(CHARACTER_TAG tag){
		switch(tag){
		case CHARACTER_TAG.HERBALIST:
		case CHARACTER_TAG.RITUALIST:
			return 13;
		default:
			return 0;
		}
	}
#endregion

    public static bool IsEven(int num) {
        return num % 2 == 0;
    }

    #region Character Relationship
	public static Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> charRelationshipCategory = new Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> () {
		{CHARACTER_RELATIONSHIP.RIVAL, CHARACTER_RELATIONSHIP_CATEGORY.NEGATIVE},
		{CHARACTER_RELATIONSHIP.FRIEND, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
		{CHARACTER_RELATIONSHIP.ENEMY, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
		{CHARACTER_RELATIONSHIP.SIBLING, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
		{CHARACTER_RELATIONSHIP.PARENT, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
		{CHARACTER_RELATIONSHIP.CHILD, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
		{CHARACTER_RELATIONSHIP.LOVER, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
		{CHARACTER_RELATIONSHIP.EX_LOVER, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
		{CHARACTER_RELATIONSHIP.APPRENTICE, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
		{CHARACTER_RELATIONSHIP.MENTOR, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
		{CHARACTER_RELATIONSHIP.ACQUAINTANCE, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
	};
    #endregion
    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture {
        get {
            if (_whiteTexture == null) {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    public static void DrawScreenRect(Rect rect, Color color) {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color) {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2) {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2) {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public static HexTile GetCenterTile(List<HexTile> tiles, HexTile[,] map, int width, int height) {
        int maxXCoordinate = tiles.Max(x => x.xCoordinate);
        int minXCoordinate = tiles.Min(x => x.xCoordinate);
        int maxYCoordinate = tiles.Max(x => x.yCoordinate);
        int minYCoordinate = tiles.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        if (width - 2 >= midPointX) {
            midPointX -= 2;
        }
        if (height - 2 >= midPointY) {
            midPointY -= 2;
        }
        if (midPointX >= 2) {
            midPointX += 2;
        }
        if (midPointY >= 2) {
            midPointY += 2;
        }
        midPointX = Mathf.Max(0, midPointX);
        midPointY = Mathf.Max(0, midPointY);
        try {
            HexTile newCenterOfMass = map[midPointX, midPointY];
            return newCenterOfMass;
        } catch {
            throw new Exception("Cannot Recompute center. Computed new center is " + midPointX.ToString() + ", " + midPointY.ToString());
        }
        
    }

    public static bool IsVisibleFrom(Renderer renderer, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
