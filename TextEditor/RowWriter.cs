using System.Text;
using TextEditor.SupportModel;

namespace TextEditor
{
    /// <summary>
    ///     Tool to print one row to content builder
    /// </summary>
    public class RowWriter
    {
        public void WriteRow(Row row, StringBuilder sb)
        {
            for (var currentPosition = row.BeginPosition; currentPosition < row.BeginPosition + row.Length; currentPosition++)
            {
                var current = row.RowData[currentPosition];
                if (current == '\r' && currentPosition + 1 < row.Length && row.RowData[currentPosition + 1] == '\n')
                {
                    current = '\n';
                    currentPosition++;
                }

                if (current == '\n')
                    sb.Append('¶');
                else if (char.IsWhiteSpace(current))
                    if (current == '\t')
                    {
                        var count = Constants.TabSize - (currentPosition - row.BeginPosition) % Constants.TabSize;
                        for (var i = 0; i < count - 1; i++)
                            sb.Append(' ');
                        sb.Append('→');
                    }
                    else
                        sb.Append('·');
                else
                    sb.Append(current);
            }
        }
    }
}
