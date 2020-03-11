using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager Instance;

	public LANGUAGES language;
	public string filePath;

	protected Dictionary<string, Dictionary<string, Dictionary<string, string>>> _localizedText = new Dictionary<string, Dictionary<string, Dictionary<string, string>>> ();
	private bool isReady = false;
	//private string missingTextString = "Localized text not found";


	//getters and setters
	public Dictionary<string, Dictionary<string, Dictionary<string, string>>> localizedText{
		get {return this._localizedText;}
	}

	void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }
	internal void Initialize(){
		this.language = UtilityScripts.Utilities.defaultLanguage;
		this.filePath = $"{Application.streamingAssetsPath}/{this.language}";
		LoadLocalizedTexts ();
	}
	/*
	 * Load Localized Text From the StreamingAssets
	 * Directory, and put them in the localizedText Dictionary
	 * by category then by file name.
	 * e.g. localizedText["Events"]["BorderConflict"]
	 * */
	public void LoadLocalizedTexts(){
		this._localizedText.Clear ();
		string[] directories = Directory.GetDirectories(this.filePath);
		for (int i = 0; i < directories.Length; i++) {
			string categoryName = Path.GetFileName(directories [i]);
			if (!this._localizedText.ContainsKey(categoryName)) {
				this._localizedText.Add (categoryName, new Dictionary<string, Dictionary<string, string>> ());
			}
			string[] files = Directory.GetFiles(directories[i], "*.json");
			for (int j = 0; j < files.Length; j++) {
				string fileName = Path.GetFileNameWithoutExtension(files[j]);
				if (!this._localizedText.ContainsKey(fileName)) {
					this._localizedText [categoryName].Add (fileName, new Dictionary<string, string> ());
				}
				if (File.Exists(files[j])) {
					string dataAsJson = File.ReadAllText(files[j]);
					LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

					for (int k = 0; k < loadedData.items.Count; k++) {
                        if (this._localizedText[categoryName][fileName].ContainsKey(loadedData.items[k].key)) {
                            throw new System.Exception($"Duplicate key {loadedData.items[k].key} in {fileName}");
                        }
						this._localizedText[categoryName][fileName].Add(loadedData.items [k].key, loadedData.items [k].value);   
					}
					//Debug.Log ("Data loaded, dictionary contains: " + this._localizedText.Count + " entries");
				} else {
					Debug.LogError ("Cannot find file!");
				}
			}
		}
		this.isReady = true;
	}

	/*
	 * Get value of a key in a category in the
	 * localizedText Dictionary.
	 * */
	public string GetLocalizedValue(string category, string file, string key){
		string result = string.Empty;
        if (!this._localizedText.ContainsKey(category)) {
            Debug.LogWarning($"Localization error! {category}/");
            //throw new System.Exception("Localization error! " + category + "/");
        } else if (!this._localizedText[category].ContainsKey(file)) {
            Debug.LogWarning($"Localization error! {category}/{file}/");
            //throw new System.Exception("Localization error! " + category + "/" + file + "/");
        } else if (!this._localizedText[category][file].ContainsKey(key)) {
            Debug.LogWarning($"Localization error! {category}/{file}/{key}");
            //throw new System.Exception("Localization error! " + category + "/" + file + "/" + key);
        } else {
            result = this._localizedText[category][file][key];
        }
        return result;
	}
    public bool HasLocalizedValue(string category, string file, string key) {
        if (!this._localizedText.ContainsKey(category)) {
            return false;
        } else if (!this._localizedText[category].ContainsKey(file)) {
            return false;
        } else if (!this._localizedText[category][file].ContainsKey(key)) {
            return false;
        } else {
            return true;
        }
    }
    public List<string> GetKeysLike(string category, string file, string keyLike, string[] except = null) {
        List<string> keys = new List<string>();
        if (!this._localizedText.ContainsKey(category)) {
            Debug.LogWarning($"Localization error! {category}/");
            //throw new System.Exception("Localization error! " + category + "/");
        } else if (!this._localizedText[category].ContainsKey(file)) {
            Debug.LogWarning($"Localization error! {category}/{file}/");
            //throw new System.Exception("Localization error! " + category + "/" + file + "/");
        }
        Dictionary<string, string> logs = this.localizedText[category][file];
        foreach (KeyValuePair<string, string> kvp in logs) {
            if (except != null) {
                bool skipKey = false;
                for (int i = 0; i < except.Length; i++) {
                    string exceptStr = except[i];
                    if (kvp.Key.Contains(exceptStr)) {
                        skipKey = true;
                        break; //skip
                    }
                }
                if (skipKey) {
                    continue; //proceed to next key
                }
            }
            string key =  kvp.Key.Substring(0, kvp.Key.IndexOf('_'));
            if (key == keyLike) {
                keys.Add(kvp.Key);
            }
        }
        return keys;
    }
    public bool HasKeysLike(string category, string file, string keyLike, string[] except = null) {
        if (!this._localizedText.ContainsKey(category)) {
            Debug.LogWarning($"Localization error! {category}/");
            //throw new System.Exception("Localization error! " + category + "/");
        } else if (!this._localizedText[category].ContainsKey(file)) {
            Debug.LogWarning($"Localization error! {category}/{file}/");
            //throw new System.Exception("Localization error! " + category + "/" + file + "/");
        }
        Dictionary<string, string> logs = this.localizedText[category][file];
        foreach (KeyValuePair<string, string> kvp in logs) {
            if (except != null) {
                bool skipKey = false;
                for (int i = 0; i < except.Length; i++) {
                    string exceptStr = except[i];
                    if (kvp.Key.Contains(exceptStr)) {
                        skipKey = true;
                        break; //skip
                    }
                }
                if (skipKey) {
                    continue; //proceed to next key
                }
            }
            string key = kvp.Key.Substring(0, kvp.Key.IndexOf('_'));
            if (key == keyLike) {
                return true;
            }
        }
        return false;
    }
    public string GetRandomLocalizedValue(string category, string file){
		string result = string.Empty;
		int count = this._localizedText [category] [file].Keys.Count;
		KeyValuePair<string, string> selected = this._localizedText [category] [file].ElementAtOrDefault(UnityEngine.Random.Range(0,this._localizedText [category] [file].Count));
		if(!string.IsNullOrEmpty(selected.Key) && !string.IsNullOrEmpty(selected.Value)){
			result = selected.Value;
		}
		return result;
	}
	public string GetRandomLocalizedKey(string category, string file){
		string result = string.Empty;
		int count = this._localizedText [category] [file].Keys.Count;
		KeyValuePair<string, string> selected = this._localizedText [category] [file].ElementAtOrDefault(UnityEngine.Random.Range(0,this._localizedText [category] [file].Count));
		if(!string.IsNullOrEmpty(selected.Key) && !string.IsNullOrEmpty(selected.Value)){
			result = selected.Key;
		}
		return result;
	}

	/*
	 * Get if the LocalizationManager has loaded
	 * all the necessary files into the localizedText
	 * Dictionary.
	 * */
	public bool GetIsReady(){
		return isReady;
	}
	

}
