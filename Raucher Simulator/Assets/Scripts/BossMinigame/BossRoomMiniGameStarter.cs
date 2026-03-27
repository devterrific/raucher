using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BossRoomMiniGameStarter : MonoBehaviour
{
    [Header("Countdown Panel")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private Image stagePreviewImage;
    [SerializeField] private float previewDuration = 2f;

    [Header("Stage Images (1-4)")]
    [SerializeField] private Sprite stage1Sprite;
    [SerializeField] private Sprite stage2Sprite;
    [SerializeField] private Sprite stage3Sprite;
    [SerializeField] private Sprite stage4Sprite;

    [Header("Mini Game Start")]
    [SerializeField] private GameObject miniGameContent;
    [SerializeField] private bool startAutomaticallyOnSceneLoad = true;
    [SerializeField] private UnityEvent onMiniGameStart;

    public int CurrentStage { get; private set; }

    private void Start()
    {
        if (startAutomaticallyOnSceneLoad)
            StartMiniGameFlow();
    }

    public void StartMiniGameFlow()
    {
        StartCoroutine(MiniGameFlowRoutine());
    }

    private IEnumerator MiniGameFlowRoutine()
    {
        // Mini Game Inhalt zuerst aus
        if (miniGameContent != null)
            miniGameContent.SetActive(false);

        // Random Stage 1 bis 4 wählen
        CurrentStage = Random.Range(1, 5);

        // Passendes Bild setzen
        if (stagePreviewImage != null)
            stagePreviewImage.sprite = GetStageSprite(CurrentStage);

        // Countdown Panel anzeigen
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        // 2 Sekunden warten
        yield return new WaitForSeconds(previewDuration);

        // Countdown Panel ausblenden
        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        // Mini Game jetzt starten
        if (miniGameContent != null)
            miniGameContent.SetActive(true);

        onMiniGameStart?.Invoke();
    }

    private Sprite GetStageSprite(int stage)
    {
        switch (stage)
        {
            case 1: return stage1Sprite;
            case 2: return stage2Sprite;
            case 3: return stage3Sprite;
            case 4: return stage4Sprite;
            default: return null;
        }
    }
}