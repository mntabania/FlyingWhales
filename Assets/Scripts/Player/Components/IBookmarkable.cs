public interface IBookmarkable {
    string persistentID { get; }
    string bookmarkName { get; }
    BOOKMARK_CATEGORY bookmarkCategory { get; }
    BOOKMARK_TYPE bookmarkType { get; }
    BookmarkableEventDispatcher eventDispatcher { get; }

    void OnSelectBookmark();
    void RemoveBookmark();
}

public class BookmarkableEventDispatcher {

    #region IListener
    public interface IListener {
        void OnBookmarkRemoved(IBookmarkable p_bookmarkable);
    }
    #endregion

    private System.Action<IBookmarkable> _onBookmarkRemoved;

    public void Subscribe(IListener p_listener) {
        _onBookmarkRemoved += p_listener.OnBookmarkRemoved;
    }
    public void Unsubscribe(IListener p_listener) {
        _onBookmarkRemoved -= p_listener.OnBookmarkRemoved;
    }
    public void ExecuteBookmarkRemovedEvent(IBookmarkable p_bookmarkable) {
        _onBookmarkRemoved?.Invoke(p_bookmarkable);
    }
}