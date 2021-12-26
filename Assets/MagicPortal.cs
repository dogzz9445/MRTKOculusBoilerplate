using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class MagicPortal : MonoBehaviour
{
    public VisualEffect m_visualEffect;
    public TransformData m_transformData;
    public float m_radius;
    public float m_maxRadius;

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
}
