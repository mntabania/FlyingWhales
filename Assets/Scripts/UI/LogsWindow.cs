using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UtilityScripts;
using Debug = UnityEngine.Debug;

public class LogsWindow : MonoBehaviour {
    [Space(10)] [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private UIHoverPosition logHoverPosition;
    [SerializeField] private GameObject daySeparatorPrefab;
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private Button clearBtn;
    private List<LogHistoryItem> logHistoryItems;
    private List<DaySeparator> daySeparators;

    [Space(10)] [Header("Filters")] 
    [SerializeField] private GameObject filterGO;
    [SerializeField] private LogFilterItem[] allFilters;
    [SerializeField] private Toggle showAllToggle;

    private string _objPersistentID;

    private static string SharedSearch;
    private static List<LOG_TAG> SharedEnabledFilters;
    
    private void OnDisable() {
        filterGO.gameObject.SetActive(false);
    }
    private void OnDestroy() {
        SharedSearch = string.Empty;
        SharedEnabledFilters = null;
    }
    public void Initialize() {
        logHistoryItems = new List<LogHistoryItem>();
        daySeparators = new List<DaySeparator>();
        searchField.onValueChanged.AddListener(OnEndSearchEdit);
        clearBtn.onClick.AddListener(OnClickClearSearch);
        //default logs filters to all be on.
        if (SharedEnabledFilters == null) {
            SharedEnabledFilters = UtilityScripts.CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
        }
        showAllToggle.SetIsOnWithoutNotify(true);
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem logFilterItem = allFilters[i];
            logFilterItem.SetOnToggleAction(OnToggleFilter);
            logFilterItem.SetIsOnWithoutNotify(true);
        }    
        for (int i = 0; i < 200; i++) {
            CreateNewLogHistoryItem();
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && EventSystem.current.currentSelectedGameObject == searchField.gameObject) {
            DoSearch();
        }
    }
    public void OnParentMenuOpened(string id) {
        _objPersistentID = id;
        //update text visual
        searchField.SetTextWithoutNotify(SharedSearch);
        clearBtn.gameObject.SetActive(!string.IsNullOrEmpty(SharedSearch));
        //update filter visuals
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            filterItem.SetIsOnWithoutNotify(SharedEnabledFilters.Contains(filterItem.filterType));
        }
        showAllToggle.SetIsOnWithoutNotify(AreAllFiltersOn());
    }
    
    private void CreateNewLogHistoryItem() {
        GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
        newLogItem.transform.localScale = Vector3.one;
        newLogItem.SetActive(true);
        LogHistoryItem logHistoryItem = newLogItem.GetComponent<LogHistoryItem>();
        logHistoryItems.Add(logHistoryItem);
    }
    public void UpdateAllHistoryInfo() {
        UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoCoroutine());
    }

    private IEnumerator UpdateAllHistoryInfoCoroutine() {
#if UNITY_EDITOR
        Stopwatch timer = new Stopwatch();
        timer.Start();
#endif
#if DEBUG_PROFILER
        Profiler.BeginSample("Get Logs that match criteria");
#endif
        List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetLogsThatMatchCriteria(_objPersistentID, SharedSearch, SharedEnabledFilters);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        int historyCount = logs?.Count ?? 0;
        int historyLastIndex = historyCount - 1;
        int missingItems = historyCount - logHistoryItems.Count;
        int batches = 0;
        for (int i = 0; i < missingItems; i++) {
            CreateNewLogHistoryItem();
            batches++;
            if (batches > 200) {
                batches = 0;
                yield return null;
            }
        }
        for (int i = 0; i < daySeparators.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(daySeparators[i]);
        }
        daySeparators.Clear();

        batches = 0;
        int currentDay = 0;
        for (int i = 0; i < logHistoryItems.Count; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            currItem.ManualReset();
            if(logs != null && i < historyCount) {
                Log currLog = logs[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                currItem.SetHoverPosition(logHoverPosition);
                if (currLog.gameDate.day != currentDay) {
                    int siblingIndex = currItem.transform.GetSiblingIndex();
                    if (siblingIndex < 0) {
                        siblingIndex = 0;
                    }
                    CreateDaySeparator(currLog.gameDate.ConvertToContinuousDays(), siblingIndex);
                    currentDay = currLog.gameDate.day;
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
            batches++;
            if (batches > 200) {
                batches = 0;
                yield return null;
            }
        }
        logs.ReleaseLogInstancesAndLogList();
#if UNITY_EDITOR
        timer.Stop();
        UnityEngine.Debug.Log($"Log items creation time was {timer.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
#endif
    }
    
    private void CreateDaySeparator(int day, int indexInHierarchy) {
        //create day separator prefab
        GameObject dayGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(daySeparatorPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
        DaySeparator daySeparator = dayGO.GetComponent<DaySeparator>();
        daySeparator.SetDay(day);
        dayGO.transform.SetSiblingIndex(indexInHierarchy); //swap log with day label since log is the first log in the day
        daySeparators.Add(daySeparator);
    }
    public void ResetScrollPosition() {
        historyScrollView.verticalNormalizedPosition = 1;
    }

#region Search
    public void DoSearch() {
        UpdateAllHistoryInfo();
    }
    private void OnEndSearchEdit(string text) {
        // if (string.IsNullOrEmpty(text)) {
        //     //only update automatically if text changed to empty
        //     UpdateAllHistoryInfo();
        // }
        SharedSearch = text;
        clearBtn.gameObject.SetActive(!string.IsNullOrEmpty(SharedSearch)); //show clear button if there is a given text
        UpdateAllHistoryInfo();
    }
    private void OnClickClearSearch() {
        searchField.SetTextWithoutNotify(string.Empty);
        OnEndSearchEdit(string.Empty);
    }
#endregion

#region Filters
    public void ToggleFilters() {
        filterGO.gameObject.SetActive(!filterGO.activeInHierarchy);
    }
    private void OnToggleFilter(bool isOn, LOG_TAG tag) {
        if (isOn) {
            SharedEnabledFilters.Add(tag);
        } else {
            SharedEnabledFilters.Remove(tag);
        }
        showAllToggle.SetIsOnWithoutNotify(AreAllFiltersOn());
        UpdateAllHistoryInfo();
    }
    private bool AreAllFiltersOn() {
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            if (!filterItem.isOn) {
                return false;
            }
        }
        return true;
    }
    public void OnToggleAllFilters(bool state) {
        SharedEnabledFilters.Clear();
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            filterItem.SetIsOnWithoutNotify(state);
            if (state) {
                //if search all is enabled then add filter. If it is not do not do anything to the list since list was cleared beforehand.
                SharedEnabledFilters.Add(filterItem.filterType);    
            }
        }
        UpdateAllHistoryInfo();
    }
#endregion
}
