using TextEditor.Model;

namespace TextEditor
{
    /// <summary>
    ///     Application constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        ///     input stream reader buffer size in <see cref="Segmentizer"/>
        /// </summary>
        public const int FileLoaderBatchSize = 65536;
        /// <summary>
        ///     priniting tab offset
        /// </summary>
        public const int TabSize = 4;
        /// <summary>
        ///     lower threshold for <see cref="Segmentizer"/>
        /// </summary>
        /// <remarks>
        ///     on small segments LineBreaks algorythm works fast
        ///     on large segments mutch more good splitted (on paragraph) segments
        ///     too mutch small segments needs more memory
        /// </remarks>
        public const long SegmentizerLowerThreshold = 1024 * 1024;
        /// <summary>
        ///     upper threshold for <see cref="Segmentizer"/>
        /// </summary>
        public const long SegmentizerUpperThreshold = 2 * 1024 * 1024;
        /// <summary>
        ///     error threshold for <see cref="Segmentizer"/>
        /// </summary>
        public const long SegmentizerErrorThreshold = 100 * 1024 * 1024;
    }
}
