// Code from https://forum.unity.com/threads/easy-curved-line-renderer-free-utility.391219/
// and https://github.com/gpvigano/EasyCurvedLine

using System.Collections.Generic;
using UnityEngine;
using Common;

namespace WizardSystem
{
    /// <summary>
    /// Render in 3D a curved line based on its control points.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class CurvedLineRenderer : MonoBehaviour
    {
        /// <summary>
        /// Size of line segments (in meters) used to approximate the curve.
        /// </summary>
        [Tooltip("Size of line segments (in meters) used to approximate the curve")]
        public float lineSegmentSize = 0.15f;

        /// <summary>
        /// Thickness of the line (initial thickness if useCustomEndWidth is true).
        /// </summary>
        [Tooltip("Width of the line (initial width if useCustomEndWidth is true)")]
        public float lineWidth = 0.1f;

        /// <summary>
        /// Use a different thickness for the line end.
        /// </summary>
        [Tooltip("Enable this to set a custom width for the line end")]
        public bool useCustomEndWidth = false;

        /// <summary>
        /// Thickness of the line at its end point (initial thickness is lineWidth).
        /// </summary>
        [Tooltip("Custom width for the line end")]
        public float endWidth = 0.1f;

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

        [Header("Gizmos")]

        /// <summary>
        /// Show gizmos at control points in Unity Editor.
        /// </summary>
        [Tooltip("Show gizmos at control points.")]
        public bool showGizmos = true;

        /// <summary>
        /// Size of the gizmos of control points.
        /// </summary>
        [Tooltip("Size of the gizmos of control points.")]
        public float gizmoSize = 0.1f;

        /// <summary>
        /// Color for rendering the gizmos of control points.
        /// </summary>
        [Tooltip("Color for rendering the gizmos of control points.")]
        public Color gizmoColor = new Color(1, 0, 0, 0.5f);

        private Vector3[] linePoints = new Vector3[0];
        private Vector3[] linePositions = new Vector3[0];
        private Vector3[] linePositionsOld = new Vector3[0];
        private LineRenderer lineRenderer = null;
        private Material lineRendererMaterial = null;

        private float oldLineWidth = 0.0f;
        private float oldEndWidth = 0.0f;
        private float oldLineRendererStartWidth = 0.0f;
        private float oldLineRendererEndWidth = 0.0f;


        public Vector3[] LinePoints
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
            GetPoints();
            SetPointsToLine();
            UpdateMaterial();
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


        private void GetPoints()
        {
            // find curved points in children
            // scan only the first hierarchy level to allow nested curved lines (like modelling a tree or a coral)
            List<Vector3> curvedLinePoints = new List<Vector3>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 childPoint = transform.GetChild(i).GetComponent<Vector3>();
                if (childPoint != null)
                {
                    curvedLinePoints.Add(childPoint);
                }
            }
            linePoints = curvedLinePoints.ToArray();

            //add positions
            if (linePositions.Length != linePoints.Length)
            {
                linePositions = new Vector3[linePoints.Length];
            }
            for (int i = 0; i < linePoints.Length; i++)
            {
                linePositions[i] = linePoints[i];
            }
        }


        private void SetPointsToLine()
        {
            if (allowWidthEditOnCurveGraph)
            {
                // if the start width was edited directly on the curve
                if (oldLineWidth == lineWidth && oldLineRendererStartWidth != lineRenderer.startWidth)
                {
                    lineWidth = lineRenderer.startWidth;
                }

                // if the end width was edited directly on the curve
                if (oldEndWidth == endWidth && oldLineRendererEndWidth != lineRenderer.endWidth)
                {
                    endWidth = lineRenderer.endWidth;
                    if (endWidth != lineWidth)
                    {
                        useCustomEndWidth = true;
                    }
                }
            }

            float actualEndWidth = useCustomEndWidth ? endWidth : lineWidth;

            // rebuild the line if any parameter was changed
            bool rebuild = (lineRenderer.startWidth != lineWidth || lineRenderer.endWidth != actualEndWidth);

            if (!rebuild)
            {
                // create old positions if they don't match
                if (linePositionsOld.Length != linePositions.Length)
                {
                    linePositionsOld = new Vector3[linePositions.Length];
                    rebuild = true;
                }
                else
                {
                    // check if line points have moved
                    for (int i = 0; i < linePositions.Length; i++)
                    {
                        //compare
                        if (linePositions[i] != linePositionsOld[i])
                        {
                            rebuild = true;
                            break;
                        }
                    }
                }
            }

            // update if line points were modified
            if (rebuild)
            {
                linePositions.CopyTo(linePositionsOld, 0);
                if (lineRenderer == null)
                {
                    lineRenderer = GetComponent<LineRenderer>();
                }

                // get smoothed values
                Vector3[] smoothedPoints = LineSmoother.SmoothLine(linePositions, lineSegmentSize);

                // set line settings
                lineRenderer.positionCount = smoothedPoints.Length;
                lineRenderer.SetPositions(smoothedPoints);
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = useCustomEndWidth ? endWidth : lineWidth;

                oldLineWidth = lineWidth;
                oldEndWidth = endWidth;
                oldLineRendererStartWidth = lineRenderer.startWidth;
                oldLineRendererEndWidth = lineRenderer.endWidth;
            }
        }


        private void OnDrawGizmosSelected()
        {
            UpdateLineRenderer();
        }


        private void OnDrawGizmos()
        {
            if (linePoints.Length == 0)
            {
                GetPoints();
            }

            //// settings for gizmos
            //foreach (CurvedLinePoint linePoint in linePoints)
            //{
            //    linePoint.showGizmo = showGizmos;
            //    linePoint.gizmoSize = gizmoSize;
            //    linePoint.gizmoColor = gizmoColor;
            //}
        }


        private void UpdateMaterial()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            Material lineMaterial = lineRenderer.sharedMaterial;
            if (lineRendererMaterial != lineMaterial)
            {
                if (lineMaterial != null)
                {
                    lineRenderer.generateLightingData = !lineMaterial.shader.name.StartsWith("Unlit");
                }
                else
                {
                    lineRenderer.generateLightingData = false;
                }
            }
            lineRendererMaterial = lineMaterial;
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
