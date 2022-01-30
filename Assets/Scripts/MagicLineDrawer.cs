using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardSystem.Common;

namespace WizardSystem
{
    public class MagicLineDrawer : MonoBehaviour
    {
        public GameObject LineDrawPrefab;
        public GameObject FixedPlanePrefab;

        private GameObject FixedPlane = null;
        private MagicLineRenderer magicLineRenderer;

        [SerializeField]
        public float FixedPlaneDistance = 5.5f;

        private GameObject LineDrawObject;
        private Matrix4x4 cameraToWorldMatrix;
        private Matrix4x4 worldToCameraMatrix;

        private void Start()
        {
        }

        public void StartDrawing()
        {
            FixedPlane = Instantiate(FixedPlanePrefab);
            cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
            worldToCameraMatrix = Camera.main.worldToCameraMatrix;
            FixedPlane.transform.Translate(cameraToWorldMatrix.MultiplyVector(new Vector3(0, 0, -FixedPlaneDistance)));
            FixedPlane.transform.forward = Camera.main.transform.forward;
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
            List<Vector3> DrawPoints2D = new List<Vector3>();
            foreach (var worldPoint in magicLineRenderer.LinePoints)
            {
                DrawPoints2D.Add(worldToCameraMatrix.MultiplyPoint(worldPoint) * 300);
            }

            StartCoroutine(DestroySlowly(magicLineRenderer.gameObject, FixedPlane.gameObject));

            TextureImage image = new TextureImage();
            image.DrawLines(DrawPoints2D);
            return image;
        }

        private IEnumerator DestroySlowly(GameObject lineRenderer, GameObject plane)
        {
            yield return new WaitForSeconds(5);
            Destroy(lineRenderer);
            Destroy(plane);
        }


        //public bool IsDrawingDrStrangeCircle { get; private set; }
        //private IEnumerator StartDrStrangePortal(Vector3 direction)
        //{
        //    //prefab.Instantiate()
        //    while (IsDrawingDrStrangeCircle)
        //    {
        //        if (IsNormalMagicReady)
        //        {
        //            yield return new WaitForSeconds(1.0f);
        //        }
        //        else
        //        {
        //            // destroy prefab
        //            IsDrawingDrStrangeCircle = false;
        //        }
        //    }
        //}

    }
}
