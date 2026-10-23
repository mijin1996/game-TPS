using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour {

    public void Change(int Index) {
        SceneManager.LoadScene(Index); // SceneManager에서 LoadScene를 불러와라 ()안에 숫자 또는 Scene 이름
        
    }
}