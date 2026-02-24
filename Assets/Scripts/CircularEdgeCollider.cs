using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class CircularEdgeCollider : MonoBehaviour
{
    public int points = 64;
    public float radius = 0.5f;

    void Start()
    {
        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
        Vector2[] pointList = new Vector2[points + 1];

        for (int i = 0; i <= points; i++)
        {
            float angle = i * Mathf.PI * 2 / points;
            pointList[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        edge.points = pointList;
    }
}