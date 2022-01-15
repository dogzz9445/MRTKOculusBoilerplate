using System;
using UnityEngine;

namespace WizardSystem.Common
{
    [Serializable]
    public class TransformData
    {
        public Vector3 LocalPosition = Vector3.zero;
        public Vector3 LocalEulerAngles = Vector3.zero;
        public Vector3 LocalScale = Vector3.one;

        // Unity requires a default constructor for serialization
        public TransformData() { }

        public TransformData(Transform transform)
        {
            LocalPosition = transform.localPosition;
            LocalEulerAngles = transform.localEulerAngles;
            LocalScale = transform.localScale;
        }

        public void ApplyTo(Transform transform)
        {
            transform.localPosition = LocalPosition;
            transform.localEulerAngles = LocalEulerAngles;
            transform.localScale = LocalScale;
        }

        public bool Compare(Transform transform)
        {
            if (transform.localPosition != LocalPosition)
                return false;
            if (transform.localEulerAngles != LocalEulerAngles)
                return false;
            if (transform.localScale != LocalScale)
                return false;
            return true;
        }
    }
}
