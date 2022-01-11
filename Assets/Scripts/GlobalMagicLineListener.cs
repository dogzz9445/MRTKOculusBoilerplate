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
using WizardSystem;

public class GlobalMagicLineListener : MonoBehaviour,
    IMixedRealityInputActionHandler,
    IMixedRealitySourceStateHandler, // Handle source detected and lost
    IMixedRealityHandJointHandler, // handle joint position updates for hands
    IMixedRealityFocusHandler,
    IMixedRealityPointerHandler
{
    public GameObject LineDrawPrefab;
    private GameObject LineDrawObject;
    public GameObject FixedPlanePrefab;

    public bool IsWizard = true;

    public bool IsOnFocusRightHand { get; private set; }
    public bool IsOnFocusLeftHand { get; private set; }
    public bool IsDrawingLine { get; private set; }
    public bool IsDrawingDrStrangeCircle { get; private set; }

    public MagicLineRenderer magicLineRenderer;
    public Camera FixedMainCamera = null;
    public GameObject FixedPlane = null;
    public float FixedPlaneDistance = 5.5f;

    private TransformData FixedTransformData;

    //
    // Hand poses
    //

    private Handedness MagicReadyHand = Handedness.Left;
    private Handedness MagicOperationHand = Handedness.Right;

    private MixedRealityPose palmReadyPose;
    private MixedRealityPose thumbReadyTipPose;
    private MixedRealityPose indexReadyTipPose;
    private MixedRealityPose middleReadyTipPose;
    private MixedRealityPose ringReadyTipPose;
    private MixedRealityPose pinkyReadyTipPose;

    private List<MixedRealityPose> GetMagicReadyHandPoses()
    {
        return new List<MixedRealityPose> { palmReadyPose, thumbReadyTipPose, indexReadyTipPose, middleReadyTipPose, ringReadyTipPose, pinkyReadyTipPose };
    }

    private MixedRealityPose palmOperationPose;
    private MixedRealityPose thumbOperationTipPose;
    private MixedRealityPose indexOperationTipPose;
    private MixedRealityPose middleOperationTipPose;
    private MixedRealityPose ringOperationTipPose;
    private MixedRealityPose pinkyOperationTipPose;

    private List<MixedRealityPose> GetMagicOperationHandPoses()
    {
        return new List<MixedRealityPose> { palmOperationPose, thumbOperationTipPose, indexOperationTipPose, middleOperationTipPose, ringOperationTipPose, pinkyOperationTipPose };
    }

    private Vector3 MainCameraForward { get => Camera.main.transform.forward; }
    private Vector3 MainCameraBackward { get => -Camera.main.transform.forward; }
    private Vector3 LookDown { get => Vector3.down; }
    private Vector3 LookUp { get => Vector3.up; }

    private bool IsMagicReadyHandLookForward { get => CalculateAnglePalmDirection(GetMagicReadyHandPoses(), MainCameraForward, 90.0f); }
    private bool IsMagicReadyHandLookBackward { get => CalculateAnglePalmDirection(GetMagicReadyHandPoses(), MainCameraBackward, 90.0f); }
    private bool IsMagicReadyHandLookDown { get => CalculateAnglePalmDirection(GetMagicReadyHandPoses(), LookDown, 90.0f);  }
    private bool IsMagicReadyHandLookUp { get => CalculateAnglePalmDirection(GetMagicReadyHandPoses(), LookUp, 90.0f);  }

    public bool IsNormalMagicReady { get => IsMagicReadyHandLookDown && IsMagicReadyHandLookForward; }
    public bool IsSkillMagicReady { get => IsMagicReadyHandLookUp && IsMagicReadyHandLookBackward; }

    public bool IsOnFocusOperationHand
    {
        get 
        {
            if (MagicOperationHand == Handedness.Right)
            {
                if (IsOnFocusRightHand)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else // (MagicOperationHand == Handedness.Left)
            {
                if (IsOnFocusRightHand)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
    }

    private void Start()
    {
        IsOnFocusRightHand = false;
        IsOnFocusLeftHand = false;
        IsDrawingLine = false;
        IsDrawingDrStrangeCircle = false;
        FixedMainCamera = new GameObject("FixedMainCamera").AddComponent<Camera>();
        FixedMainCamera.targetDisplay = 2;
        FixedMainCamera.cameraType = CameraType.Preview;
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

    private void Update()
    {
        // TODO:
        // 그리고 있을때 시간에 따라서 주기적으로 스무딩
        // 뺄수있으면 코루틴으로
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

    public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
    {
        if (IsWizard)
        {
            if (eventData.Handedness == MagicReadyHand)
            {
                eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out palmReadyPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out thumbReadyTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out indexReadyTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.MiddleTip, out middleReadyTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.RingTip, out ringReadyTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.PinkyTip, out pinkyReadyTipPose);
            }
        }
    }

    private IEnumerator StartDrStrangePortal(Vector3 direction)
    {
        //prefab.Instantiate()
        while (IsDrawingDrStrangeCircle)
        {
            if (IsNormalMagicReady)
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
        // 테스트 소스
        // 배포 시 제거
        #region test
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
                magicLineRenderer = LineDrawObject.GetComponent<MagicLineRenderer>();
                magicLineRenderer.SetDistance(FixedPlaneDistance);
            }
        }
        #endregion

        if (!IsOnFocusOperationHand)
        {
            // 노말 명령
            if (IsNormalMagicReady)
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
                    magicLineRenderer = LineDrawObject.GetComponent<MagicLineRenderer>();
                    magicLineRenderer.SetDistance(FixedPlaneDistance);
                }
            }

            // 
            if (IsSkillMagicReady)
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
                    magicLineRenderer = LineDrawObject.GetComponent<MagicLineRenderer>();
                    magicLineRenderer.SetDistance(FixedPlaneDistance);
                }
            }
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(eventData.Pointer.Position, eventData.Pointer.Rays[0].Direction, out raycastHit))
            {
                magicLineRenderer.AddPoint(raycastHit.point);
            }
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            IsDrawingLine = false;

        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

    public bool IsMagicReadyHand(IMixedRealityPointer pointer)
    {
        if (MagicReadyHand == Handedness.Right)
        {
            if (pointer.PointerName.StartsWith("Right"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (MagicReadyHand == Handedness.Left)
        {
            if (pointer.PointerName.StartsWith("Left"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public bool IsMagicOperationHand(IMixedRealityPointer pointer)
    {
        if (MagicOperationHand == Handedness.Right)
        {
            if (pointer.PointerName.StartsWith("Right"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (MagicOperationHand == Handedness.Left) // (MagicReadyHand == Handedness.Left)
        {
            if (pointer.PointerName.StartsWith("Left"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }


    public void SaveMagicCircle()
    {
        //if (!IsDrawingDrStrangeCircle)
        //{
        //    var direction = Camera.main.transform.forward;
        //    if (CalculateAnglePalmDirection(new List<MixedRealityPose>() { palmPose, thumbTipPose, indexTipPose, middleTipPose, ringTipPose, pinkyTipPose }, direction, 90.0f))
        //    {
        //        IsDrawingDrStrangeCircle = true;
        //        StartCoroutine(StartDrStrangePortal(direction));
        //    }
        //}

        //TwoDimentionalDrawPoints.Clear();
        //foreach (var worldPoint in FixedPlaneDrawPoints)
        //{
        //    TwoDimentionalDrawPoints.Add(FixedMainCamera.WorldToScreenPoint(worldPoint));
        //}
        //FixedPlaneDrawPoints.Clear();

        //TextureImage image = new TextureImage();
        //image.DrawLines(TwoDimentionalDrawPoints);

        //var uniqueFileName = FileGenerator.GetUniqueName("shape", Application.dataPath + "/../Shapes/", ".png");
        //File.WriteAllBytes(Application.dataPath + "/../Shapes/" + uniqueFileName, image.GetRawImage());
    }
}
