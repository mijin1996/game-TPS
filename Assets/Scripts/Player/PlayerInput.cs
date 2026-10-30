using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public string moveAxisName = "Vertical";
    public string rotateAxisName = "Horizontal";
    public string fireButtonName = "Fire1";
    public string reloadButtonName = "Reload";
    public bool jump { get; private set; }
    private bool jumpRequested = false;

    public DynamicJoystick joy;

    private bool isFireButtonPressed = false;
    private bool isReloadButtonPressed = false;
    private bool grenadeRequested = false;
    private bool scopeToggled = false; 

    public float move { get; private set; }
    public float rotate { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool grenade { get; private set; }
    public bool scope { get; private set; }
    

    // ---- 모바일 UI 버튼에서 호출할 함수들 ----
    public void OnFireButtonDown() { isFireButtonPressed = true; }
    public void OnFireButtonUp() { isFireButtonPressed = false; }
    public void OnReloadButtonClick() { isReloadButtonPressed = true; }
    public void OnGrenadeButtonClick() { grenadeRequested = true; }

    public void OnJumpButtonClick() { jumpRequested = true; }
    public void ConsumeJump() { jumpRequested = false; }

    public void OnScopeButtonClick() { scopeToggled = !scopeToggled; }

    

    private void Update() {
        if (GM.ins != null && (GM.ins.isGameOver || GM.ins.isPaused)) {
            move = 0;
            rotate = 0;
            fire = false;
            reload = false;
            scope = false;
            grenade = false;
            jump = false;
            return;
        }

        if (joy != null && (joy.Horizontal != 0f || joy.Vertical != 0f)) {
            move = joy.Vertical;
            rotate = joy.Horizontal;
        } else {
            move = Input.GetAxis(moveAxisName);
            rotate = Input.GetAxis(rotateAxisName);
        }


        if (Input.GetButtonDown("Jump")) {
            jumpRequested = true;
        }
        jump = jumpRequested;

        //move = Input.GetAxis(moveAxisName);
        //rotate = Input.GetAxis(rotateAxisName);
        fire = isFireButtonPressed || Input.GetKeyDown(KeyCode.F); //fire 버튼을 실행하기 위함 = fire버튼이 눌리거나 || 키보드가 눌리거나 📌
        reload = isReloadButtonPressed || Input.GetKeyDown(KeyCode.R);

        grenade = grenadeRequested || Input.GetKeyDown(KeyCode.G);
        grenadeRequested = false; 

        // 재장전은 "누른 순간"만 true여야 하므로, 이번 프레임에 반영한 뒤 초기화
        isReloadButtonPressed = false;

        scope = scopeToggled || (Input.touchCount == 0 && Input.GetMouseButton(1));
    }
}

/*
 [📌📝MEMO] p.588

<p.644 ~ 669>
    先必须有Core RP Library => 然后才能使用Scriptable Render Pipeline(SRP)

    在 Unity的 SRP (Scriptable Render Pipeline) 架构下，主要有两种实现方式：
    URP (Universal Render Pipeline)
    HDRP (High Definition Render Pipeline)
    (注：Built-in Render Pipeline 属于旧版内置渲染管线，不属于 SRP。)

-----------------
개념 관계
BRP (Built-in Render Pipeline)
ㄴ SRP (Scriptable Render Pipeline)
    ㄴ URP, HDRP
-----------------

> 前向渲染 (Forward Rendering): 主要用于低配置设备。计算影响每个物体的所有光照的传统方式。（与 Deferred Rendering 相对） | 低配置
> 光照贴图 (Lightmap): 预先计算 (Pre-computation / Baking) | 适合静态物体 (Static Objects Only) | 高性能 & 适合低配置设备 (High Performance)

< Global Illumunation(GI) - Ray Tracing(RT):: 굴절, 그림자, 간접광, 반사를 물리적 계산으로 렌더링 하는 기숧
> Baked GI(烘焙) :: 不可修改 | 高性能 & 适合低配置设备
> Realtime GI(实时) :: 可与 Lightmap (Baked GI) 混合使用 | 适合动态光影变化
> RT GI (Ray Tracing GI / 光线追踪): 通过物理光线追踪实现最高画质的 GI | 极高计算负担 (📌🦺only for 高配置)

-----------------
< ScriptableData >



p. 690
Property(属性) : 필드 값을 get/set 안전하고 내부에서 제어, 외부에서 탐색하는 통로

*/
