using UnityEngine;

namespace ZombieHUD
{
    // HUD 스크립트들이 공통으로 쓰는 카메라/방위 계산 도우미
    public static class HudCameraUtil
    {
        // 게임 화면을 그리는 카메라를 찾는다.
        // 현재 씬의 Main Camera에는 "MainCamera" 태그가 없어 Camera.main 이 null 이므로,
        // RenderTexture 로 그리지 않는(=화면에 직접 그리는) 카메라 중 depth 가 가장 높은 것을 대신 사용한다.
        public static Camera FindGameplayCamera()
        {
            Camera main = Camera.main;
            if (main != null) return main;

            Camera best = null;
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.targetTexture != null) continue; // 미니맵 카메라 등 제외
                if (best == null || cam.depth > best.depth) best = cam;
            }
            return best;
        }

        // Transform 이 바라보는 수평 방위각(0~360, 0 = 월드 +Z = 북쪽, 시계방향 증가)
        public static float GetYaw(Transform t)
        {
            Vector3 forward = t.forward;
            forward.y = 0f;

            // 카메라가 거의 수직으로 내려다볼 때는 forward 가 0 에 가까우므로 up 벡터로 대체
            if (forward.sqrMagnitude < 1e-6f)
            {
                forward = t.up;
                forward.y = 0f;
            }

            return Mathf.Repeat(Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, 360f);
        }
    }
}
