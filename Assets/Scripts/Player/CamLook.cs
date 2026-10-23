using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// 카메라 시점 조작 — CamPivot에 부착. 좌우는 Player 본체, 상하는 이 오브젝트만 회전
public class CamLook : MonoBehaviour {
    public float mouseSensitivity = 3f, touchSensitivity = 0.2f;
    public float minPitch = -40f, maxPitch = 70f;

    private Rigidbody playerRb;
    private float pitch;
    private int lookFingerId = -1; // 시점 조작 중인 손가락 번호 (-1이면 없음)
    private List<RaycastResult> uiHits = new List<RaycastResult>();

    private void Start() {
        playerRb = GetComponentInParent<Rigidbody>();
        pitch = transform.localEulerAngles.x;
    }

    private void Update() {
        if (GM.ins != null && (GM.ins.isGameOver || GM.ins.isPaused)) return;
        Vector2 delta = GetLookDelta();

        if (playerRb != null) playerRb.rotation *= Quaternion.Euler(0f, delta.x, 0f);
        pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private Vector2 GetLookDelta() {
        // 터치가 없으면 마우스
        if (Input.touchCount == 0)
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        foreach (Touch touch in Input.touches) {
            // UI(조이스틱, 버튼) 위가 아닌 곳에서 시작한 손가락만 시점용으로 지정
            if (touch.phase == TouchPhase.Began && lookFingerId == -1 && !IsOverUI(touch.position))
                //!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                lookFingerId = touch.fingerId;

            if (touch.fingerId != lookFingerId) continue;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) { lookFingerId = -1; break; }
            return touch.deltaPosition * touchSensitivity;
        }
        return Vector2.zero;
    }

    private bool IsOverUI(Vector2 screenPos) {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = screenPos;
        uiHits.Clear();
        EventSystem.current.RaycastAll(data, uiHits);
        return uiHits.Count > 0;
    }

}