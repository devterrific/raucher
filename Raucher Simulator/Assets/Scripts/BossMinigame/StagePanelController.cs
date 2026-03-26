using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StagePanelController : MonoBehaviour
{
    [Serializable]
    public class StagePanelEntry
    {
        public string stageName;
        public Sprite stageSprite;
    }

    [Header("UI")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Image _stageImage;

    [Header("Stage Images")]
    [SerializeField] private List<StagePanelEntry> _stageEntries = new();

    [Header("Settings")]
    [SerializeField] private float _showDuration = 2f;

    private void Awake()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    public IEnumerator ShowStagePanel(int stageIndex)
    {
        if (_panelRoot == null || _stageImage == null || _stageEntries.Count == 0)
            yield break;

        if (stageIndex < 0 || stageIndex >= _stageEntries.Count)
            yield break;

        _stageImage.sprite = _stageEntries[stageIndex].stageSprite;
        _panelRoot.SetActive(true);

        yield return new WaitForSeconds(_showDuration);

        _panelRoot.SetActive(false);
    }

    public Sprite GetStageSprite(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stageEntries.Count)
            return null;

        return _stageEntries[stageIndex].stageSprite;
    }
}