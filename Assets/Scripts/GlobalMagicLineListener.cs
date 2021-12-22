using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Windows;

public class GlobalMagicLineListener : MonoBehaviour,
    IMixedRealityInputActionHandler,
    IMixedRealitySourceStateHandler, // Handle source detected and lost
    IMixedRealityHandJointHandler, // handle joint position updates for hands
    IMixedRealityFocusHandler,
    IMixedRealityPointerHandler
{
    const float DISTANCE_WEIGHT = 0.7071067811865475f;

    public GameObject LineDrawPrefab;
    private GameObject LineDrawObject;
    public GameObject FixedPlanePrefab;

    public bool IsOnFocusRightHand { get; private set; }
    public bool IsOnFocusLeftHand { get; private set; }
    public bool IsDrawingLine { get; private set; }

    public Camera FixedMainCamera = null;
    public GameObject FixedPlane = null;
    public float FixedPlaneDistance = 5.5f;
    public LineRenderer FixedPlaneLineRenderer;
    public float FixedPlaneLineWidth = 0.015f;
    public Stack<Vector3> FixedPlaneDrawPoints = new Stack<Vector3>();
    public RaycastHit FixedPlaneRayCastHit;

    public List<Vector3> TwoDimentionalDrawPoints = new List<Vector3>();

    private void Start()
    {
        IsOnFocusRightHand = false;
        IsOnFocusLeftHand = false;
        IsDrawingLine = false;
        FixedMainCamera = new GameObject("FixedMainCamera").AddComponent<Camera>();
        FixedMainCamera.targetDisplay = 2;
    }

    private void OnEnable()
    {
        // Instruct Input System that we would like to receive all input events of type
        // IMixedRealitySourceStateHandler and IMixedRealityHandJointHandler
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    private void OnDisable()
    {
        // This component is being destroyed
        // Instruct the Input System to disregard us for input event handling
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityFocusHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    // IMixedRealitySourceStateHandler interface
    public void OnSourceDetected(SourceStateEventData eventData)
    {
        var hand = eventData.Controller as IMixedRealityHand;

        // Only react to articulated hand input sources
        if (hand != null)
        {
            Debug.Log("Source detected: " + hand.ControllerHandedness);
        }
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        var hand = eventData.Controller as IMixedRealityHand;

        // Only react to articulated hand input sources
        if (hand != null)
        {
            Debug.Log("Source lost: " + hand.ControllerHandedness);
        }
    }

    public void OnHandJointsUpdated(
        InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
    {
        //MixedRealityPose palmPose;
        //if (eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out palmPose))
        //{
        //    Debug.Log("Hand Joint Palm Updated: " + palmPose.Position);
        //}
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {
        Debug.Log(eventData.InputSource.SourceName + " " +eventData.MixedRealityInputAction.Description);
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
        Debug.Log(eventData.InputSource.SourceName + " " + eventData.MixedRealityInputAction.Description);
    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        if (eventData.Pointer.PointerName.StartsWith("Right"))
        {
            IsOnFocusRightHand = true;
        }
        else if (eventData.Pointer.PointerName.StartsWith("Left"))
        {
            IsOnFocusLeftHand = true;
        }
    }

    public void OnFocusExit(FocusEventData eventData)
    {
        if (eventData.Pointer.PointerName.StartsWith("Right"))
        {
            IsOnFocusRightHand = false;
        }
        else if (eventData.Pointer.PointerName.StartsWith("Left"))
        {
            IsOnFocusLeftHand = false;
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (!IsOnFocusRightHand)
        {
            if (!IsDrawingLine)
            {
                IsDrawingLine = true;
                FixedMainCamera.CopyFrom(Camera.main);
                FixedMainCamera.targetDisplay = 2;
                FixedPlane = Instantiate(FixedPlanePrefab);
                FixedPlane.transform.Translate(FixedMainCamera.ScreenToWorldPoint(new Vector3(FixedMainCamera.pixelWidth / 2.0f, FixedMainCamera.pixelHeight / 2.0f, FixedPlaneDistance)));
                FixedPlane.transform.forward = FixedMainCamera.transform.forward;
                FixedPlane.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
                LineDrawObject = Instantiate(LineDrawPrefab) as GameObject;
                FixedPlaneLineRenderer = LineDrawObject.GetComponent<LineRenderer>();
                FixedPlaneLineRenderer.positionCount = 0;
            }

        }
        if (!IsOnFocusLeftHand)
        {
            if (!IsDrawingLine)
            {
                IsDrawingLine = true;
                FixedMainCamera.CopyFrom(Camera.main);
            }
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            if (Physics.Raycast(eventData.Pointer.Position, eventData.Pointer.Rays[0].Direction, out FixedPlaneRayCastHit))
            {
                if (FixedPlaneDrawPoints.Count == 0 || FixedPlaneDrawPoints.Peek() != FixedPlaneRayCastHit.point)
                {
                    DrawPoint(FixedPlaneRayCastHit.point);
                }
            }
        }
    }

    public void DrawPoint(Vector3 pointPosition)
    {
        FixedPlaneDrawPoints.Push(pointPosition);
        FixedPlaneLineRenderer.positionCount = FixedPlaneDrawPoints.Count;
        FixedPlaneLineRenderer.startWidth = FixedPlaneLineWidth * FixedPlaneDistance * DISTANCE_WEIGHT;
        FixedPlaneLineRenderer.endWidth = FixedPlaneLineWidth * FixedPlaneDistance * DISTANCE_WEIGHT;
        FixedPlaneLineRenderer.SetPosition(FixedPlaneDrawPoints.Count - 1, pointPosition);
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            IsDrawingLine = false;

            TwoDimentionalDrawPoints.Clear();
            foreach (var worldPoint in FixedPlaneDrawPoints)
            {
                TwoDimentionalDrawPoints.Add(FixedMainCamera.WorldToScreenPoint(worldPoint));
            }
            FixedPlaneDrawPoints.Clear();

            Texture2D texture = new Texture2D(300, 300);
            var fillColor = Color.white;
            var fillColorArray = texture.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = fillColor;
            }

            texture.SetPixels(fillColorArray);

            // if width < 300, height < 300
            Vector2 maxPoint = new Vector2(TwoDimentionalDrawPoints.Max(x => x.x), TwoDimentionalDrawPoints.Max(x => x.y));
            Vector2 minPoint = new Vector2(TwoDimentionalDrawPoints.Min(x => x.x), TwoDimentionalDrawPoints.Min(x => x.y));
            float originalWidth = maxPoint.x - minPoint.x;
            float originalHeight = maxPoint.y - minPoint.y;

            var normalizedTwoDimentionalDrawPoints = TwoDimentionalDrawPoints
                .Select(point => new Vector2((280 * (point.x - minPoint.x) / originalWidth) + 10, (280 * (point.y - minPoint.y) / originalHeight) + 10))
                .ToList();

            for (int i = 0; i < normalizedTwoDimentionalDrawPoints.Count - 1; i++)
            {
                DrawLineOnTexture(texture, normalizedTwoDimentionalDrawPoints[i], normalizedTwoDimentionalDrawPoints[i + 1], Color.black);
            }

            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();

            var uniqueFileName = GetUniqueName("shape", Application.dataPath + "/../Shapes/", ".png");
            File.WriteAllBytes(Application.dataPath + "/../Shapes/" + uniqueFileName, bytes);
        }
    }

    public void DrawLineOnTexture(Texture2D texture, Vector2 p1, Vector2 p2, Color color)
    {
        Vector2 drawPoint = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)drawPoint.x != (int)p2.x || (int)drawPoint.y != (int)p2.y)
        {
            drawPoint = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            texture.SetPixel((int)drawPoint.x, (int)drawPoint.y, color);
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

    private string GetUniqueName(string name, string folderPath, string extension)
    {
        string validatedName = name + extension;
        int tries = 1;
        while (File.Exists(folderPath + validatedName))
        {
            validatedName = string.Format("{0}_{1:00000}{2}", name, tries++, extension);
        }
        return validatedName;
    }
}
