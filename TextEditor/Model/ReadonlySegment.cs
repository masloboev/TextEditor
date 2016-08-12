namespace TextEditor.Model
{
    /// <summary>
    ///     Implementation for the segment that wasn't edited.
    /// </summary>
    class ReadonlySegment : ISegment
    {
        public long RowDataOffset { get; }
        public bool IsMonoWord { get; }
        public bool EndsWithNewLine { get; }

        private readonly long _endPosition;

        public ReadonlySegment(char[] data, long beginPosition, long endPosition, bool isMonoWord, bool endsWithNewLine)
        {
            RowData = data;
            RowDataOffset = beginPosition;
            _endPosition = endPosition;
            IsMonoWord = isMonoWord;
            EndsWithNewLine = endsWithNewLine;
        }

        public long Length => (int)(_endPosition - RowDataOffset + 1);

        public char[] RowData { get; }

    }
}
