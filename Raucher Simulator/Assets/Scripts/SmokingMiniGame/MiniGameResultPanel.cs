using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameResultPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    private System.Action onRetry;
    private System.Action onContinue;

    private void Awake()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(HandleRetryClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
        }
    }

    public void Show(string textToShow, System.Action retryAction, System.Action continueAction)
    {
        gameObject.SetActive(true);

        onRetry = retryAction;
        onContinue = continueAction;

        if (resultText != null)
        {
            resultText.text = textToShow;
        }
    }

    public void Hide()
    {
        onRetry = null;
        onContinue = null;

        gameObject.SetActive(false);
    }

    private void HandleRetryClicked()
    {
        onRetry?.Invoke();
    }

    private void HandleContinueClicked()
    {
        onContinue?.Invoke();
    }
}
