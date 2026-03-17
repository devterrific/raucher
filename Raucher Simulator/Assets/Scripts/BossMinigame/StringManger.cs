using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StringManger : MonoBehaviour
{
    [SerializeField] private List<string> _worldList = new List<string>();
    [SerializeField] private TextMeshProUGUI _textHolder;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private int _maxWorlds = 5;
    [SerializeField] private GameObject _canvesHolder;

    // private vars...
    private int counterIndex;
    private int worldCounter = 0;

    private void Start()
    {
        GetNextIndex();
    }

    private void Update()
    {
        if (worldCounter >= _maxWorlds)
            _canvesHolder.SetActive(false);
    }

    public void CheckEvent()
    {
        if (worldCounter < _maxWorlds && _inputField.text == _worldList[counterIndex])
        {
            _inputField.text = "";
            GetNextIndex();
            worldCounter++;
        }
    }

    private void GetNextIndex()
    {   
        counterIndex = IndexGetter();
        _textHolder.text = _worldList[counterIndex];
    }

    public int IndexGetter()
    {  
        return UnityEngine.Random.Range(0, _worldList.Count);
    } 
}
