using UnityEngine;

public class LineRendererArray
{
    Transform parent_;
    GameObject[] gameObjectLineArray_;
    GameObject[] gameObjectColumnArray_;
    LineRenderer[] lineRenderersLineArray_;
    LineRenderer[] lineRenderersColumnArray_;

    public LineRendererArray(Transform parent)
    {
        parent_ = parent;
    }

    public void Init(int line, int column)
    {
        gameObjectLineArray_ = new GameObject[line];
        lineRenderersLineArray_ = new LineRenderer[line];
        for (int i=0; i<line; i++)
        {
            var obj = new GameObject("GridLine");
            obj.transform.SetParent(parent_);
            var lineRenderer = obj.AddComponent<LineRenderer>();
            lineRenderer.startColor = Color.gray;
            lineRenderer.endColor = Color.gray;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderersLineArray_[i] = lineRenderer;
            gameObjectLineArray_[i] = obj;
        }

        gameObjectColumnArray_ = new GameObject[column];
        lineRenderersColumnArray_ = new LineRenderer[column];
        for (int i=0; i<column; i++)
        {
            var obj = new GameObject("GridColumn");
            obj.transform.SetParent(parent_);
            var lineRenderer = obj.AddComponent<LineRenderer>();
            lineRenderer.startColor = Color.gray;
            lineRenderer.endColor = Color.gray;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderersColumnArray_[i] = lineRenderer;
            gameObjectColumnArray_[i] = obj;   
        }
    }

    public void Uninit()
    {

    }

    public LineRenderer GetLineRendererWithLine(int line)
    {
        return lineRenderersLineArray_[line];
    }

    public LineRenderer GetLineRendererWithColumn(int column)
    {
        return lineRenderersColumnArray_[column];
    }
}