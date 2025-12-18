using System;
using UnityEngine;
using UnityEngine.UI;

public class TobaccoDragAndDrop : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas canvas;              // dein UI Canvas
    [SerializeField] private RectTransform paperRect;    // RectTransform vom platzierten Papier (spawnedPaper)
    [SerializeField] private Image tobaccoPrefab;        // dein Tabak UI Prefab

    [Header("Placement")]
    [SerializeField] private Vector2 localOffsetOnPaper = new Vector2(0f, 0f); // optionales Feintuning
    [SerializeField] private bool clampInsidePaper = true;

    [Header("Placement Rules")]
    [SerializeField] private bool requireMouseOverPaper = true;


    private Image spawnedTobacco;
    private RectTransform tobaccoRT;
    private bool isDragging;

    public bool IsDragging => isDragging;

    // Callback wenn Tabak erfolgreich platziert wurde
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

        // Spawn Tabak als eigenes UI Element im Canvas (nicht im Paper, damit es frei folgt)
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

        // Linksklick -> platzieren
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

        // Screen -> Local Point in Canvas
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

        // Wir platzieren Tabak als Child vom Paper, damit er “fest” drauf liegt
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
            // clamp so, dass Tabak vollständig im Paper bleibt
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
