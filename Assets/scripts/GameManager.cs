using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Cấu hình Map")]
    public GameObject matchPrefab;
    public Transform[] rows;

    [Header("Vùng bốc diêm")]
    public Transform rightPanel; // Kéo Right_panel 30% vào đây

    [Header("Turn Management")]
    public bool isPlayerTurn = true; // true là người chơi, false là AI
    public GameObject winPanel;      // Panel hiện chữ Thắng/Thua (nếu có)
    [Header("End Game UI")]
    public TMPro.TextMeshProUGUI statusText; // Kéo cái Text hiển thị trạng thái vào đây

    private int selectedRowIndex = -1; // Để khóa người chơi chỉ bốc 1 hàng

    [Header("Turn UI")]
    public UnityEngine.UI.Image currentTurnAvatar; // Kéo cái Image dưới chữ "Lượt của" vào đây
    public Sprite p1Sprite;    // Ảnh bạn vẽ Người 1
    public Sprite p2Sprite;    // Ảnh bạn vẽ Người 2
    public Sprite robotSprite; // Ảnh bạn vẽ Máy

    bool isGameOver = false;

    [Header("Menus")]
    public GameObject pausePanel; // Dòng này cực kỳ quan trọng để Unity nhận diện Panel

    void UpdateTurnUI()
    {
        if (GameData.isPvP)
        {
            // Chế độ Người vs Người
            if (isPlayerTurn)
            {
                statusText.text = "LƯỢT NGƯỜI 1";
                currentTurnAvatar.sprite = p1Sprite; // Hiện ảnh P1 bạn vẽ
            }
            else
            {
                statusText.text = "LƯỢT NGƯỜI 2";
                currentTurnAvatar.sprite = p2Sprite; // Hiện ảnh P2 bạn vẽ
            }
        }
        else
        {
            // Chế độ Người vs Máy
            if (isPlayerTurn)
            {
                statusText.text = "LƯỢT CỦA BẠN";
                currentTurnAvatar.sprite = p1Sprite;
            }
            else
            {
                statusText.text = "AI ĐANG SUY NGHĨ...";
                currentTurnAvatar.sprite = robotSprite; // Hiện ảnh con Bot bạn vẽ
            }
        }
    }

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        foreach (Transform row in rows)
        {
            int count = Random.Range(1, 11);
            for (int i = 0; i < count; i++)
            {
                GameObject newMatch = Instantiate(matchPrefab, row);

                // Tự động gán sự kiện Click bằng code
                UnityEngine.UI.Button btn = newMatch.GetComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => OnMatchClicked(newMatch));
            }
        }
    }

    // --- PHẦN MỚI: LOGIC BỐC DIÊM ---

    public void OnMatchClicked(GameObject clickedMatch)
    {
        Transform matchTransform = clickedMatch.transform;

        // 1. Nếu diêm đang ở bên trái (trong hàng)
        if (matchTransform.parent != rightPanel)
        {
            int currentRow = GetRowIndexOfMatch(matchTransform);

            // Chỉ được bốc ở 1 hàng duy nhất trong 1 lượt
            if (selectedRowIndex != -1 && selectedRowIndex != currentRow)
            {
                Debug.Log("Chỉ được bốc ở hàng thứ " + (selectedRowIndex + 1));
                return;
            }

            selectedRowIndex = currentRow;
            matchTransform.SetParent(rightPanel);
        }
        // 2. Nếu diêm đang ở bên phải (trả lại)
        else
        {
            matchTransform.SetParent(rows[selectedRowIndex]);
            if (rightPanel.childCount == 0) selectedRowIndex = -1;
        }
    }

    private int GetRowIndexOfMatch(Transform match)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (match.parent == rows[i]) return i;
        }
        return -1;
    }
    public void ConfirmMove()
    {
        if (rightPanel.childCount == 0) return; // Nếu chưa bốc que nào thì không cho xác nhận

        foreach (Transform child in rightPanel)
        {
            Destroy(child.gameObject); // Xóa diêm đã bốc
        }
        selectedRowIndex = -1; // Reset để lượt sau bốc hàng khác
        Debug.Log("Đã bốc xong! Chờ AI...");
        // Ở đây sau này sẽ gọi hàm cho AI đánh

    }
    public void ConfirmPick()
    {
        if (rightPanel.childCount == 0 || isGameOver) return; // Nếu game xong rồi thì không cho bấm nữa

        foreach (Transform child in rightPanel) Destroy(child.gameObject);
        selectedRowIndex = -1;

        // 1. Kiểm tra thắng thua TRƯỚC
        if (CheckGameOver())
        {
            return; // DỪNG TẠI ĐÂY, không chạy xuống phần đổi lượt hay đổi chữ nữa
        }

        // 2. Nếu chưa thắng thì mới đổi lượt và cập nhật UI lượt chơi
        isPlayerTurn = !isPlayerTurn;
        UpdateTurnUI();

        // 3. Xử lý AI
        if (!GameData.isPvP && !isPlayerTurn)
        {
            ExecuteAITurn();
        }
    }


    public void ExecuteAITurn()
    {
        int nimSum = 0;
        int rowsWithMoreThanOne = 0;
        int rowsWithExactlyOne = 0;

        // 1. Tính toán trạng thái bàn cờ
        int[] counts = new int[rows.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            counts[i] = rows[i].childCount;
            nimSum ^= counts[i];
            if (counts[i] > 1) rowsWithMoreThanOne++;
            if (counts[i] == 1) rowsWithExactlyOne++;
        }

        // 2. Logic đặc biệt cho luật "Bốc cuối là thua" (Misere Play)
        // Nếu chỉ còn các hàng có đúng 1 que diêm
        if (rowsWithMoreThanOne == 1)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > 1)
                {
                    // Bốc sao cho để lại số lượng hàng có 1 que là số LẺ
                    int target = (rowsWithExactlyOne % 2 == 0) ? 1 : 0;
                    StartCoroutine(BotPickAnimation(i, counts[i] - target));
                    return;
                }
            }
        }

        // 3. Logic Nim-sum thông thường
        if (nimSum != 0)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                int target = counts[i] ^ nimSum;
                if (target < counts[i])
                {
                    StartCoroutine(BotPickAnimation(i, counts[i] - target));
                    return;
                }
            }
        }

        // 4. Nếu rơi vào thế thua, bốc đại 1 que ở hàng đầu tiên có diêm
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] > 0)
            {
                StartCoroutine(BotPickAnimation(i, 1));
                return;
            }
        }

    }
    IEnumerator BotPickAnimation(int rowIndex, int amount)
    {
        yield return new WaitForSeconds(1f); // Bot giả vờ suy nghĩ 1 giây

        for (int i = 0; i < amount; i++)
        {
            if (rows[rowIndex].childCount > 0)
            {
                Transform match = rows[rowIndex].GetChild(rows[rowIndex].childCount - 1);
                match.SetParent(rightPanel);
                yield return new WaitForSeconds(0.3f); // Bốc từng que cho mượt
            }
        }

        yield return new WaitForSeconds(0.5f);
        ConfirmPick(); // Gọi hàm xác nhận để kết thúc lượt của Bot
    }
    bool CheckGameOver()
    {
        if (IsAllMatchesGone())
        {
            isGameOver = true; // Khóa game lại
            statusText.color = Color.yellow;

            if (GameData.isPvP)
            {
                statusText.text = isPlayerTurn ? "NGƯỜI CHƠI 2 THẮNG!" : "NGƯỜI CHƠI 1 THẮNG!";
            }
            else
            {
                statusText.text = isPlayerTurn ? "AI THẮNG CUỘC!" : "BẠN ĐÃ THẮNG AI!";
            }
            return true;
        }
        return false;
    }

    // Hàm phụ để kiểm tra xem trên bàn còn diêm không
    bool IsAllMatchesGone()
    {
        foreach (var row in rows)
        {
            if (row.childCount > 0) return false;
        }
        return true;
    }
    public void ResetGame()
    {
        // Load lại scene hiện tại để bắt đầu ván mới hoàn toàn
        Time.timeScale = 1f; // Reset thời gian phòng trường hợp game đang dừng
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Hàm cho nút Menu (mở Panel hỏi)
    public void OpenPauseMenu()
    {
        pausePanel.SetActive(true);
        // Time.timeScale = 0f; // Dừng game nếu muốn
    }

    // Hàm quay về màn hình chính (dùng cho nút trong Panel)
    public void GoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Thay "MainMenu" bằng tên Scene menu của bạn
    }
    public void ClosePauseMenu()
    {
        pausePanel.SetActive(false); // Ẩn cái bảng đi
        Time.timeScale = 1f;         // Cho game chạy tiếp (nếu nãy có dùng Time.timeScale = 0)
    }
}