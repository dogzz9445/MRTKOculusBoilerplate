using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SequenceBase
{
    protected bool isCompleted;
    protected bool isSuccessed;

    public event Action Completed;

    protected bool IsCompleted
    {
        get => isCompleted;
        set
        {
            isCompleted = value;
            OnCompleted();
        }
    }

    protected bool IsSuccessed
    {
        get => isSuccessed;
        set => isSuccessed = value;
    }

    public void OnCompleted()
    {
        if (Completed != null && IsCompleted == true)
        {
            Completed();
        }
    }

    public IEnumerator StartSequence()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.OnComplete(() => IsCompleted = true);

        yield return WaitForCompletion();
    }

    public IEnumerator WaitForCompletion()
    {
        while (!IsCompleted)
        {
            yield return Task.Yield();
        }

        yield return null;
    }

}
