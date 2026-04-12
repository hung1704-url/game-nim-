using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Cấu hình Map")]
    public GameObject matchPrefab;
    public Transform[] rows;

    [Header("Vùng bốc diêm")]
    public Transform rightPanel; // Kéo Right_panel 30% vào đây

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
        // Nếu chưa bốc que nào thì không cho bấm
        if (rightPanel.childCount == 0) return;

        // 1. Xóa sạch diêm ở vùng nháp (vì đã bốc xong)
        foreach (Transform child in rightPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. Quan trọng: Reset biến để lượt sau bốc được hàng khác
        selectedRowIndex = -1;

        Debug.Log("Lượt của AI bắt đầu!");
        // Gọi AI ở đây: StartCoroutine(AI_Turn());
    }
}