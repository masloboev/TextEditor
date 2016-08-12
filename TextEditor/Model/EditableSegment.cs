namespace TextEditor.Model
{
    /// <summary>
    ///     Segment that is editing by user.
    ///     For future release. 
    /// </summary>
    class EditableSegment : ISegment
    {
        public long RowDataOffset { get; }
        public bool IsMonoWord { get; }
        public bool EndsWithNewLine { get; }

        public EditableSegment()
        {
            RowData = new char[0];
            RowDataOffset = 0;
            IsMonoWord = false;
            EndsWithNewLine = false;
        }

        public long Length { get; } = 0;

        public char[] RowData { get; }
    }
}
