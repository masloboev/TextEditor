using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor
{    
    /// <summary>
    ///     Interface for algorythm to split segment lines into rows with specified row length
    /// </summary>
    public interface ILineBreaker
    {
        /// <summary>
        /// Calculates rows list of segment
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>rows list</returns>
        [return: NotNull]
        List<Row> GetRows([NotNull] ISegment segment);

        /// <summary>
        /// Just height of segment in rows calculation (used in layout)
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>row count</returns>
        int GetRowsCount([NotNull] ISegment segment);
        
        /// <summary>
        /// Gets count of symbols in row.
        /// </summary>
        int SymbolsInRowCount { get; }
    }
}
