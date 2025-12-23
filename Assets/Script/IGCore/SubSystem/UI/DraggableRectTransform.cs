using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

namespace IGCore.SubSystem.UI
{
    // Created by Gemini and artofcom.(artofcom2@gmail.com) [July.04.2025]
    //
    // DraggableComp
    // Supports Boundary Limitation.
    // Dragging. 
    // Pinch Zoon In and Out.
    //
    // Note : Also required something with RaycastTarget so it can consume pointer event.
    //
    [RequireComponent(typeof(RectTransform))]
    public class DraggableRectTransform : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] RectTransform viewTransform;

        private RectTransform rectTransform;
        private RectTransform visibleRectTransform;

        private Vector2 pointerOffsetToImagePivotInParentLocal;

        [SerializeField] private float zoomScaleFactor = 0.1f; 
        [SerializeField] private float maxOverallScale = 5.0f;

        // --- Pinch Zoom 관련 변수 ---
        private float initialPinchDistance; // 두 손가락 터치 시작 시의 거리
        private Vector3 initialScaleOnPinchStart; // 핀치 시작 시 이미지의 스케일
        private bool isPinching = false; // 현재 핀치 중인지 여부
        private bool isInteracting = false;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(rectTransform, "Requires RectTransform!" );

            if (targetCanvas == null)
            {
                targetCanvas = GetCanvasFromParent(transform.parent);
                if(targetCanvas == null)
                {
                    Assert.IsNotNull(targetCanvas, "TargetCanvas should be set." );
                    enabled = false;
                    return;
                }
            }
            visibleRectTransform = viewTransform==null ? targetCanvas.GetComponent<RectTransform>() : viewTransform;
            if (visibleRectTransform == null)
            {
                Assert.IsNotNull(visibleRectTransform, "TargetCanvas or viewTransform should be set." );
                enabled = false;
            }
        }

        protected Canvas GetCanvasFromParent(Transform transform)
        {
            if(transform == null)
                return null;

            var canvas = transform.GetComponent<Canvas>();
            if(canvas != null)
                return canvas;

            return GetCanvasFromParent(transform.parent);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 핀치 중에는 단일 터치 드래그를 시작하지 않음
            if (Input.touchCount > 1) return; 

            isInteracting = true;

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

        public void OnPointerUp(PointerEventData eventData)
        {
            // --- Reset the interaction flag when the pointer is lifted ---
            isInteracting = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 핀치 중에는 단일 터치 드래그를 처리하지 않음
            if (Input.touchCount > 1 || isPinching) return; 

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


        // --- Update 함수 추가: Pinch Zoom 처리 ---
        void Update()
        {
            // 두 손가락 터치 감지 (모바일 환경)
            if (isInteracting && Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // 핀치 시작
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    initialScaleOnPinchStart = rectTransform.localScale;
                    isPinching = true;
                }
                // 핀치 중
                else if (isPinching && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
                {
                    float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                
                    // 스케일 비율 계산
                    if (initialPinchDistance == 0) initialPinchDistance = 1; // 0으로 나누기 방지
                    float scaleFactor = currentPinchDistance / initialPinchDistance;

                    // 새로운 전체 스케일 계산
                    float newOverallScale = initialScaleOnPinchStart.x * scaleFactor;

                    // 동적 최소 스케일과 최대 스케일로 범위 제한
                    float minOverallScaleDynamic = CalculateMinScaleToCoverCanvas();
                    newOverallScale = Mathf.Clamp(newOverallScale, minOverallScaleDynamic, maxOverallScale);

                    // 스케일 변화가 없으면 리턴
                    if (Mathf.Approximately(newOverallScale, rectTransform.localScale.x)) return;

                    // --- Pinch 중심 기준 스케일 로직 (핵심) ---
                    // 1. Pinch의 중간점 (스크린 좌표) 계산
                    Vector2 pinchCenterScreen = (touch0.position + touch1.position) / 2f;

                    // 2. 현재 이미지 피벗의 캔버스 로컬 위치를 가져옵니다.
                    Vector2 currentImagePivotCanvasLocalPos = visibleRectTransform.InverseTransformPoint(rectTransform.position);

                    // 3. Pinch 중간점의 캔버스 로컬 위치를 가져옵니다.
                    Vector2 pinchCenterCanvasLocal;
                    // ScreenPointToLocalPointInRectangle은 카메라가 필요할 수 있으므로, 캔버스 렌더 모드에 따라 처리
                    Camera uiCamera = (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main; // 또는 Canvas의 renderCamera 사용
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(visibleRectTransform, pinchCenterScreen, uiCamera, out pinchCenterCanvasLocal);
                
                    // 4. 이미지 피벗에서 Pinch 중심까지의 벡터 (캔버스 로컬 좌표)
                    Vector2 vectorFromImagePivotToPinchCenter = pinchCenterCanvasLocal - currentImagePivotCanvasLocalPos;

                    // 5. 새로운 스케일에 따른 벡터 조정
                    //    새로운 스케일 / 현재 스케일 = 스케일 비율
                    float actualScaleRatio = newOverallScale / rectTransform.localScale.x; // 실제 적용될 스케일 비율
                    Vector2 newVectorFromImagePivotToPinchCenter = vectorFromImagePivotToPinchCenter * actualScaleRatio;

                    // 6. 새로운 이미지 피벗의 캔버스 로컬 위치 계산
                    Vector2 newImagePivotCanvasLocalPos = pinchCenterCanvasLocal - newVectorFromImagePivotToPinchCenter;

                    // 7. 새로운 이미지 스케일 적용 (Z 스케일도 1로 고정)
                    rectTransform.localScale = new Vector3(newOverallScale, newOverallScale, 1f);

                    // 8. 클램핑된 위치를 월드 좌표로 변환하고 이미지 부모의 anchoredPosition으로 변환하여 적용합니다.
                    Vector3 newImagePivotWorldPos = visibleRectTransform.TransformPoint(newImagePivotCanvasLocalPos);
                    Vector2 desiredAnchoredPosition = rectTransform.parent.InverseTransformPoint(newImagePivotWorldPos);

                    // --- 스케일 후 경계 제한 재적용 ---
                    ApplyClamping(ref desiredAnchoredPosition);
                    rectTransform.anchoredPosition = desiredAnchoredPosition;

                    // Pinch 시작점과 스케일 다시 업데이트 (더 부드러운 Pinch를 위함)
                    initialPinchDistance = currentPinchDistance;
                    initialScaleOnPinchStart = rectTransform.localScale;
                }
                // 핀치 끝
                else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
                {
                    isPinching = false;
                }
            }
            else if (Input.touchCount < 2 && isPinching) // 손가락 하나가 떨어지거나 모두 떨어졌을 때 핀치 종료
            {
                isPinching = false;
            }

            if (Input.touchCount == 0)
            {
                isInteracting = false;
                isPinching = false;
            }
        }

        private void ApplyScaleChange(float scaleChangeAmount)
        {
            float currentOverallScale = rectTransform.localScale.x; 
        
            float minOverallScaleDynamic = CalculateMinScaleToCoverCanvas();

            float newOverallScale = Mathf.Clamp(currentOverallScale + scaleChangeAmount, minOverallScaleDynamic, maxOverallScale);

            if (Mathf.Approximately(newOverallScale, currentOverallScale)) return;

            Vector2 currentImagePivotCanvasLocalPos = visibleRectTransform.InverseTransformPoint(rectTransform.position);
            Vector2 canvasCenterLocalPos = visibleRectTransform.rect.center;
            Vector2 vectorFromImagePivotToCanvasCenter = canvasCenterLocalPos - currentImagePivotCanvasLocalPos;

            float scaleRatio = newOverallScale / currentOverallScale;
            Vector2 newVectorFromImagePivotToCanvasCenter = vectorFromImagePivotToCanvasCenter * scaleRatio;

            Vector2 newImagePivotCanvasLocalPos = canvasCenterLocalPos - newVectorFromImagePivotToCanvasCenter;

            rectTransform.localScale = new Vector3(newOverallScale, newOverallScale, 1f); 

            Vector3 newImagePivotWorldPos = visibleRectTransform.TransformPoint(newImagePivotCanvasLocalPos);
            Vector2 desiredAnchoredPosition = rectTransform.parent.InverseTransformPoint(newImagePivotWorldPos);

            ApplyClamping(ref desiredAnchoredPosition);
            rectTransform.anchoredPosition = desiredAnchoredPosition;
        }

        private float CalculateMinScaleToCoverCanvas()
        {
            if (rectTransform.sizeDelta.x == 0 || rectTransform.sizeDelta.y == 0) return 0.01f;

            float originalImageWidth = rectTransform.sizeDelta.x;
            float originalImageHeight = rectTransform.sizeDelta.y;

            float canvasWidth = visibleRectTransform.rect.width;
            float canvasHeight = visibleRectTransform.rect.height;

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

        private void ApplyClamping(ref Vector2 desiredAnchoredPosition)
        {
            Vector3 imagePivotWorldPos = rectTransform.parent.TransformPoint(desiredAnchoredPosition);
            Vector2 imagePivotCanvasLocalPos = visibleRectTransform.InverseTransformPoint(imagePivotWorldPos);

            Vector2 currentActualImageSize = new Vector2(
                rectTransform.sizeDelta.x * rectTransform.localScale.x,
                rectTransform.sizeDelta.y * rectTransform.localScale.y
            ); 

            float canvasRectMinX = visibleRectTransform.rect.xMin;
            float canvasRectMaxX = visibleRectTransform.rect.xMax;
            float canvasRectMinY = visibleRectTransform.rect.yMin;
            float canvasRectMaxY = visibleRectTransform.rect.yMax;
        
            float minPivotX_canvasLocal = canvasRectMinX + (currentActualImageSize.x * rectTransform.pivot.x);
            float maxPivotX_canvasLocal = canvasRectMaxX - (currentActualImageSize.x * (1f - rectTransform.pivot.x));
            float minPivotY_canvasLocal = canvasRectMinY + (currentActualImageSize.y * rectTransform.pivot.y);
            float maxPivotY_canvasLocal = canvasRectMaxY - (currentActualImageSize.y * (1f - rectTransform.pivot.y));
        
            float finalMinX = Mathf.Min(minPivotX_canvasLocal, maxPivotX_canvasLocal);
            float finalMaxX = Mathf.Max(minPivotX_canvasLocal, maxPivotX_canvasLocal);
            float finalMinY = Mathf.Min(minPivotY_canvasLocal, maxPivotY_canvasLocal);
            float finalMaxY = Mathf.Max(minPivotY_canvasLocal, maxPivotY_canvasLocal);

            Vector2 clampedImagePivotCanvasLocalPos = imagePivotCanvasLocalPos;
            clampedImagePivotCanvasLocalPos.x = Mathf.Clamp(clampedImagePivotCanvasLocalPos.x, finalMinX, finalMaxX);
            clampedImagePivotCanvasLocalPos.y = Mathf.Clamp(clampedImagePivotCanvasLocalPos.y, finalMinY, finalMaxY);
        
            Vector3 clampedImagePivotWorldPos = visibleRectTransform.TransformPoint(clampedImagePivotCanvasLocalPos);

            desiredAnchoredPosition = rectTransform.parent.InverseTransformPoint(clampedImagePivotWorldPos);
        }
    }
}