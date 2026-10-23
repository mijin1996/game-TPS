using UnityEngine;
using Unity.Cinemachine;

// 조준 스코프 — PlayerInput의 scope 값을 읽어서 화면을 바꾼다
public class Scope : MonoBehaviour {
    public PlayerInput playerInput;
    public GameObject scopeUI;
    public GameObject crosshairUI;  
    public CinemachineCamera cam; 
    public Transform player;     
    public float zoomFov = 20f;      

    private float normalFov;
    private bool isOn = false;
    private Renderer[] playerRenderers;

    public GameObject scopeCam;

    private void Start() {
        normalFov = cam.Lens.FieldOfView;
        playerRenderers = player.GetComponentsInChildren<Renderer>();
        Apply(false);
    }

    private void Update() {
        if (playerInput.scope != isOn) Apply(playerInput.scope);   // 상태가 바뀔 때만 적용
    }

    private void Apply(bool on) {
        isOn = on;
        scopeUI.SetActive(on);
        crosshairUI.SetActive(!on);
        scopeCam.SetActive(on);
    }
}

/*

FOV : 시야를 의미

"
*/