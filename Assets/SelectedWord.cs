using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedWord : MonoBehaviour
{
    public VisualWord _visualWord;

    private void Awake()
    {
        _visualWord = GetComponent<VisualWord>();
    }

    private void OnEnable()
    {
        _visualWord.onSelected += HandleSelecting;
    }

    private void OnDisable()
    {
        _visualWord.onSelected -= HandleSelecting;
    }


    private void HandleSelecting()
    {
        Debug.Log("SelectedWord");
        gameObject.transform.SetAsLastSibling();
    }
}