using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayPvE() // Gán cho nút Đấu với máy
    {
        GameData.isPvP = false;
        SceneManager.LoadScene("gameplay"); // Tên scene bốc diêm của bạn
    }

    public void PlayPvP() // Gán cho nút Đấu với người
    {
        GameData.isPvP = true;
        SceneManager.LoadScene("gameplay");
    }

    public void OpenInstructions() // Gán cho nút Luật chơi
    {
        SceneManager.LoadScene("Instructions");
    }

    public void BackToMenu() // Gán cho nút Quay lại ở scene Luật chơi
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame() // Gán cho nút Thoát
    {
        Application.Quit(); // Lệnh đóng ứng dụng
        Debug.Log("Đã bấm thoát game!"); // Hiện thông báo để mình biết lúc đang test trong Unity
    }
}