using UnityEngine;
using UnityEngine.EventSystems;

// 톱니바퀴 그림에 붙이면, 누를 때 설정 창이 열린다
public class SettingsOpenButton : MonoBehaviour, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData) {
        SettingsMenu.ins.Open();
    }
}
