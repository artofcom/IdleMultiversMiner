using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableImage_v2 : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Canvas targetCanvas;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private Vector2 pointerOffsetToImagePivotInParentLocal;

    [SerializeField] private float zoomScaleFactor = 0.1f; 
    [SerializeField] private float maxOverallScale = 5.0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("DraggableImage 스크립트는 RectTransform 컴포넌트가 있는 GameObject에만 적용할 수 있습니다.");
            enabled = false;
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogError("DraggableImage 스크립트에 targetCanvas가 할당되지 않았습니다. Inspector에서 Canvas를 할당해주세요.");
            enabled = false;
            return;
        }
        canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            Debug.LogError("할당된 targetCanvas에 RectTransform 컴포넌트가 없습니다.");
            enabled = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();

        Vector2 mouseLocalPosInParent;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out mouseLocalPosInParent))
        {
            pointerOffsetToImagePivotInParentLocal = mouseLocalPosInParent - rectTransform.anchoredPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newMouseLocalPosInParent;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out newMouseLocalPosInParent))
        {
            Vector2 desiredAnchoredPosition = newMouseLocalPosInParent - pointerOffsetToImagePivotInParentLocal;

            ApplyClamping(ref desiredAnchoredPosition);

            rectTransform.anchoredPosition = desiredAnchoredPosition;
        }
    }

    public void ScaleUp()
    {
        ApplyScaleChange(zoomScaleFactor);
    }

    public void ScaleDown()
    {
        ApplyScaleChange(-zoomScaleFactor);
    }

    private void ApplyScaleChange(float scaleChangeAmount)
    {
        float currentOverallScale = rectTransform.localScale.x; 
        
        float minOverallScaleDynamic = CalculateMinScaleToCoverCanvas();

        float newOverallScale = Mathf.Clamp(currentOverallScale + scaleChangeAmount, minOverallScaleDynamic, maxOverallScale);

        if (Mathf.Approximately(newOverallScale, currentOverallScale)) return;

        Vector2 currentImagePivotCanvasLocalPos = canvasRectTransform.InverseTransformPoint(rectTransform.position);
        Vector2 canvasCenterLocalPos = canvasRectTransform.rect.center;
        Vector2 vectorFromImagePivotToCanvasCenter = canvasCenterLocalPos - currentImagePivotCanvasLocalPos;

        float scaleRatio = newOverallScale / currentOverallScale;
        Vector2 newVectorFromImagePivotToCanvasCenter = vectorFromImagePivotToCanvasCenter * scaleRatio;

        Vector2 newImagePivotCanvasLocalPos = canvasCenterLocalPos - newVectorFromImagePivotToCanvasCenter;

        rectTransform.localScale = new Vector3(newOverallScale, newOverallScale, 1f); 

        Vector3 newImagePivotWorldPos = canvasRectTransform.TransformPoint(newImagePivotCanvasLocalPos);
        Vector2 desiredAnchoredPosition = rectTransform.parent.InverseTransformPoint(newImagePivotWorldPos);

        ApplyClamping(ref desiredAnchoredPosition);
        rectTransform.anchoredPosition = desiredAnchoredPosition;
    }

    private float CalculateMinScaleToCoverCanvas()
    {
        if (rectTransform.sizeDelta.x == 0 || rectTransform.sizeDelta.y == 0) return 0.01f;

        float originalImageWidth = rectTransform.sizeDelta.x;
        float originalImageHeight = rectTransform.sizeDelta.y;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        float canvasAspect = canvasWidth / canvasHeight;
        float imageAspect = originalImageWidth / originalImageHeight;

        float scaleToFitWidth = canvasWidth / originalImageWidth;
        float scaleToFitHeight = canvasHeight / originalImageHeight;

        if (canvasAspect > imageAspect)
        {
            return scaleToFitWidth; 
        }
        else
        {
            return scaleToFitHeight; 
        }
    }

    /// <summary>
    /// 주어진 desiredAnchoredPosition을 캔버스 경계 내로 클램핑합니다.
    /// 이 함수는 OnDrag와 ApplyScaleChange에서 재활용됩니다.
    /// </summary>
    /// <param name="desiredAnchoredPosition">클램핑할 이미지의 부모 로컬 anchoredPosition 참조.</param>
    private void ApplyClamping(ref Vector2 desiredAnchoredPosition)
    {
        // 1. 원하는 anchoredPosition (이미지 부모 로컬)을 월드 좌표로 변환
        Vector3 imagePivotWorldPos = rectTransform.parent.TransformPoint(desiredAnchoredPosition);

        // 2. 이미지 피벗 월드 좌표를 캔버스 로컬 좌표로 변환
        Vector2 imagePivotCanvasLocalPos = canvasRectTransform.InverseTransformPoint(imagePivotWorldPos);

        // --- 핵심 수정: 이미지의 실제 스케일된 크기를 `sizeDelta`와 `localScale`로 명시적으로 계산 ---
        Vector2 currentActualImageSize = new Vector2(
            rectTransform.sizeDelta.x * rectTransform.localScale.x,
            rectTransform.sizeDelta.y * rectTransform.localScale.y
        ); 

        // 4. 캔버스의 로컬 좌표계에서의 실제 경계 값을 가져옵니다.
        float canvasRectMinX = canvasRectTransform.rect.xMin;
        float canvasRectMaxX = canvasRectTransform.rect.xMax;
        float canvasRectMinY = canvasRectTransform.rect.yMin;
        float canvasRectMaxY = canvasRectTransform.rect.yMax;
        
        // 5. 이미지의 피벗과 현재 스케일된 크기를 고려하여, 피벗이 움직일 수 있는 캔버스 로컬 좌표의 최소/최대값 계산
        float minPivotX_canvasLocal = canvasRectMinX + (currentActualImageSize.x * rectTransform.pivot.x);
        float maxPivotX_canvasLocal = canvasRectMaxX - (currentActualImageSize.x * (1f - rectTransform.pivot.x));
        float minPivotY_canvasLocal = canvasRectMinY + (currentActualImageSize.y * rectTransform.pivot.y);
        float maxPivotY_canvasLocal = canvasRectMaxY - (currentActualImageSize.y * (1f - rectTransform.pivot.y));
        
        // min/max 순서 보정 (finalMin/Max는 항상 min <= max를 만족)
        float finalMinX = Mathf.Min(minPivotX_canvasLocal, maxPivotX_canvasLocal);
        float finalMaxX = Mathf.Max(minPivotX_canvasLocal, maxPivotX_canvasLocal);
        float finalMinY = Mathf.Min(minPivotY_canvasLocal, maxPivotY_canvasLocal);
        float finalMaxY = Mathf.Max(minPivotY_canvasLocal, maxPivotY_canvasLocal);

        // Debug.Log for verification
        // Debug.Log($"Image Local Scale: {rectTransform.localScale.x}");
        // Debug.Log($"Current Actual Image Size (Calculated): {currentActualImageSize}");
        // Debug.Log($"Clamping Bounds (Raw Canvas Local): X[{minPivotX_canvasLocal}, {maxPivotX_canvasLocal}], Y[{minPivotY_canvasLocal}, {maxPivotY_canvasLocal}]");
        Debug.Log($"Clamping Bounds (Final Canvas Local): X[{finalMinX}, {finalMaxX}], Y[{finalMinY}, {finalMaxY}]");
        // Debug.Log($"Image Pivot (Canvas Local, Pre-Clamp): {imagePivotCanvasLocalPos}");

        // 6. 계산된 최종 범위 내로 이미지의 캔버스 로컬 피벗 위치를 클램핑합니다.
        Vector2 clampedImagePivotCanvasLocalPos = imagePivotCanvasLocalPos;
        clampedImagePivotCanvasLocalPos.x = Mathf.Clamp(clampedImagePivotCanvasLocalPos.x, finalMinX, finalMaxX);
        clampedImagePivotCanvasLocalPos.y = Mathf.Clamp(clampedImagePivotCanvasLocalPos.y, finalMinY, finalMaxY);
        
        // Debug.Log($"Image Pivot (Canvas Local, Post-Clamp): {clampedImagePivotCanvasLocalPos}");

        // 7. 클램핑된 캔버스 로컬 피벗 위치를 다시 월드 좌표로 변환합니다.
        Vector3 clampedImagePivotWorldPos = canvasRectTransform.TransformPoint(clampedImagePivotCanvasLocalPos);

        // 8. 클램핑된 월드 좌표를 이미지 부모의 로컬 anchoredPosition으로 변환하여 desiredAnchoredPosition을 직접 업데이트합니다.
        desiredAnchoredPosition = rectTransform.parent.InverseTransformPoint(clampedImagePivotWorldPos);
    }
}