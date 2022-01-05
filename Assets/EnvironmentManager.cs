using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    private Light sunSource;

    // Start is called before the first frame update
    void Start()
    {
        if (sunSource != null)
        {
            RenderSettings.sun = sunSource;
        }
    }

}
