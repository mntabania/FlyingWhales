using System.Collections.Generic;

public class BookmarkCategory {

    #region IListener
    public interface IListener {
        void OnBookmarkAdded(IBookmarkable p_bookmarkable);
        void OnBookmarkCategoryEmptiedOut(BookmarkCategory p_category);
    }
    #endregion
    
    public string displayName { get; }
    public BOOKMARK_CATEGORY bookmarkCategory { get; }
    public List<IBookmarkable> bookmarked { get; }

    private System.Action<IBookmarkable> _onBookmarkAdded;
    private System.Action<BookmarkCategory> _onBookmarkCategoryEmptiedOut;
    
    public BookmarkCategory(BOOKMARK_CATEGORY p_category) {
        bookmarkCategory = p_category;
        bookmarked = new List<IBookmarkable>();
        displayName = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_category.ToString());
    }

    public void AddBookmark(IBookmarkable p_bookmarkable) {
        bookmarked.Add(p_bookmarkable);
        ExecuteBookmarkAdded(p_bookmarkable);
    }
    public bool RemoveBookmark(IBookmarkable p_bookmarkable) {
        if (bookmarked.Remove(p_bookmarkable)) {
            p_bookmarkable.bookmarkEventDispatcher.ExecuteBookmarkRemovedEvent(p_bookmarkable);
            if (IsEmpty()) {
                ExecuteBookmarkEmptiedOut(this);
            }
            return true;
        }
        return false;
    }
    private bool IsEmpty() {
        return bookmarked.Count == 0;
    }

    #region Listeners
    public void SubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded += p_listener.OnBookmarkAdded;
        _onBookmarkCategoryEmptiedOut += p_listener.OnBookmarkCategoryEmptiedOut;
    }
    public void UnsubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded -= p_listener.OnBookmarkAdded;
        _onBookmarkCategoryEmptiedOut -= p_listener.OnBookmarkCategoryEmptiedOut;
    }
    private void ExecuteBookmarkAdded(IBookmarkable p_bookmarkable) {
        _onBookmarkAdded?.Invoke(p_bookmarkable);
    }
    private void ExecuteBookmarkEmptiedOut(BookmarkCategory p_bookmarkCategory) {
        _onBookmarkCategoryEmptiedOut?.Invoke(p_bookmarkCategory);
    }
    #endregion
}