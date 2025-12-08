using UnityEngine;

public class PathDataObejctMono : MonoBehaviour
{
    public TextMesh textMesh;
    private int m_index = 0;
    private Vector2Int m_position;

    public void SetPathData(int index, Vector2Int position)
    {
        m_index = index;
        m_position = position;
        textMesh.text = index.ToString();

        transform.position = new(m_position.x, m_position.y, 0);
        gameObject.SetActive(true);
    }

    public void SetIndex(int index)
    {
        m_index = index;
        textMesh.text = index.ToString();
    }

    public Vector2Int PathPos => m_position;
}
