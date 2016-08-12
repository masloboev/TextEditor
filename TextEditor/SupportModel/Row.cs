using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Model for Row to print in viewport. Is similar to <see cref="ReadonlySegment"/>
    /// </summary>
    public class Row
    {
        public long BeginPosition { get; }
        public bool IsMonoWord { get; }
        public bool EndsWithNewLine { get; }

        private readonly long _endPosition;

        public Row(char[] data, long beginPosition, long endPosition, bool isMonoWord, bool endsWithNewLine)
        {
            RowData = data;
            BeginPosition = beginPosition;
            _endPosition = endPosition;
            IsMonoWord = isMonoWord;
            EndsWithNewLine = endsWithNewLine;
        }

        public long Length => (int)(_endPosition - BeginPosition + 1);

        public char[] RowData { get; }

        public long RowOffset => BeginPosition;
    }
}
