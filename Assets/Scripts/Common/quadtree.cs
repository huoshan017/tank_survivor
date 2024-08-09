using Common.Geometry;

namespace Common
{
    public class QuadTree
    {
        public QuadTree()
        {

        }

        public void Init(Rect rect)
        {
            rect_ = rect;
        }

        public int Insert(Rect rect)
        {
            idCounter_ += 1;
            return idCounter_;
        }

        public bool Remove(int id)
        {
            return true;
        }

        Rect rect_;
        QuadTree[] children_;
        MapListCombo<int, Rect> rectList_;
        int idCounter_;
    }
}