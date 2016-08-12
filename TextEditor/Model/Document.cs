using System.Collections.Generic;

namespace TextEditor.Model
{
    /// <summary>
    /// Base presentation of text document
    /// For presentation the "Rope" model was choosen
    /// see <see cref="Segmentizer"/>
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Base class for the Rope node
        /// </summary>
        private class NodeBase
        {
            /// <summary>
            /// Total length of the text containing in subtree down the node
            /// </summary>
            public long Length { get; protected set; }

            /// <summary>
            /// Reference to the parent node
            /// </summary>
            public Node Parent { get; set; }
        }
        /// <summary>
        /// Class for the node that doesn't contains text and contains subtrees only
        /// </summary>
        private sealed class Node : NodeBase
        {
            /// <summary>
            /// Left child. Text that is close to the begin of the document
            /// </summary>
            public NodeBase LeftChild { get; }
            /// <summary>
            /// Right child. Text that is close to the end of the document
            /// </summary>
            public NodeBase RightChild { get; }

            public Node(NodeBase leftChild, NodeBase rightChild)
            {
                LeftChild = leftChild;
                LeftChild.Parent = this;

                RightChild = rightChild;
                RightChild.Parent = this;

                Length = LeftChild.Length + RightChild.Length;
            }
        }

        /// <summary>
        /// Class for the node that contains text only
        /// </summary>
        private sealed class SegmentLeaf : NodeBase
        {
            /// <summary>
            /// Segment reference
            /// </summary>
            public ISegment Segment { get; }

            public SegmentLeaf(ISegment segment)
            {
                Segment = segment;
                Length = segment.Length;
            }
        }

        /// <summary>
        /// Reference to the root of the Rope tree
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly NodeBase _root;

        /// <summary>
        /// Index to find the leaf node by the text segment
        /// </summary>
        private readonly Dictionary<ISegment, SegmentLeaf> _segmentNodesMap;

        public Document(List<ISegment> segments)
        {
            if (segments.Count == 0) // I prefer don't handle the empty tree
                segments = new List<ISegment> { new EditableSegment() };

            _segmentNodesMap = new Dictionary<ISegment, SegmentLeaf>();
            _root = BuildTree(segments, 0, segments.Count);
            _firstSegmentLeaf = LeftmostNode(_root);
            _lastSegmentLeaf = RightmostNode(_root);
        }

        /// <summary>
        ///     Recursive method for tree construction from the list of leafs.
        ///     I don't construct new array on every recursion step. Just manipulating indexes
        /// </summary>
        /// <param name="segments">The list of leafs</param>
        /// <param name="leftPosition">Offset of sublist</param>
        /// <param name="segmentsCount">Size of the sublist</param>
        /// <returns>The built node</returns>
        private NodeBase BuildTree(IReadOnlyList<ISegment> segments, int leftPosition, int segmentsCount)
        {
            if (segmentsCount == 1)
            { // Just Construct and return Leaf
                var segment = segments[leftPosition];
                var segmentNode = new SegmentLeaf(segment);
                _segmentNodesMap[segment] = segmentNode;
                return segmentNode;
            }
            // Cunstruct both subtrees
            var leftSize = segmentsCount / 2;
            return new Node(
                BuildTree(segments, leftPosition, leftSize),
                BuildTree(segments, leftPosition + leftSize, segmentsCount - leftSize));
        }

        /// <summary>
        ///     Method to find the deepest and leftest Leaf of the node.
        /// </summary>
        /// <param name="node">the node</param>
        /// <returns>Found Leaf</returns>
        private static SegmentLeaf LeftmostNode(NodeBase node)
        {
            var currentNode = node;
            while (currentNode is Node)
                currentNode = ((Node)currentNode).LeftChild;

            return (SegmentLeaf)currentNode;
        }

        /// <summary>
        ///     Method to find the deepest and rightest Leaf of the node.
        /// </summary>
        /// <param name="node">the node</param>
        /// <returns>Found Leaf</returns>
        private static SegmentLeaf RightmostNode(NodeBase node)
        {
            var currentNode = node;
            while (currentNode is Node)
                currentNode = ((Node)currentNode).RightChild;

            return (SegmentLeaf)currentNode;
        }

        /// <summary>
        ///     Method to find the next closest segment in the document that comes after.
        ///     Walks tree up and then down. 
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>Next segment</returns>
        public ISegment Next(ISegment segment)
        {
            var currentNode = (NodeBase)_segmentNodesMap[segment];
            var segmentParent = currentNode.Parent;
            while (segmentParent != null)
            {
                if (segmentParent.LeftChild == currentNode)
                    return LeftmostNode(segmentParent.RightChild).Segment;
                currentNode = segmentParent;
                segmentParent = currentNode.Parent;
            }
            return null;
        }

        /// <summary>
        ///     Method to find the next closest segment in the document that comes before.
        ///     Walks tree up and then down. 
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>Next segment</returns>
        public ISegment Prev(ISegment segment)
        {
            var currentNode = (NodeBase)_segmentNodesMap[segment];
            var segmentParent = currentNode.Parent;
            while (segmentParent != null)
            {
                if (segmentParent.RightChild == currentNode)
                    return RightmostNode(segmentParent.LeftChild).Segment;
                currentNode = segmentParent;
                segmentParent = currentNode.Parent;
            }
            return null;
        }

        private readonly SegmentLeaf _firstSegmentLeaf;
        private readonly SegmentLeaf _lastSegmentLeaf;
        public ISegment FirstSegment => _firstSegmentLeaf.Segment;
        public ISegment LastSegment => _lastSegmentLeaf.Segment;
        public int SegmentsCount => _segmentNodesMap.Count;
    }
}
