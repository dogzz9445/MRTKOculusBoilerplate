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

        private IEnumerator DestroySlowly(GameObject lineRendererObject, GameObject plane)
        {
            Destroy(plane);
            yield return new WaitForSeconds(5);
            var lineRenderer = lineRendererObject.GetComponent<LineRenderer>();

            float fadeOutSpeed = 0.0f;
            while (fadeOutSpeed < 2.0f)
            {
                fadeOutSpeed += Time.deltaTime;
                Color color = Color.Lerp(new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(0f, 0f, 0f, 0f), fadeOutSpeed);
                lineRenderer.materials[0].SetColor("_TintColor", color);
            }
            Destroy(lineRendererObject);
        }

        // TODO: 
        //IEnumerator FadeLineRenderer()
        //{
        //    Gradient lineRendererGradient = new Gradient();
        //    float fadeSpeed = 3f;
        //    float timeElapsed = 0f;
        //    float alpha = 1f;

        //    while (timeElapsed < fadeSpeed)
        //    {
        //        alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeSpeed);

        //        lineRendererGradient.SetKeys
        //        (
        //            lineRenderer.colorGradient.colorKeys,
        //            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1f) }
        //        );
        //        lineRenderer.colorGradient = lineRendererGradient;

        //        timeElapsed += Time.deltaTime;
        //        yield return null;
        //    }

        //    Destroy(gameObject);
        //}

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
