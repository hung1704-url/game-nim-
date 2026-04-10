using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Kéo thả Prefab que diêm vào đây ở bảng Inspector
    public GameObject matchPrefab;
    // Kéo thả 5 cái Row (hàng) vào danh sách này
    public Transform[] rows;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        foreach (Transform row in rows)
        {
            // Mỗi hàng sẽ có từ 1 đến 10 que ngẫu nhiên
            int count = Random.Range(1, 11);
            for (int i = 0; i < count; i++)
            {
                Instantiate(matchPrefab, row);
            }
        }
    }
}