using UnityEngine;

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
    /// <summary>
    /// Event for when a bookmarkable changes names or icons.
    /// </summary>
    private System.Action<IBookmarkable> _onBookmarkableChangedNameOrElements;

    public void Subscribe(IListener p_listener, IBookmarkable p_subscribeTo) {
        _onBookmarkRemoved += p_listener.OnBookmarkRemoved;
        _onBookmarkableChangedNameOrElements += p_listener.OnBookmarkChangedName;
        Debug.Log($"{p_listener.ToString()} subscribed to bookmarkable: {p_subscribeTo.bookmarkName} events");
    }
    public void Unsubscribe(IListener p_listener, IBookmarkable p_unsubscribeFrom) {
        _onBookmarkRemoved -= p_listener.OnBookmarkRemoved;
        _onBookmarkableChangedNameOrElements -= p_listener.OnBookmarkChangedName;
        Debug.Log($"{p_listener.ToString()} unsubscribed to bookmarkable: {p_unsubscribeFrom.bookmarkName} events");
    }
    public void ExecuteBookmarkRemovedEvent(IBookmarkable p_bookmarkable) {
        _onBookmarkRemoved?.Invoke(p_bookmarkable);
    }
    public void ExecuteBookmarkChangedNameOrElementsEvent(IBookmarkable p_bookmarkable) {
        _onBookmarkableChangedNameOrElements?.Invoke(p_bookmarkable);
    }

    public void ClearAll() {
        _onBookmarkRemoved = null;
        _onBookmarkableChangedNameOrElements = null;
    }
}