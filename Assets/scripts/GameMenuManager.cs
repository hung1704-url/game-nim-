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
    [Header("Preview Settings")]
    public UnityEngine.UI.Image imgLeft;
    public UnityEngine.UI.Image imgRight;
    public Sprite p1Sprite, p2Sprite, robotSprite;

    // Hàm này gọi khi chuột RÊ VÀO nút Người vs Người
    public void OnHoverPvP()
    {
        imgLeft.sprite = p1Sprite;
        imgRight.sprite = p2Sprite;
        SetAlpha(1); // Hiện ảnh lên
    }

    // Hàm này gọi khi chuột RÊ VÀO nút Người vs Máy
    public void OnHoverPvE()
    {
        imgLeft.sprite = p1Sprite;
        imgRight.sprite = robotSprite;
        SetAlpha(1); // Hiện ảnh lên
    }

    // Hàm này gọi khi chuột RỜI KHỎI nút
    public void OnHoverExit()
    {
        SetAlpha(0); // Ẩn ảnh đi
    }

    void SetAlpha(float alpha)
    {
        Color c = imgLeft.color;
        c.a = alpha;
        imgLeft.color = c;
        imgRight.color = c;
    }
}