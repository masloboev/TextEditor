using System.Collections.Generic;
using System.Text;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Model for viewport calculated segment rows
    /// </summary>
    public class SegmentViewModel
    {
        public ISegment Segment { get; }
        /// <summary>
        ///     Rows prepared for viewport
        /// </summary>
        public List<Row> Rows { get; }

        public SegmentViewModel(LineBreaker lineBreaker, ISegment segment)
        {
            Segment = segment;
            Rows = lineBreaker.GetRows(Segment);
        }

        /// <summary>
        ///     Method prints whole segment to contents builder
        /// </summary>
        /// <remarks>
        ///     Methods prints different symbols depends on segment type
        ///     ↓ nornal segments with paragraph end
        ///     ⇣ bad segments separated by whitespace not paragraph.
        ///     ⇃ segments with only one word 
        /// </remarks>
        public void WriteContent(RowWriter rowWriter, StringBuilder sb, int linePosition, int rowsCount)
        {
            for (var i = linePosition; i < linePosition + rowsCount; i++)
            {
                rowWriter.WriteRow(Rows[i], sb);
                if (i == RowsCount - 1)
                    sb.AppendLine(Segment.IsMonoWord ? "⇃" : Segment.EndsWithNewLine ? "↓" : "⇣");
                else
                    sb.AppendLine();
            }
        }

        /// <summary>
        ///     calculated segment height
        /// </summary>
        public int RowsCount => Rows.Count;
    }
}
