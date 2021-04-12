public interface IBookmarkable {
    string bookmarkName { get; }
    BOOKMARK_TYPE bookmarkType { get; }
    BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

    void OnSelectBookmark();
    void RemoveBookmark();
}

public class BookmarkableEventDispatcher {

    #region IListener
    public interface IListener {
        void OnBookmarkRemoved(IBookmarkable p_bookmarkable);
        void OnBookmarkChangedName(IBookmarkable p_bookmarkable);
    }
    #endregion

    private System.Action<IBookmarkable> _onBookmarkRemoved;
    private System.Action<IBookmarkable> _onBookmarkableChangedName;

    public void Subscribe(IListener p_listener) {
        _onBookmarkRemoved += p_listener.OnBookmarkRemoved;
        _onBookmarkableChangedName += p_listener.OnBookmarkChangedName;
    }
    public void Unsubscribe(IListener p_listener) {
        _onBookmarkRemoved -= p_listener.OnBookmarkRemoved;
        _onBookmarkableChangedName -= p_listener.OnBookmarkChangedName;
    }
    public void ExecuteBookmarkRemovedEvent(IBookmarkable p_bookmarkable) {
        _onBookmarkRemoved?.Invoke(p_bookmarkable);
    }
    public void ExecuteBookmarkChangedNameEvent(IBookmarkable p_bookmarkable) {
        _onBookmarkableChangedName?.Invoke(p_bookmarkable);
    }

    public void ClearAll() {
        _onBookmarkRemoved = null;
        _onBookmarkableChangedName = null;
    }
}