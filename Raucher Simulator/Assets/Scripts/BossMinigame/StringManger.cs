using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StringManger : MonoBehaviour
{
    [Serializable]
    public class StageData
    {
        [Min(1)] public int wordsToType = 5;
        [Min(0.1f)] public float timeLimit = 8f;
        public int rewardPoints = 10;
    }

    [Header("Words")]
    [SerializeField] private List<string> _worldList = new List<string>();

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textHolder;
    [SerializeField] private TMP_InputField _inputField;

    [Header("Stages")]
    [SerializeField] private List<StageData> _stages = new List<StageData>();

    [Header("Scene Names")]
    [SerializeField] private string _successSceneName;
    [SerializeField] private string _failSceneName;

    // private vars...
    private int counterIndex;
    private int worldCounter = 0;
    private SpawnManager _spawnManager;

    private int currentStageIndex = -1;
    private float remainingTime = 0f;
    private int totalPoints = 0;
    private bool stageRunning = false;
    private bool gameEnded = false;

    private void Reset()
    {
        if (_stages == null || _stages.Count == 0)
        {
            _stages = new List<StageData>
            {
                new StageData { wordsToType = 5,  timeLimit = 8f,  rewardPoints = 10 },
                new StageData { wordsToType = 10, timeLimit = 14f, rewardPoints = 20 },
                new StageData { wordsToType = 16, timeLimit = 20f, rewardPoints = 30 },
                new StageData { wordsToType = 24, timeLimit = 30f, rewardPoints = 40 }
            };
        }
    }

    private void Awake()
    {
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }

    private void Start()
    {
        if (_worldList == null || _worldList.Count == 0)
        {
            Debug.LogWarning("StringManger: _worldList is empty.");
            return;
        }

        if (_stages == null || _stages.Count == 0)
        {
            Debug.LogWarning("StringManger: _stages is empty.");
            return;
        }

        if (_textHolder == null)
        {
            Debug.LogWarning("StringManger: _textHolder is missing.");
            return;
        }

        if (_inputField == null)
        {
            Debug.LogWarning("StringManger: _inputField is missing.");
            return;
        }

        int randomStage = UnityEngine.Random.Range(0, _stages.Count);
        StartStage(randomStage);
    }

    private void Update()
    {
        if (!stageRunning || gameEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            FailMinigame();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckEvent();
        }
    }

    private void StartStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stages.Count)
        {
            Debug.LogWarning("StringManger: invalid stage index.");
            return;
        }

        currentStageIndex = stageIndex;
        worldCounter = 0;
        remainingTime = _stages[currentStageIndex].timeLimit;
        stageRunning = true;
        gameEnded = false;

        _inputField.text = "";
        _inputField.ActivateInputField();
        _inputField.Select();

        GetNextIndex();
    }

    public void CheckEvent()
    {
        if (!stageRunning || gameEnded)
            return;

        if (_inputField == null || _worldList == null || _worldList.Count == 0)
            return;

        string inputText = _inputField.text.Trim();
        string targetWord = _worldList[counterIndex];

        if (inputText == targetWord)
        {
            _inputField.text = "";
            worldCounter++;

            if (worldCounter >= _stages[currentStageIndex].wordsToType)
            {
                CompleteStage();
                return;
            }

            GetNextIndex();
            _inputField.ActivateInputField();
            _inputField.Select();
        }
        else
        {
            _inputField.ActivateInputField();
            _inputField.Select();
        }
    }

    private void CompleteStage()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        stageRunning = false;
        totalPoints += _stages[currentStageIndex].rewardPoints;

        if (string.IsNullOrWhiteSpace(_successSceneName))
        {
            Debug.LogWarning("StringManger: Success scene name is empty.");
            return;
        }

        SceneManager.LoadScene(_successSceneName);
    }

    private void FailMinigame()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        stageRunning = false;

        if (string.IsNullOrWhiteSpace(_failSceneName))
        {
            Debug.LogWarning("StringManger: Fail scene name is empty.");
            return;
        }

        SceneManager.LoadScene(_failSceneName);
        _spawnManager.LoadSceneWithDelay(1, .5f);

    }

    private void GetNextIndex()
    {
        if (_worldList == null || _worldList.Count == 0)
            return;

        counterIndex = IndexGetter();
        _textHolder.text = _worldList[counterIndex];
    }

    public int IndexGetter()
    {
        return UnityEngine.Random.Range(0, _worldList.Count);
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public int GetCurrentStageIndex()
    {
        return currentStageIndex;
    }

    public int GetWorldCounter()
    {
        return worldCounter;
    }

    public int GetTargetWords()
    {
        if (currentStageIndex < 0 || currentStageIndex >= _stages.Count)
            return 0;

        return _stages[currentStageIndex].wordsToType;
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }

    public int GetCurrentStageRewardPoints()
    {
        if (currentStageIndex < 0 || currentStageIndex >= _stages.Count)
            return 0;

        return _stages[currentStageIndex].rewardPoints;
    }
}