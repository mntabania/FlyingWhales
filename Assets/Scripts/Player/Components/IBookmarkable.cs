using UnityEngine;

public interface IBookmarkable {
    string bookmarkName { get; }
    BOOKMARK_TYPE bookmarkType { get; }
    BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

    void OnSelectBookmark();
    void RemoveBookmark();
}

public class GenericTextBookmarkable : IBookmarkable {
    
    private System.Func<string> _nameGetter;
    private System.Func<BOOKMARK_TYPE> _bookmarkTypeGetter;
    private System.Action _onSelectAction;
    private System.Action _removeBookmarkAction;
    
    public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

    #region getters
    public string bookmarkName => _nameGetter?.Invoke() ?? string.Empty;
    public BOOKMARK_TYPE bookmarkType => _bookmarkTypeGetter?.Invoke() ?? BOOKMARK_TYPE.Text;
    #endregion

    public GenericTextBookmarkable(System.Func<string> p_nameGetter, System.Func<BOOKMARK_TYPE> p_bookmarkTypeGetter, System.Action p_onSelectAction, System.Action p_removeBookmarkAction) {
        bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        _nameGetter = p_nameGetter;
        _bookmarkTypeGetter = p_bookmarkTypeGetter;
        _onSelectAction = p_onSelectAction;
        _removeBookmarkAction = p_removeBookmarkAction;
    }
    public void OnSelectBookmark() {
        _onSelectAction?.Invoke();
    }
    public void RemoveBookmark() {
        _removeBookmarkAction?.Invoke();
    }
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