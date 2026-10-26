using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ZombieHUD
{
    // 화면 우측 하단의 배틀그라운드 스타일 미니맵
    // - 플레이어 위에 직교(Orthographic) 탑뷰 카메라를 띄워 RenderTexture 로 지형을 그린다. (북쪽 고정)
    // - 플레이어 화살표는 캐릭터 방향, 시야 부채꼴은 카메라 방향으로 회전한다.
    // - 좀비(LivingEntity, 플레이어 제외)는 빨간 점 아이콘으로 표시한다.
    // - '=' / '-' (또는 키패드 +/-) 로 줌 단계를 바꾼다.
    [DisallowMultipleComponent]
    public class PubgMinimap : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("미니맵 중심. 비워두면 PlayerHealth 를 가진 오브젝트를 자동으로 찾습니다.")]
        public Transform target;

        [Header("UI References")]
        public RawImage mapImage;
        [Tooltip("격자 텍스처(Wrap Mode = Repeat)를 가진 RawImage")]
        public RawImage gridImage;
        [Tooltip("적 아이콘이 생성될 부모 (맵과 같은 크기)")]
        public RectTransform iconLayer;
        public RectTransform playerIcon;
        public RectTransform viewCone;
        public Sprite enemyIconSprite;

        [Header("Minimap Camera")]
        [Tooltip("비워두면 런타임에 자동 생성합니다.")]
        public Camera minimapCamera;
        [Tooltip("플레이어 머리 위 카메라 높이(m)")]
        public float cameraHeight = 60f;
        public int textureSize = 512;
        [Tooltip("미니맵에 그릴 레이어. 기본값은 UI / Player / Enemy 레이어 제외 (플레이어와 적은 아이콘으로 표시)")]
        public LayerMask cullingMask = ~((1 << 5) | (1 << 3) | (1 << 6));
        public Color backgroundColor = new Color(0.16f, 0.17f, 0.15f, 1f);

        [Header("Zoom")]
        [Tooltip("각 줌 단계에서 미니맵 한 변이 보여주는 월드 거리(m)")]
        public float[] zoomLevels = { 60f, 100f, 160f };
        public int zoomIndex = 1;
        public KeyCode zoomInKey = KeyCode.Equals;
        public KeyCode zoomOutKey = KeyCode.Minus;
        public float zoomLerpSpeed = 10f;

        [Header("Grid")]
        [Tooltip("격자 한 칸의 월드 크기(m)")]
        public float gridCellSize = 20f;

        [Header("Enemies")]
        public bool showEnemies = true;
        public Color enemyColor = new Color(1f, 0.25f, 0.2f, 1f);
        public float enemyIconSize = 9f;
        [Tooltip("적 목록을 다시 검색하는 간격(초). 매 프레임 검색하지 않기 위함")]
        public float enemyScanInterval = 0.5f;
        [Tooltip("범위 밖 적을 미니맵 가장자리에 붙여서 표시")]
        public bool clampEnemiesToEdge = false;

        public float CurrentWorldSize { get; private set; }

        private RenderTexture renderTexture;
        private bool ownsCamera;
        private Transform viewSource;
        private float nextEnemyScan;
        private float nextTargetSearch;
        private readonly List<LivingEntity> enemies = new List<LivingEntity>();
        private readonly List<Image> iconPool = new List<Image>();

        private void Start()
        {
            zoomIndex = Mathf.Clamp(zoomIndex, 0, Mathf.Max(0, zoomLevels.Length - 1));
            CurrentWorldSize = zoomLevels.Length > 0 ? zoomLevels[zoomIndex] : 100f;
            SetupCamera();
        }

        private void SetupCamera()
        {
            if (minimapCamera == null)
            {
                GameObject camGo = new GameObject("[Minimap Camera]");
                minimapCamera = camGo.AddComponent<Camera>();
                ownsCamera = true;
            }

            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = CurrentWorldSize * 0.5f;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = backgroundColor;
            minimapCamera.cullingMask = cullingMask;
            minimapCamera.nearClipPlane = 0.3f;
            minimapCamera.farClipPlane = cameraHeight + 100f;
            minimapCamera.depth = -10f;
            minimapCamera.allowHDR = false;
            minimapCamera.allowMSAA = false;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 수직 하향, 화면 위쪽 = 북쪽(+Z)

            renderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = "MinimapRT";
            renderTexture.Create();
            minimapCamera.targetTexture = renderTexture;

            // URP 카메라 옵션: 미니맵은 그림자/포스트프로세싱이 필요 없으므로 끈다 (성능)
            UniversalAdditionalCameraData data = minimapCamera.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderShadows = false;
                data.renderPostProcessing = false;
                data.antialiasing = AntialiasingMode.None;
                data.requiresDepthTexture = false;
                data.requiresColorTexture = false;
            }

            if (mapImage != null) mapImage.texture = renderTexture;
        }

        private void LateUpdate()
        {
            if (!ResolveTarget()) return;

            HandleZoom();

            Vector3 center = target.position;
            float half = CurrentWorldSize * 0.5f;

            // 카메라 추적
            if (minimapCamera != null)
            {
                minimapCamera.transform.position = new Vector3(center.x, center.y + cameraHeight, center.z);
                minimapCamera.orthographicSize = half;
            }

            // 격자: 텍스처 UV 를 월드 좌표에 맞춰 스크롤 → 격자가 땅에 고정된 것처럼 보임
            if (gridImage != null && gridCellSize > 0f)
            {
                float cells = CurrentWorldSize / gridCellSize;
                gridImage.uvRect = new Rect((center.x - half) / gridCellSize, (center.z - half) / gridCellSize, cells, cells);
            }

            // 플레이어 화살표 = 캐릭터가 바라보는 방향
            if (playerIcon != null)
                playerIcon.localEulerAngles = new Vector3(0f, 0f, -HudCameraUtil.GetYaw(target));

            // 시야 부채꼴 = 카메라가 바라보는 방향
            if (viewCone != null)
            {
                if (viewSource == null)
                {
                    Camera cam = HudCameraUtil.FindGameplayCamera();
                    if (cam != null) viewSource = cam.transform;
                }
                if (viewSource != null)
                    viewCone.localEulerAngles = new Vector3(0f, 0f, -HudCameraUtil.GetYaw(viewSource));
            }

            UpdateEnemyIcons(center);
        }

        private bool ResolveTarget()
        {
            if (target != null) return true;
            if (Time.unscaledTime < nextTargetSearch) return false;
            nextTargetSearch = Time.unscaledTime + 1f;

            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null) target = player.transform;
            return target != null;
        }

        private void HandleZoom()
        {
            if (zoomLevels == null || zoomLevels.Length == 0) return;

            if (Input.GetKeyDown(zoomInKey) || Input.GetKeyDown(KeyCode.KeypadPlus))
                zoomIndex = Mathf.Max(0, zoomIndex - 1);
            if (Input.GetKeyDown(zoomOutKey) || Input.GetKeyDown(KeyCode.KeypadMinus))
                zoomIndex = Mathf.Min(zoomLevels.Length - 1, zoomIndex + 1);

            float goal = zoomLevels[zoomIndex];
            CurrentWorldSize = Mathf.Lerp(CurrentWorldSize, goal, 1f - Mathf.Exp(-zoomLerpSpeed * Time.unscaledDeltaTime));
        }

        private void UpdateEnemyIcons(Vector3 center)
        {
            int used = 0;

            if (showEnemies && iconLayer != null)
            {
                if (Time.unscaledTime >= nextEnemyScan)
                {
                    nextEnemyScan = Time.unscaledTime + enemyScanInterval;
                    enemies.Clear();
                    foreach (LivingEntity entity in FindObjectsByType<LivingEntity>(FindObjectsSortMode.None))
                    {
                        if (!(entity is PlayerHealth)) enemies.Add(entity);
                    }
                }

                Vector2 mapSize = iconLayer.rect.size;
                for (int i = 0; i < enemies.Count; i++)
                {
                    LivingEntity e = enemies[i];
                    if (e == null || e.dead || !e.isActiveAndEnabled) continue;

                    Vector3 d = e.transform.position - center;
                    Vector2 n = new Vector2(d.x, d.z) / CurrentWorldSize; // 미니맵 안이면 -0.5 ~ 0.5

                    if (Mathf.Abs(n.x) > 0.5f || Mathf.Abs(n.y) > 0.5f)
                    {
                        if (!clampEnemiesToEdge) continue;
                        n.x = Mathf.Clamp(n.x, -0.5f, 0.5f);
                        n.y = Mathf.Clamp(n.y, -0.5f, 0.5f);
                    }

                    Image icon = GetIcon(used++);
                    icon.rectTransform.anchoredPosition = new Vector2(n.x * mapSize.x, n.y * mapSize.y);
                }
            }

            // 사용하지 않는 아이콘은 숨김 (파괴하지 않고 재사용)
            for (int i = used; i < iconPool.Count; i++)
            {
                if (iconPool[i].enabled) iconPool[i].enabled = false;
            }
        }

        private Image GetIcon(int index)
        {
            while (iconPool.Count <= index)
            {
                GameObject go = new GameObject("EnemyIcon", typeof(RectTransform));
                go.layer = iconLayer.gameObject.layer;
                go.transform.SetParent(iconLayer, false);
                Image img = go.AddComponent<Image>();
                img.sprite = enemyIconSprite;
                img.color = enemyColor;
                img.raycastTarget = false;
                RectTransform rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(enemyIconSize, enemyIconSize);
                iconPool.Add(img);
            }

            Image icon = iconPool[index];
            if (!icon.enabled) icon.enabled = true;
            return icon;
        }

        private void OnDestroy()
        {
            if (minimapCamera != null) minimapCamera.targetTexture = null;
            if (ownsCamera && minimapCamera != null) Destroy(minimapCamera.gameObject);
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
