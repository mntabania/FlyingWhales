using System.Collections.Generic;
using System.Linq;

public class BookmarkComponent {
    
    public Dictionary<BOOKMARK_CATEGORY, BookmarkCategory> bookmarkedObjects { get; }

    public BookmarkComponent() {
        bookmarkedObjects = new Dictionary<BOOKMARK_CATEGORY, BookmarkCategory>();
    }

    #region Listeners
    public void SubscribeListeners() {
        Messenger.AddListener<Party>(PartySignals.UNDEPLOY_PARTY, OnPartyUnDeployed);
        Messenger.AddListener<Party>(PartySignals.PARTY_QUEST_FINISHED_SUCCESSFULLY, PartyQuestFinishedSuccessfully);
        Messenger.AddListener<Party>(PartySignals.PARTY_QUEST_FAILED, PartyQuestFailed);
        Messenger.AddListener<Party>(PartySignals.DISBAND_PARTY, OnPartyDisbanded);
        
        Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
        Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedTarget);
    }
    private void OnPlayerStoredTarget(IStoredTarget p_storedTarget) {
        AddBookmark(p_storedTarget, BOOKMARK_CATEGORY.Targets);
    }
    private void OnPlayerRemovedTarget(IStoredTarget p_storedTarget) {
        RemoveBookmark(p_storedTarget, BOOKMARK_CATEGORY.Targets);
    }
    private void OnPartyDisbanded(Party p_party) {
        RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
    }
    private void OnPartyUnDeployed(Party p_party) {
        RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
    }
    private void PartyQuestFinishedSuccessfully(Party p_party) {
        RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
    }
    private void PartyQuestFailed(Party p_party) {
        RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
    }
    #endregion
    
    public void AddBookmark(IBookmarkable p_bookmarkable, BOOKMARK_CATEGORY p_category) {
        if (!bookmarkedObjects.ContainsKey(p_category)) {
            var bookmarkCategory = new BookmarkCategory(p_category);
            bookmarkedObjects.Add(p_category, bookmarkCategory);
            Messenger.Broadcast(PlayerSignals.BOOKMARK_CATEGORY_ADDED, bookmarkCategory);
        }
        bookmarkedObjects[p_category].AddBookmark(p_bookmarkable);
    }
    public void RemoveBookmark(IBookmarkable p_bookmarkable, BOOKMARK_CATEGORY p_category) {
        if (bookmarkedObjects.ContainsKey(p_category)) {
            BookmarkCategory bookmarkCategory = bookmarkedObjects[p_category];
            bookmarkCategory.RemoveBookmark(p_bookmarkable);
        }
    }
    public void RemoveBookmark(IBookmarkable p_bookmarkable) {
        foreach (var bookmarkedObject in bookmarkedObjects) {
            bookmarkedObject.Value.RemoveBookmark(p_bookmarkable);
        }
    }
}

public class SaveDataBookmarkComponent : SaveData<BookmarkComponent>{
    public override void Save(BookmarkComponent data) {
        base.Save(data);
    }
    public override BookmarkComponent Load() {
        return new BookmarkComponent();
    }
}