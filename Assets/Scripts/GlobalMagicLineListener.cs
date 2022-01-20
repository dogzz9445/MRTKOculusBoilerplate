using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Windows;
using WizardSystem;
using WizardSystem.Common;
using WizardSystem.Protobuf;

[RequireComponent(typeof(MagicLineDrawer))]
public class GlobalMagicLineListener : MonoBehaviour,
    IMixedRealityInputActionHandler,
    IMixedRealitySourceStateHandler, // Handle source detected and lost
    IMixedRealityHandJointHandler, // handle joint position updates for hands
    IMixedRealityFocusHandler,
    IMixedRealityPointerHandler
{

    public bool IsWizard = true;

    public bool IsOnFocusRightHand { get; private set; }
    public bool IsOnFocusLeftHand { get; private set; }
    public bool IsDrawingLine { get; private set; }

    private MagicLineDrawer magicLineDrawer;

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

    private void Awake()
    {
        IsOnFocusRightHand = false;
        IsOnFocusLeftHand = false;
        IsDrawingLine = false;
        magicLineDrawer = GetComponent<MagicLineDrawer>();
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
            else if (eventData.Handedness == MagicOperationHand)
            {
                eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out palmOperationPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.ThumbTip, out thumbOperationTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out indexOperationTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.MiddleTip, out middleOperationTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.RingTip, out ringOperationTipPose);
                eventData.InputData.TryGetValue(TrackedHandJoint.PinkyTip, out pinkyOperationTipPose);
            }
        }
    }

    public bool CalculateAnglePalmDirection(List<MixedRealityPose> tips, Vector3 direction, float angleThreshold)
    {
        foreach(var tip in tips)
        {
            if (Mathf.Abs(Vector3.SignedAngle(-tip.Up, direction, Vector3.up)) > angleThreshold)
            {
                return false;
            }
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
                magicLineDrawer.StartDrawing();
            }
        }
        #endregion

        //if (!IsOnFocusOperationHand)
        //{
        //    // 노말 명령
        //    if (IsNormalMagicReady)
        //    {
        //        if (!IsDrawingLine)
        //        {
        //            IsDrawingLine = true;
        //        }
        //    }

        //    // 
        //    if (IsSkillMagicReady)
        //    {
        //        if (!IsDrawingLine)
        //        {
        //            IsDrawingLine = true;
        //        }
        //    }
        //}
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(eventData.Pointer.Position, eventData.Pointer.Rays[0].Direction, out raycastHit))
            {
                magicLineDrawer.AddPoint(raycastHit.point);
            }
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (IsDrawingLine)
        {
            IsDrawingLine = false;
            var image = magicLineDrawer.EndDrawing();

            SaveImage(image);
            SendImage(image);
        }
    }

    public async void SaveImage(TextureImage image)
    {
        // 이미지 저장
        var uniqueFileName = FileGenerator.GetUniqueName("shape", Application.dataPath + "/../Shapes/", ".png");
        File.WriteAllBytes(Application.dataPath + "/../Shapes/" + uniqueFileName, image.GetRawImage());
        await Task.Yield();
    }

    public async void SendImage(TextureImage image)
    {
        Color32[] colors = image.Texture.GetPixels32();
        List<byte> uploadingBytes = new List<byte>();
        foreach (var color in colors)
        {
            uploadingBytes.Add((byte)(color.r * 255));
            uploadingBytes.Add((byte)(color.g * 255));
            uploadingBytes.Add((byte)(color.b * 255));
        }

        Image uploadingImage = new Image()
        {
            ImageData = ByteString.CopyFrom(uploadingBytes.ToArray()),
            Width = image.Texture.width,
            Height = image.Texture.height
        };
        Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        var client = new WizardService.WizardServiceClient(channel);
        Magic reply = await client.PostMagicImageRawAsync(uploadingImage);
        Debug.Log(MagicTypeConverter.GetMagicName(MagicTypeConverter.GetMagicType(int.Parse(reply.Type))));
        await channel.ShutdownAsync();
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

}
