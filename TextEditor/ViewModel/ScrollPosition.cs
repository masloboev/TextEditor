using TextEditor.Model;

namespace TextEditor.ViewModel
{

    public struct ScrollPosition
    {
        public ISegment FirstSegment;
        public int RowsBeforeScrollCount;
    }
}
