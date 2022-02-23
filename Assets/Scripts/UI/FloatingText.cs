using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private TMP_Text _textMesh;

    [SerializeField]
    private string _text;

    public string Text 
    {
        get
        {
            if (_textMesh != null)
            {
                _textMesh.text = _text;
            }
            return _text;
        }
        set
        {
            _text = value;
            if (_textMesh != null)
            {
                _textMesh.text = _text;
            }
        }
    } 

    private void Awake()
    {
        _textMesh = GetComponentInChildren<TMP_Text>();
        if (_textMesh != null)
        {
            Debug.Log("hi");
            _textMesh.text = _text;
        }
        else
        {
            Debug.Log("error");
        }
    }

    private void OnEnable()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
