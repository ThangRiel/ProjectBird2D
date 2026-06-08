using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleMovement : MonoBehaviour
{
    public float speed = 5f;
    public float destroyX = -15f;
    private PoolItem myPoolItem;

    void Awake()
    {
        // Lấy script PoolItem đã được gắn vào (hoặc tự tạo)
        myPoolItem = GetComponent<PoolItem>();
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < destroyX)
        {
            // Trả về pool bằng tag đã lưu trong PoolItem
            if (myPoolItem != null)
            {
                ObjectPooler.Instance.ReturnToPool(myPoolItem.assignedTag, gameObject);
            }
        }
    }
    void OnEnable()
    {
        // Cập nhật lại bounds của Tilemap khi nó được kích hoạt từ Pool
        Tilemap tm = GetComponentInChildren<Tilemap>();
        if (tm != null)
        {
            tm.RefreshAllTiles();
        }
    }
}