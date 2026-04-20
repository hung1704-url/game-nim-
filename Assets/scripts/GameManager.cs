using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (rightPanel.childCount == 0) return;

        foreach (Transform child in rightPanel) Destroy(child.gameObject);
        selectedRowIndex = -1;

        CheckGameOver(); // Kiểm tra xem có ai vừa bốc que cuối không

        // Đổi lượt
        isPlayerTurn = !isPlayerTurn;

        // KIỂM TRA CHẾ ĐỘ CHƠI
        if (GameData.isPvP)
        {
            // Chế độ Người vs Người: Chỉ hiện chữ báo lượt, không gọi Bot
            statusText.text = isPlayerTurn ? "LƯỢT NGƯỜI 1" : "LƯỢT NGƯỜI 2";
        }
        else
        {
            // Chế độ Người vs Máy: Nếu đến lượt Máy thì mới gọi AI bốc
            if (!isPlayerTurn)
            {
                statusText.text = "AI ĐANG SUY NGHĨ...";
                ExecuteAITurn();
            }
            else
            {
                statusText.text = "LƯỢT CỦA BẠN";
            }
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
    void CheckGameOver()
    {
        int totalMatches = 0;
        foreach (Transform row in rows)
        {
            totalMatches += row.childCount;
        }

        // Nếu không còn que diêm nào trên bàn
        if (totalMatches == 0)
        {
            if (isPlayerTurn)
            {
                // Nếu bốc xong mà đến lượt Player (nghĩa là AI vừa bốc que cuối)
                statusText.text = "AI BỐC QUE CUỐI - BẠN THẮNG!";
                statusText.color = Color.green;
            }
            else
            {
                // Nếu bốc xong mà đến lượt AI (nghĩa là Player vừa bốc que cuối)
                statusText.text = "BẠN BỐC QUE CUỐI - AI THẮNG!";
                statusText.color = Color.red;
            }

            Debug.Log("GAME OVER!");
            // Bạn có thể thêm code để hiện nút "Chơi lại" ở đây
        }
    }
}