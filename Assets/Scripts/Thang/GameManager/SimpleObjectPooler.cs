using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPooler : MonoBehaviour
{
    // Prefab của bụi chúng ta vừa tạo
    public GameObject dustPrefab;
    // Số lượng bụi tối đa trong pool
    public int poolSize = 20;

    // Danh sách lưu trữ các object bụi
    private List<GameObject> pooledObjects = new List<GameObject>();

    void Start()
    {
        // Tạo sẵn các object trong pool ban đầu
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(dustPrefab);
            obj.SetActive(false); // Ban đầu tắt
            pooledObjects.Add(obj);
        }
    }

    // Hàm để lấy một object bụi sẵn sàng sử dụng từ pool
    public GameObject GetPooledDustEffect()
    {
        // Duyệt qua pool để tìm object nào đang không hoạt động
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        // Nếu pool hết, chúng ta có thể chọn mở rộng nó (hoặc bỏ qua)
        // Trong ví dụ này, để đơn giản, chúng ta mở rộng nó.
        GameObject obj = Instantiate(dustPrefab);
        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }
}