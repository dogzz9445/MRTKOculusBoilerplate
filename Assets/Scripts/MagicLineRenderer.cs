// Code from https://forum.unity.com/threads/easy-curved-line-renderer-free-utility.391219/
// and https://github.com/gpvigano/EasyCurvedLine

using System.Collections.Generic;
using UnityEngine;
using WizardSystem.Common;

namespace WizardSystem
{
    /// <summary>
    /// Render in 3D a curved line based on its control points.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class MagicLineRenderer : MonoBehaviour
    {
        const float DISTANCE_WEIGHT = 0.07071067811865475f;
        /// <summary>
        /// Size of line segments (in meters) used to approximate the curve.
        /// </summary>
        [Tooltip("Size of line segments (in meters) used to approximate the curve")]
        public float lineSegmentSize = 15.15f;

        /// <summary>
        /// Thickness of the line (initial thickness if useCustomEndWidth is true).
        /// </summary>
        [Tooltip("Width of the line (initial width if useCustomEndWidth is true)")]
        public float lineWidth = 0.1f;

        private float distanceLineWidth = 0.1f;

        /// <summary>
        /// Automatically update the line.
        /// </summary>
        [Tooltip("Automatically update the line.")]
        public bool autoUpdate = true;

        /// <summary>
        /// Allow editing width directly on curve graph.
        /// </summary>
        [Tooltip("Allow editing width directly on curve graph.")]
        public bool allowWidthEditOnCurveGraph = true;

        private List<Vector3> linePoints = new List<Vector3>();
        private Vector3[] linePointsOld = new Vector3[0];
        private Vector3[] smoothedPoints = new Vector3[0];
        private LineRenderer lineRenderer = null;

        private float oldLineWidth = 0.0f;
        private float oldEndWidth = 0.0f;
        private float oldLineRendererStartWidth = 0.0f;
        private float oldLineRendererEndWidth = 0.0f;


        public List<Vector3> LinePoints
        {
            get
            {
                return linePoints;
            }
        }


        /// <summary>
        /// Collect control points positions and update the line renderer.
        /// </summary>
        public void UpdateLineRenderer()
        {
            SetPointsToLine();
        }


        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }


        /// <summary>
        /// Collect control points positions and update the line renderer.
        /// </summary>
        public void Update()
        {
            if (autoUpdate)
            {
                UpdateLineRenderer();
            }
        }

        public void SetDistance(float distance)
        {
            distanceLineWidth = distance * DISTANCE_WEIGHT * lineWidth;
        }

        public void AddPoint(Vector3 point)
        {
            if (linePoints.Count == 0 || linePoints[linePoints.Count - 1] != point)
            {
               linePoints.Add(point);
            }
        }

        private void SetPointsToLine()
        {
            if (allowWidthEditOnCurveGraph)
            {
                // if the start width was edited directly on the curve
                if (oldLineWidth == distanceLineWidth && 
                    oldLineRendererStartWidth != lineRenderer.startWidth && 
                    oldLineRendererEndWidth != lineRenderer.endWidth)
                {
                    distanceLineWidth = lineRenderer.startWidth;
                }
            }

            // rebuild the line if any parameter was changed
            bool rebuild = (lineRenderer.startWidth != distanceLineWidth || lineRenderer.endWidth != distanceLineWidth);

            if (linePointsOld.Length != linePoints.Count)
            {
                linePointsOld = new Vector3[linePoints.Count];
                rebuild = true;
            }

            // update if line points were modified
            if (rebuild)
            {
                linePoints.CopyTo(linePointsOld, 0);
                if (lineRenderer == null)
                {
                    lineRenderer = GetComponent<LineRenderer>();
                }

                // get smoothed values
                smoothedPoints = LineSmoother.SmoothLine(linePoints.ToArray(), lineSegmentSize);

                Debug.Log("original size: " + linePoints.Count + " smooth : " + smoothedPoints.Length);
                lineRenderer.positionCount = smoothedPoints.Length;
                lineRenderer.SetPositions(smoothedPoints);
                lineRenderer.startWidth = distanceLineWidth;
                lineRenderer.endWidth = distanceLineWidth;

                oldLineWidth = distanceLineWidth;
                oldLineRendererStartWidth = lineRenderer.startWidth;
                oldLineRendererEndWidth = lineRenderer.endWidth;
            }
        }
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MagicLineRenderer : MonoBehaviour
//{
//    public GameObject lineDrawPrefabs; // this is where we put the prefabs object

//    private bool isMousePressed;
//    private GameObject lineDrawPrefab;
//    private LineRenderer lineRenderer;
//    public List<Vector3> drawPoints = new List<Vector3>();
//    [SerializeField]
//    public float distance = 10.0f;
//    [SerializeField]
//    public float lineWidth = 0.015f;

//    // 1 / sqrt(2)
//    const float distanceWeight = 0.7071067811865475f;

//    // Use this for initialization
//    void Start()
//    {
//        isMousePressed = false;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetMouseButtonDown(1))
//        {
//            // delete the LineRenderers when right mouse down
//            GameObject[] delete = GameObject.FindGameObjectsWithTag("DrawCanvas");
//            int deleteCount = delete.Length;
//            for (int i = deleteCount - 1; i >= 0; i--)
//                Destroy(delete[i]);
//        }

//        if (Input.GetMouseButtonDown(0))
//        {
//            // left mouse down, make a new line renderer
//            isMousePressed = true;
//            lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
//            lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
//            lineRenderer.positionCount = 0;
//        }
//        else if (Input.GetMouseButtonUp(0))
//        {
//            // left mouse up, stop drawing
//            isMousePressed = false;
//            drawPoints.Clear();
//        }

//        if (isMousePressed)
//        {
//            // when the left mouse button pressed
//            // continue to add vertex to line renderer
//            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
//            if (!drawPoints.Contains(mousePos))
//            {
//                drawPoints.Add(mousePos);
//                lineRenderer.positionCount = drawPoints.Count;
//                lineRenderer.startWidth = lineWidth * distance * distanceWeight;
//                lineRenderer.endWidth = lineWidth * distance * distanceWeight;
//                lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
//            }
//        }
//    }
//}
