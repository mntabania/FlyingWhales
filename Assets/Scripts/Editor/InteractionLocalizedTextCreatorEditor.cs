#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class InteractionLocalizedTextCreatorEditor : EditorWindow {
    public TrelloCard cardData;
    //public List<LogReplacerItem> logReplacers = new List<LogReplacerItem>();
    public LogReplacerDictionary logReplacers;

    string trelloJSONStr = "Place trello json here...";
    //Vector2 scrollPos = Vector2.zero;

    string textAreaStr;
    Vector2 textAreaScroll = Vector2.zero;

    [MenuItem("Window/Interaction Localized Text Creator")]
    static void Init() {
        EditorWindow.GetWindow(typeof(InteractionLocalizedTextCreatorEditor)).Show();
    }

    private void OnGUI() {
        SerializedObject serializedObject = new SerializedObject(this);
        textAreaScroll = EditorGUILayout.BeginScrollView(textAreaScroll);
        EditorGUILayout.LabelField(textAreaStr, GUILayout.Height(position.height));

        SerializedProperty serializedProperty = serializedObject.FindProperty("logReplacers");
        EditorGUILayout.PropertyField(serializedProperty, true);
        EditorGUILayout.EndScrollView();

        if (cardData != null) {
            if (GUILayout.Button("Save As Localized Text")) {
                SaveAsLocalizedText();
            }
        }
        if (GUILayout.Button("Load JSON data")) {
            string path = EditorUtility.OpenFilePanel("Pick json file", "", "json");
            if (path.Length != 0) {
                trelloJSONStr = File.ReadAllText(path);
            }
            LoadJSONData();
        }
        serializedObject.ApplyModifiedProperties();
    }
    private void LoadJSONData() {
        if (!string.IsNullOrEmpty(trelloJSONStr)) {
            cardData = JsonUtility.FromJson<TrelloCard>(trelloJSONStr);
            Debug.Log(cardData.ToString());
            textAreaStr = cardData.ToString();
            LoadLogReplacers();
        } else {
            cardData = null;
        }
    }
    private void LoadLogReplacers() {
        logReplacers.Clear();
        for (int i = 0; i < cardData.checklists.Count; i++) {
            Checklist currChecklist = cardData.checklists[i];
            for (int j = 0; j < currChecklist.checkItems.Count; j++) {
                ChecklistItem currChecklistItem = currChecklist.checkItems[j];
                string text = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                text = text.TrimStart();
                List<string> words = UtilityScripts.Utilities.ExtractFromString(text, "[", "]");
                for (int k = 0; k < words.Count; k++) {
                    string currWord = words[k];
                    currWord = "[" + currWord + "]";
                    if (!logReplacers.ContainsKey(currWord)) {
                        logReplacers.Add(currWord, GetLogIdentifierForString(currWord));
                    }
                }
            }
        }
    }
    private void SaveAsLocalizedText() {
        cardData = JsonUtility.FromJson<TrelloCard>(trelloJSONStr);
        LocalizationData localizationData = new LocalizationData();
        localizationData.items = new List<LocalizationItem>();
        for (int i = 0; i < cardData.checklists.Count; i++) {
            Checklist currChecklist = cardData.checklists[i];
            string stateName = string.Empty;
            if (currChecklist.name.Equals("Action Start")){
                stateName = "start";
            } else if (currChecklist.name.Contains("State") || currChecklist.name.Contains("Result")) {
                stateName = currChecklist.name.Substring(currChecklist.name.IndexOf(": ") + 1);
                stateName = stateName.Trim().ToLower();
            } else {
                continue; //skip
            }

            int logCount = 1;
            for (int j = 0; j < currChecklist.checkItems.Count; j++) {
                ChecklistItem currChecklistItem = currChecklist.checkItems[j];
                if (currChecklistItem.name.Contains("Text Description")) {
                    string description = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                    description = ConvertToLogFillers(description.TrimStart());
                    localizationData.items.Add(new LocalizationItem() {
                        key = stateName + "_description",
                        value = description,
                    });
                } 
                //else if (currChecklistItem.name.Contains("Log") || currChecklistItem.name.Contains("Logs")) {
                //    string log = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                //    log = ConvertToLogFillers(log.TrimStart());
                //    localizationData.items.Add(new LocalizationItem() {
                //        key = stateName + "_log" + logCount,
                //        value = log,
                //    });
                //    logCount++;
                //} 
                else if (currChecklistItem.name.Contains("Thought Bubble")) {
                    string log = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                    log = ConvertToLogFillers(log.TrimStart());
                    if (currChecklistItem.name.Contains("Moving")) {
                        localizationData.items.Add(new LocalizationItem() {
                            key = "thought_bubble_m",
                            value = log,
                        });
                    } else if (currChecklistItem.name.Contains("Target:")) {
                        localizationData.items.Add(new LocalizationItem() {
                            key = "target_log",
                            value = log,
                        });
                    } else {
                        localizationData.items.Add(new LocalizationItem() {
                            key = "thought_bubble",
                            value = log,
                        });
                    }
                    logCount++;
                }
            }
        }

        var path = EditorUtility.SaveFilePanel(
                "Save data as Localized Text JSON",
                Application.streamingAssetsPath + "/ENGLISH",
                cardData.name + ".json",
                "json");

        if (path.Length != 0) {
            var jsonData = JsonUtility.ToJson(localizationData);
            if (jsonData != null)
                File.WriteAllText(path, jsonData);
        }
    }

    private string ConvertToLogFillers(string source) {
        string newString = source;
        foreach (KeyValuePair<string, LOG_IDENTIFIER> kvp in logReplacers) {
            if (kvp.Value != LOG_IDENTIFIER.NONE) {
                newString = newString.Replace(kvp.Key, UtilityScripts.Utilities.GetStringForIdentifier(kvp.Value));
            }
        }

        //if (logReplacers.ContainsKey(source)) {
        //    if (logReplacers[source] != LOG_IDENTIFIER.NONE) {
        //        newString = newString.Replace(source, Utilities.GetStringForIdentifier(logReplacers[source]));
        //    }
        //}
        //newString = newString.Replace("[Demon Name]", "%113%");
        //newString = newString.Replace("[Demon]", "%113%");
        //newString = newString.Replace("[Minion Name]", "%113%");

        //newString = newString.Replace("[Character Name]", "%00@");
        //newString = newString.Replace("[Character Name 1]", "%00@");
        //newString = newString.Replace("[Character Name 2]", "%10@");

        //newString = newString.Replace("[Location Name]", "%04@");
        //newString = newString.Replace("[Location Name 2]", "%14@");

        //newString = newString.Replace("[Faction Name]", "%01@");
        //newString = newString.Replace("[Faction Name 1]", "%01@");
        //newString = newString.Replace("[Faction Name 2]", "%11@");
        return newString;
    }

    private LOG_IDENTIFIER GetLogIdentifierForString(string str) {
        switch (str) {
            case "[Demon Name]":
            case "[Demon]":
            case "[Minion Name]":
            case "[User Name]":
                return LOG_IDENTIFIER.MINION_1;
            case "[Character Name]":
            case "[Character Name 1]":
            case "[Actor Name]":
                return LOG_IDENTIFIER.ACTIVE_CHARACTER;
            case "[Character Name 2]":
            case "[Target Name]":
                return LOG_IDENTIFIER.TARGET_CHARACTER;
            case "[Location Name]":
            case "[Location Name 1]":
                return LOG_IDENTIFIER.LANDMARK_1;
            case "[Location Name 2]":
                return LOG_IDENTIFIER.LANDMARK_1;
            case "[Faction Name]":
            case "[Faction Name 1]":
                return LOG_IDENTIFIER.FACTION_1;
            case "[Faction Name 2]":
                return LOG_IDENTIFIER.FACTION_2;
            default:
                return LOG_IDENTIFIER.NONE;
        }
    }
}

[System.Serializable]
public class TrelloCard {
    public string name;
    public List<Checklist> checklists;

    public override string ToString() {
        string summary = name + "\n";
        for (int i = 0; i < checklists.Count; i++) {
            Checklist currList = checklists[i];
            summary += currList.name + "\n";
            for (int j = 0; j < currList.checkItems.Count; j++) {
                ChecklistItem currListItem = currList.checkItems[j];
                summary += "    " + currListItem.name + "\n";
            }
        }
        return summary;
    }
}
[System.Serializable]
public class Checklist {
    public string name;
    public List<ChecklistItem> checkItems;
}
[System.Serializable]
public class ChecklistItem {
    public string name;
}
