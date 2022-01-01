using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common;
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

    public bool IsWizard = true;
    public bool IsDoctorStrange = false;

    public bool IsOnFocusRightHand { get; private set; }
    public bool IsOnFocusLeftHand { get; private set; }
    public bool IsDrawingLine { get; private set; }
    public bool IsDrawingDrStrangeCircle { get; private set; }

    public Camera FixedMainCamera = null;
    private TransformData FixedTransformData;
    public GameObject FixedPlane = null;
    public float FixedPlaneDistance = 5.5f;
    public LineRenderer FixedPlaneLineRenderer;
    public float FixedPlaneLineWidth = 0.015f;
    public Stack<Vector3> FixedPlaneDrawPoints = new Stack<Vector3>();
    public RaycastHit FixedPlaneRayCastHit;

    public List<Vector3> TwoDimentionalDrawPoints = new List<Vector3>();

    private MixedRealityPose palmPose;
    private MixedRealityPose thumbTipPose;
    private MixedRealityPose indexTipPose;
    private MixedRealityPose middleTipPose;
    private MixedRealityPose ringTipPose;
    private MixedRealityPose pinkyTipPose;


    private void Start()
    {
        IsOnFocusRightHand = false;
        IsOnFocusLeftHand = false;
        IsDrawingLine = false;
        IsDrawingDrStrangeCircle = false;
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
        if (IsDoctorStrange)
        {
            if (eventData.Handedness == Handedness.Left)
            {
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out palmPose))
                    return;
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out thumbTipPose))
                    return;
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out indexTipPose))
                    return;
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.MiddleTip, out middleTipPose))
                    return;
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.RingTip, out ringTipPose))
                    return;
                if (!eventData.InputData.TryGetValue(TrackedHandJoint.PinkyTip, out pinkyTipPose))
                    return;

                if (!IsDrawingDrStrangeCircle)
                {
                    var direction = Camera.main.transform.forward;
                    if (CalculateAnglePalmDirection(new List<MixedRealityPose>() { palmPose, thumbTipPose, indexTipPose, middleTipPose, ringTipPose, pinkyTipPose }, direction, 90.0f))
                    {
                        IsDrawingDrStrangeCircle = true;
                        StartCoroutine(StartDrStrangePortal(direction));
                    }
                }
            }
        }

        if (IsWizard)
        {
            if (eventData.Handedness == Handedness.Right)
            {
            }
        }
    }

    private IEnumerator StartDrStrangePortal(Vector3 direction)
    {
        //prefab.Instantiate()
        while (IsDrawingDrStrangeCircle)
        {
            if (CalculateAnglePalmDirection(new List<MixedRealityPose>() { palmPose, thumbTipPose, indexTipPose, middleTipPose, ringTipPose, pinkyTipPose }, direction, 90.0f))
            {
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                // destroy prefab
                IsDrawingDrStrangeCircle = false;
            }
        }
    }

    public bool CalculateAnglePalmDirection(List<MixedRealityPose> tips, Vector3 direction, float angleThreshold)
    {
        foreach(var tip in tips)
        {
            if (Mathf.Abs(Vector3.SignedAngle(-tip.Up, direction, Vector3.up)) > angleThreshold)
                return false;
        }
        return true;
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
                // 1
                FixedTransformData = new TransformData(Camera.main.transform);
                //FixedPlane.transform.Translate(FixedTransformData.LocalPosition + FixedTransformData.LocalEulerAngles * FixedPlaneDistance);
                // 2
                FixedPlane.transform.Translate(FixedMainCamera.ScreenToWorldPoint(new Vector3(FixedMainCamera.pixelWidth / 2.0f, FixedMainCamera.pixelHeight / 2.0f, FixedPlaneDistance)));
                FixedPlane.transform.forward = FixedMainCamera.transform.forward;
                FixedPlane.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
                LineDrawObject = Instantiate(LineDrawPrefab) as GameObject;
                FixedPlaneLineRenderer = LineDrawObject.GetComponent<LineRenderer>();
                FixedPlaneLineRenderer.positionCount = 0;
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

            // 
            TwoDimentionalDrawPoints.Clear();
            foreach (var worldPoint in FixedPlaneDrawPoints)
            {
                TwoDimentionalDrawPoints.Add(FixedMainCamera.WorldToScreenPoint(worldPoint));
            }
            FixedPlaneDrawPoints.Clear();

            TextureImage image = new TextureImage();
            image.DrawLines(TwoDimentionalDrawPoints);

            var uniqueFileName = FileGenerator.GetUniqueName("shape", Application.dataPath + "/../Shapes/", ".png");
            File.WriteAllBytes(Application.dataPath + "/../Shapes/" + uniqueFileName, image.GetRawImage());
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

}
