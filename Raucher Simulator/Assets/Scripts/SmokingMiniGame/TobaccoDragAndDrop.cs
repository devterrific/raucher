using System;
using UnityEngine;
using UnityEngine.UI;

public class TobaccoDragAndDrop : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas canvas; 
    [SerializeField] private RectTransform paperRect;
    [SerializeField] private Image tobaccoPrefab;  

    [Header("Placement")]
    [SerializeField] private Vector2 localOffsetOnPaper = new Vector2(0f, 0f);
    [SerializeField] private bool clampInsidePaper = true;

    [Header("Placement Rules")]
    [SerializeField] private bool requireMouseOverPaper = true;


    private Image spawnedTobacco;
    private RectTransform tobaccoRT;
    private bool isDragging;

    public bool IsDragging => isDragging;

    private Action onPlaced;

    public void BeginDrag(Canvas c, RectTransform paper, Image prefab, Action placedCallback)
    {
        canvas = c;
        paperRect = paper;
        tobaccoPrefab = prefab;
        onPlaced = placedCallback;

        if (canvas == null || paperRect == null || tobaccoPrefab == null)
        {
            Debug.LogWarning("TobaccoDragAndDrop: Missing references (Canvas/Paper/Prefab).");
            return;
        }
        spawnedTobacco = Instantiate(tobaccoPrefab, canvas.transform);
        spawnedTobacco.raycastTarget = false; // darf Klicks nicht blockieren
        tobaccoRT = spawnedTobacco.rectTransform;

        isDragging = true;
        FollowMouse();
    }

    private void Update()
    {
        if (!isDragging) return;

        FollowMouse();

        if (Input.GetMouseButtonDown(0))
        {
            if (!requireMouseOverPaper || IsMouseOverPaper())
                PlaceOnPaper();
            else
                Debug.Log("Tabak kann nur auf dem Papier platziert werden!");
        }
    }

    private void FollowMouse()
    {
        if (canvas == null || tobaccoRT == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        tobaccoRT.anchoredPosition = localPoint;
    }

    private void PlaceOnPaper()
    {
        if (paperRect == null || tobaccoRT == null) return;

        tobaccoRT.SetParent(paperRect, worldPositionStays: false);

        // Mausposition -> Local Point im Paper
        Vector2 localPointInPaper;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            paperRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPointInPaper
        );

        Vector2 finalPos = localPointInPaper + localOffsetOnPaper;

        if (clampInsidePaper)
        {
            Rect pr = paperRect.rect;
            Rect tr = tobaccoRT.rect;

            float halfW = tr.width * 0.5f;
            float halfH = tr.height * 0.5f;

            finalPos.x = Mathf.Clamp(finalPos.x, pr.xMin + halfW, pr.xMax - halfW);
            finalPos.y = Mathf.Clamp(finalPos.y, pr.yMin + halfH, pr.yMax - halfH);
        }

        tobaccoRT.anchoredPosition = finalPos;

        isDragging = false;
        onPlaced?.Invoke();
    }

    private bool IsMouseOverPaper()
    {
        if (paperRect == null || canvas == null) return false;

        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(
            paperRect,
            Input.mousePosition,
            cam
        );
    }

}
