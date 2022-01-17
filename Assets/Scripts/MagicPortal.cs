using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using WizardSystem.Common;

[ExecuteInEditMode]
public class MagicPortal : MonoBehaviour
{
    // ºñÁê¾ó ÀÌÆåÆ®
    public VisualEffect m_visualEffect;
    public TransformData m_transformData;
    public float m_radius;
    public float m_maxRadius;

    // Æ÷Å»
    [field: SerializeField]
    public MagicPortal OtherPortal { get; private set; }

    [SerializeField]
    private Renderer outllineRenderer;

    [field: SerializeField]
    public Color PortalColour { get; private set; }

    [SerializeField]
    private LayerMask placementMask;

    [SerializeField]
    private Transform testTransform;

    public bool IsPlaced { get; private set; } = false;
    private Collider wallCollider;

    public Renderer Renderer { get; private set; }
    private new BoxCollider collider;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        Renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(StartMagicPortal());
    }

    private IEnumerator StartMagicPortal()
    {
        float targetInterval = 2.0f;
        float m = 0.0f;
        m_radius = 0.0f;
        while (m_radius != m_maxRadius)
        {
            m += Time.deltaTime;
            float curInterval = Math.Min(m / targetInterval, 1.0f);
            m_radius = Mathf.Lerp(0.0f, m_maxRadius, curInterval);
            yield return Task.Yield();
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

        //Renderer.enabled = OtherPortal.IsPlaced;

        //for (int i = 0; i < portalObjects.Count; ++i)
        //{
        //    Vector3 objPos = transform.InverseTransformPoint(portalObjects[i].transform.position);

        //    if (objPos.z > 0.0f)
        //    {
        //        portalObjects[i].Warp();
        //    }
        //}

        if (m_visualEffect == null)
        {
            m_visualEffect = GetComponent<VisualEffect>();
            if (m_visualEffect == null)
            {
                return;
            }
        }

        if (!m_transformData.Compare(transform))
        {
            m_transformData = new TransformData(transform);
            m_visualEffect.SetVector3("CenterPosition", transform.localPosition);
            m_visualEffect.SetVector3("CenterForward", transform.forward);
        }

        m_visualEffect.SetFloat("Radius", m_radius);
    }

    private void OnTriggerEnter(Collider other)
    {
        //var obj = other.GetComponent<PortalableObject>();
        //if (obj != null)
        //{
        //    portalObjects.Add(obj);
        //    obj.SetIsInPortal(this, OtherPortal, wallCollider);
        //}
    }

    public bool PlacePortal(Collider wallColider, Vector3 pos, Quaternion rot)
    {
        testTransform.position = pos;
        testTransform.rotation = rot;
        testTransform.position -= testTransform.forward * 0.001f;

        FixOverhangs();
        //FixIntersects();

        //if (CheckOverlap())
        //{
        //    this.wallCollider = wallCollider;
        //    transform.position = testTransform.position;
        //    transform.rotation = testTransform.rotation;

        //    gameObject.SetActive(true);
        //    IsPlaced = true;
        //    return true;
        //}

        return false;
    }

    private void FixOverhangs()
    {
        //var testPoints = new List<Vector3>
        //{
        //    new Vector3(-1.1f, 0.0f, 0.1f),
        //    new Vector3(1.1f, 0.0f, 0.1f),

        //}
    }
}
