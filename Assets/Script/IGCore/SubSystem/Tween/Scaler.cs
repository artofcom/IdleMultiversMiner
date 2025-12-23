using UnityEngine;

public class Scaler : MonoBehaviour
{
    [SerializeField] Vector3 minScale = new Vector3(1, 1, 1); // 최소 크기
    [SerializeField]  Vector3 maxScale = new Vector3(1, 2, 1); // 최대 크기
    [SerializeField]  float duration = 2f; // 한 사이클의 지속 시간 (초)

    private float timer = 0f; // 타이머
    private bool scalingUp = true; // 현재 스케일링 방향 (확대 중인지 여부)

    void Update()
    {
        // 타이머 업데이트
        timer += Time.deltaTime;

        // 현재 진행률 계산 (0에서 1 사이 값)
        float progress = timer / duration;

        // Lerp를 사용하여 스케일 계산
        Vector3 targetScale = scalingUp ? maxScale : minScale;
        Vector3 fromScale = scalingUp ? minScale : maxScale;
        transform.localScale = Vector3.Lerp(fromScale, targetScale, progress);

        // 스케일링 방향 전환
        if (progress >= 1f)
        {
            scalingUp = !scalingUp; // 방향 전환
            timer = 0f; // 타이머 초기화
        }
    }
}