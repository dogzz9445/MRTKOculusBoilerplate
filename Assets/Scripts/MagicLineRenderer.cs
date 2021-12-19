using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicLineRenderer : MonoBehaviour
{
    public GameObject lineDrawPrefabs; // this is where we put the prefabs object

    private bool isMousePressed;
    private GameObject lineDrawPrefab;
    private LineRenderer lineRenderer;
    public List<Vector3> drawPoints = new List<Vector3>();
    [SerializeField]
    public float distance = 10.0f;
    [SerializeField]
    public float lineWidth = 0.015f;

    // 1 / sqrt(2)
    const float distanceWeight = 0.7071067811865475f;

    // Use this for initialization
    void Start()
    {
        isMousePressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // delete the LineRenderers when right mouse down
            GameObject[] delete = GameObject.FindGameObjectsWithTag("DrawCanvas");
            int deleteCount = delete.Length;
            for (int i = deleteCount - 1; i >= 0; i--)
                Destroy(delete[i]);
        }

        if (Input.GetMouseButtonDown(0))
        {
            // left mouse down, make a new line renderer
            isMousePressed = true;
            lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
            lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // left mouse up, stop drawing
            isMousePressed = false;
            drawPoints.Clear();
        }

        if (isMousePressed)
        {
            // when the left mouse button pressed
            // continue to add vertex to line renderer
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
            if (!drawPoints.Contains(mousePos))
            {
                drawPoints.Add(mousePos);
                lineRenderer.positionCount = drawPoints.Count;
                lineRenderer.startWidth = lineWidth * distance * distanceWeight;
                lineRenderer.endWidth = lineWidth * distance * distanceWeight;
                lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
            }
        }
    }
}
