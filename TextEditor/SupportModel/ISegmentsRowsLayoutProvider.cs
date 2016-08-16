using System;
using System.Threading.Tasks;
using TextEditor.Attributes;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Interface provides SegmentsRowsLayout for each viewport with
    ///     SegmentsRowsLayout calculation is very slow, cancellation and progress is appliable
    /// </summary>
    public interface ISegmentsRowsLayoutProvider
    {
        /// <summary>
        ///     Tries to get the precalulated layout for specified symbolsInRowCount .
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        /// <returns>if precalculated layout exists returns it</returns>
        ISegmentsRowsLayout TryGet(int symbolsInRowCount);

        /// <summary>
        ///     Initiation for async calculation with progress
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        /// <param name="onProgress">The progress callback</param>
        /// <returns>I return calculating task or start new task. Task creates and erases only in UI thread, so this code is safe</returns>
        [return: NotNull]
        Task<ISegmentsRowsLayout> GetAsync(int symbolsInRowCount, EventHandler<string> onProgress);

        /// <summary>
        /// View model progress unsubscription method.
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        /// <param name="onProgress">The progress callback</param>
        void Unprogress(int symbolsInRowCount, EventHandler<string> onProgress);
    }
}
