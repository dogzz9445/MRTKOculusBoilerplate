using System.Collections.Generic;
using UnityEngine;
using WizardSystem.Common;

namespace WizardSystem
{
    public class MagicLineDrawer : MonoBehaviour
    {
        public GameObject LineDrawPrefab;
        private GameObject LineDrawObject;
        public GameObject FixedPlanePrefab;

        public Camera FixedMainCamera = null;
        public GameObject FixedPlane = null;
        public MagicLineRenderer magicLineRenderer;
        public TransformData FixedTransformData;

        public float FixedPlaneDistance = 5.5f;

        public MagicLineDrawer()
        {
            FixedMainCamera = new GameObject("FixedMainCamera").AddComponent<Camera>();
            FixedMainCamera.targetDisplay = 2;
            FixedMainCamera.cameraType = CameraType.Preview;
        }

        public void StartDrawing()
        {
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

        public void AddPoint(Vector3 point)
        {
            magicLineRenderer.AddPoint(point);
        }

        public TextureImage EndDrawing()
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
            List<Vector3> DrawPoints2D = new List<Vector3>();

            DrawPoints2D.Clear();
            foreach (var worldPoint in magicLineRenderer.LinePoints)
            {
                DrawPoints2D.Add(FixedMainCamera.WorldToScreenPoint(worldPoint));
            }
            magicLineRenderer.LinePoints.Clear();

            TextureImage image = new TextureImage();
            image.DrawLines(DrawPoints2D);
            return image;
        }
    }
}
