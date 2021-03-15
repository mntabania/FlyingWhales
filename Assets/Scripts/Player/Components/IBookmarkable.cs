public interface IBookmarkable {
    string bookmarkName { get; }
    BOOKMARK_CATEGORY bookmarkCategory { get; }

    void OnSelectBookmark();
    void RemoveBookmark();
}