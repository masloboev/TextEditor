using System;
using System.Collections.Generic;
using TextEditor.Attributes;

namespace TextEditor.Model
{
    /// <summary>
    /// Implementation of <seealso cref="TextEditor.Model.IDocument" />
    /// Base presentation of text document
    /// </summary>
    public class Document : IDocument
    {
        /// <summary>
        /// The segments list
        /// </summary>
        private readonly IReadOnlyList<ISegment> _segments;

        /// <summary>
        /// Index to find the leaf node by the text segment
        /// </summary>
        private readonly Dictionary<ISegment, int> _segmentIndexMap;

        /// <summary>
        /// Initializes a new instance by segmenents list.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Document([NotNull] IReadOnlyList<ISegment> segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            if (segments.Count == 0) // I prefer don't handle the empty tree
                segments = new List<ISegment> {new EmptySegment()};

            _segments = segments;

            _segmentIndexMap = new Dictionary<ISegment, int>(_segments.Count);
            for (var i = 0; i < _segments.Count; i++)
                _segmentIndexMap[_segments[i]] = i;
        }

        /// <summary>
        /// Method to find the next closest segment in the document that comes after.
        /// Walks tree up and then down.
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>
        /// Next segment
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public ISegment Next([NotNull] ISegment segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            var segmentIndex = _segmentIndexMap[segment];
            return segmentIndex == _segments.Count - 1 ? null : _segments[segmentIndex + 1];
        }

        /// <summary>
        /// Method to find the next closest segment in the document that comes before.
        /// Walks tree up and then down.
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>
        /// Next segment
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public ISegment Prev([NotNull] ISegment segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            var segmentIndex = _segmentIndexMap[segment];
            return segmentIndex == 0 ? null : _segments[segmentIndex - 1];
        }

        /// <summary>
        /// Gets the first segment of the document.
        /// </summary>
        [NotNull]
        public ISegment FirstSegment => _segments[0];
        /// <summary>
        /// Gets the last segment of the document.
        /// </summary>
        [NotNull]
        public ISegment LastSegment => _segments[_segments.Count - 1];

        /// <summary>
        /// Gets the segments count.
        /// </summary>
        public int SegmentsCount => _segments.Count;
    }
}
