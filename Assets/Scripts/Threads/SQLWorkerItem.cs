namespace Threads {
    /// <summary>
    /// Base class for all multi-threaded scripts related to SQL Database Management.
    /// </summary>
    public abstract class SQLWorkerItem : Multithread {

        public abstract void Reset();
    }
}