using Steamworks;
using System.Collections;
using UnityEngine;

public class AchievementManager : MonoBehaviour {

    public static AchievementManager Instance = null;

    private enum Achievement : int {
        FIRST_SNATCH,
        FIRST_INTERACTION,
        SNATCH_5,
    };

    private SteamAchievement[] m_Achievements = new SteamAchievement[] {
        new SteamAchievement(Achievement.FIRST_SNATCH, "", ""),
        new SteamAchievement(Achievement.FIRST_INTERACTION, "", ""),
        new SteamAchievement(Achievement.SNATCH_5, "", ""),
    };

    private CGameID _gameID;

    protected Steamworks.Callback<UserStatsReceived_t> onUserStatsRecieved;
    protected Steamworks.Callback<UserStatsStored_t> onUserStatsStored;
    protected Steamworks.Callback<UserAchievementStored_t> onUserAchievementStored;

    // Did we get the stats from Steam?
    private bool statsRequested;
    private bool statsValid;

    // Should we store stats this frame?
    private bool shouldStoreStats;

    // Persisted Stat details
    public int charactersSnatched;

    #region Monobehaviours
    private void OnEnable() {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        _gameID = new CGameID(SteamUtils.GetAppID());

        onUserStatsRecieved = Steamworks.Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        onUserStatsStored = Steamworks.Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        onUserAchievementStored = Steamworks.Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        statsRequested = false;
        statsValid = false;
    }
    private void Awake() {
        Instance = this;
    }
    private void Update() {
        if (!SteamManager.Initialized)
            return;

        if (!statsRequested) {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized) {
                statsRequested = true;
                return;
            }

            // If yes, request our stats
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            statsRequested = bSuccess;
        }

        if (!statsValid)
            return;

        //Store stats in the Steam database if necessary
        if (shouldStoreStats) {
            // already set any achievements in UnlockAchievement

            // set stats
            SteamUserStats.SetStat("snatched_characters", charactersSnatched);
            
            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            shouldStoreStats = !bSuccess;
        }

        
    }
    private void OnGUI() {
        if (!SteamManager.Initialized) {
            //GUILayout.Label("Steamworks not Initialized");
            return;
        }

        if (UIManager.Instance == null || !UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        //GUILayout.Label("m_ulTickCountGameStart: " + m_ulTickCountGameStart);
        //GUILayout.Label("m_flGameDurationSeconds: " + m_flGameDurationSeconds);
        //GUILayout.Label("m_flGameFeetTraveled: " + m_flGameFeetTraveled);
        //GUILayout.Space(10);
        //GUILayout.Label("NumGames: " + m_nTotalGamesPlayed);
        //GUILayout.Label("NumWins: " + m_nTotalNumWins);
        //GUILayout.Label("NumLosses: " + m_nTotalNumLosses);
        //GUILayout.Label("FeetTraveled: " + m_flTotalFeetTraveled);
        //GUILayout.Label("MaxFeetTraveled: " + m_flMaxFeetTraveled);
        //GUILayout.Label("AverageSpeed: " + m_flAverageSpeed);

        GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
        foreach (SteamAchievement ach in m_Achievements) {
            GUILayout.Label(ach.m_eAchievementID.ToString());
            GUILayout.Label($"{ach.m_strName} - {ach.m_strDescription}");
            GUILayout.Label($"Achieved: {ach.m_bAchieved}");
            GUILayout.Space(20);
        }

        // FOR TESTING PURPOSES ONLY!
        if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS")) {
            SteamUserStats.ResetAllStats(true);
            ResetStats();
            //SteamUserStats.RequestCurrentStats();
            //OnGameStateChange(EClientGameState.k_EClientGameActive);
            shouldStoreStats = true;
        }
        GUILayout.EndArea();
    }
    #endregion

    public void Initialize() {
        Messenger.AddListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
    }

    #region Callbacks
    private void OnUserStatsReceived(UserStatsReceived_t val) {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)_gameID == val.m_nGameID) {
            if (EResult.k_EResultOK == val.m_eResult) {
                Debug.Log("Received stats and achievements from Steam\n");

                statsValid = true;

                // load achievements
                foreach (SteamAchievement ach in m_Achievements) {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret) {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    } else {
                        Debug.LogWarning(
                            $"SteamUserStats.GetAchievement failed for Achievement {ach.m_eAchievementID}\nIs it registered in the Steam Partner site?");
                    }
                }

                // load stats
                SteamUserStats.GetStat("snatched_characters", out charactersSnatched);
            } else {
                Debug.Log($"RequestStats - failed, {val.m_eResult}");
            }
        }
    }
    private void OnUserStatsStored(UserStatsStored_t val) {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)_gameID == val.m_nGameID) {
            if (EResult.k_EResultOK == val.m_eResult) {
                Debug.Log("StoreStats - success");
            } else if (EResult.k_EResultInvalidParam == val.m_eResult) {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)_gameID;
                OnUserStatsReceived(callback);
            } else {
                Debug.Log($"StoreStats - failed, {val.m_eResult}");
            }
        }
    }
    private void OnAchievementStored(UserAchievementStored_t val) {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)_gameID == val.m_nGameID) {
            if (0 == val.m_nMaxProgress) {
                Debug.Log($"Achievement '{val.m_rgchAchievementName}' unlocked!");
            } else {
                Debug.Log(
                    $"Achievement '{val.m_rgchAchievementName}' progress callback, ({val.m_nCurProgress},{val.m_nMaxProgress})");
            }
        }
    }
    #endregion

    private void UnlockAchievement(SteamAchievement achievement) {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        shouldStoreStats = true;
    }

    private void ResetStats() {
        charactersSnatched = 0;
    }

    #region Handlers
    private void OnCharacterSnatched(Character snatchedCharacter) {
        charactersSnatched++;
        shouldStoreStats = true;
        Debug.Log($"Incremented snatched characters to {charactersSnatched}");
    }
    #endregion

    private class SteamAchievement {
        public Achievement m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public SteamAchievement(Achievement achievementID, string name, string desc) {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
}

