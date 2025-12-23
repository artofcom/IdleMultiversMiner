using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableImage_v1 : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Canvas targetCanvas;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private Vector2 pointerOffsetToImagePivotInParentLocal;

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

            // --- 화면 경계 제한 로직 시작 ---
            // 1. 이미지의 원하는 anchoredPosition(부모 로컬)을 월드 좌표로 변환합니다.
            Vector3 imagePivotWorldPos = rectTransform.parent.TransformPoint(desiredAnchoredPosition);

            // 2. 이미지의 피벗 월드 좌표를 캔버스 로컬 좌표로 변환합니다.
            Vector2 imagePivotCanvasLocalPos = canvasRectTransform.InverseTransformPoint(imagePivotWorldPos);

            // 3. 이미지의 실제 스케일된 크기 (픽셀 단위)를 가져옵니다.
            Vector2 actualImageSize = rectTransform.rect.size;

            // 4. 캔버스의 로컬 좌표계에서의 실제 경계 값을 가져옵니다.
            float canvasMinX = canvasRectTransform.rect.xMin;
            float canvasMaxX = canvasRectTransform.rect.xMax;
            float canvasMinY = canvasRectTransform.rect.yMin;
            float canvasMaxY = canvasRectTransform.rect.yMax;
            
            // 5. 이미지의 피벗과 실제 크기를 고려하여, 이미지 피벗이 움직일 수 있는 캔버스 로컬 좌표의 최소/최대값을 계산합니다.
            // 이 계산은 이미지가 캔버스보다 크거나 작을 때 모두 동작하며, 피벗이 캔버스 로컬 Rect의 (0,0)에 있지 않을 때도 유효해야 합니다.

            // 이미지의 왼쪽 가장자리가 캔버스의 왼쪽 가장자리에 닿을 때, 피벗의 캔버스 로컬 X 위치
            // 피벗의 X 위치 = 캔버스_좌하단_X + (이미지_너비 * 이미지_피벗_X)
            float clampedMinX = canvasMinX + (actualImageSize.x * rectTransform.pivot.x);
            
            // 이미지의 오른쪽 가장자리가 캔버스의 오른쪽 가장자리에 닿을 때, 피벗의 캔버스 로컬 X 위치
            // 피벗의 X 위치 = 캔버스_우상단_X - (이미지_너비 * (1 - 이미지_피벗_X))
            float clampedMaxX = canvasMaxX - (actualImageSize.x * (1f - rectTransform.pivot.x));

            // 이미지의 아래쪽 가장자리가 캔버스 아래쪽 가장자리에 닿을 때, 피벗의 캔버스 로컬 Y 위치
            float clampedMinY = canvasMinY + (actualImageSize.y * rectTransform.pivot.y);
            
            // 이미지의 위쪽 가장자리가 캔버스 위쪽 가장자리에 닿을 때, 피벗의 캔버스 로컬 Y 위치
            float clampedMaxY = canvasMaxY - (actualImageSize.y * (1f - rectTransform.pivot.y));
            
            // --- 핵심 수정: min과 max가 뒤집혔을 경우를 처리 ---
            // 이미지가 캔버스보다 작아서 이미지의 전체 너비/높이가 캔버스에 들어갈 경우,
            // clampedMinX가 clampedMaxX보다 커질 수 있습니다. (예: clampedMinX = 50, clampedMaxX = -50)
            // 이때는 이미지가 캔버스 중앙에 고정되도록 min/max 범위를 조정해야 합니다.
            // 올바른 min/max 범위는 항상 min <= max 이어야 합니다.
            
            float finalMinX = Mathf.Min(clampedMinX, clampedMaxX);
            float finalMaxX = Mathf.Max(clampedMinX, clampedMaxX);
            float finalMinY = Mathf.Min(clampedMinY, clampedMaxY);
            float finalMaxY = Mathf.Max(clampedMinY, clampedMaxY);

            // Debug.Log($"Raw Clamped Bounds: X[{clampedMinX}, {clampedMaxX}], Y[{clampedMinY}, {clampedMaxY}]");
            // Debug.Log($"Final Clamped Bounds: X[{finalMinX}, {finalMaxX}], Y[{finalMinY}, {finalMaxY}]");
            // Debug.Log($"Image Pivot (Canvas Local): {imagePivotCanvasLocalPos}");

            // 6. 계산된 최종 범위 내로 이미지의 캔버스 로컬 피벗 위치를 클램핑합니다.
            Vector2 clampedImagePivotCanvasLocalPos = imagePivotCanvasLocalPos;
            clampedImagePivotCanvasLocalPos.x = Mathf.Clamp(clampedImagePivotCanvasLocalPos.x, finalMinX, finalMaxX);
            clampedImagePivotCanvasLocalPos.y = Mathf.Clamp(clampedImagePivotCanvasLocalPos.y, finalMinY, finalMaxY);
            
            // Debug.Log($"Clamped Image Pivot (Canvas Local): {clampedImagePivotCanvasLocalPos}");

            // 7. 클램핑된 캔버스 로컬 피벗 위치를 다시 월드 좌표로 변환합니다.
            Vector3 clampedImagePivotWorldPos = canvasRectTransform.TransformPoint(clampedImagePivotCanvasLocalPos);

            // 8. 클램핑된 월드 좌표를 이미지 부모의 로컬 anchoredPosition으로 변환하여 적용합니다.
            rectTransform.anchoredPosition = rectTransform.parent.InverseTransformPoint(clampedImagePivotWorldPos);
            // --- 화면 경계 제한 로직 끝 ---
        }
    }
}