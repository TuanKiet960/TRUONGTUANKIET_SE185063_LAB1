using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RobotVacuumController : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    [Tooltip("Tốc độ di chuyển (unit/giây) khi chạy")]
    public float moveSpeed = 2f;

    [Tooltip("Max độ lệch (độ) khi phản xạ sau va chạm")]
    [Range(0f, 90f)]
    public float maxBounceAngle = 20f;

    // Khoảng đẩy ra khỏi collider (units)
    [Tooltip("Khoảng dịch ra khỏi va chạm để tránh kẹt")]
    public float pushOutDistance = 0.05f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private CircleCollider2D circleCol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCol = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Hướng ban đầu ngẫu nhiên
        float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized;

        rb.velocity = moveDirection * moveSpeed;
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Furniture") ||
            col.gameObject.CompareTag("Wall"))
        {
            // 1) Đẩy robot ra khỏi collider
            Vector2 normal = col.contacts[0].normal;
            Vector2 push = normal * (circleCol.radius + pushOutDistance);
            rb.position = col.contacts[0].point + push;

            // 2) Tính phản xạ
            Vector2 reflect = Vector2.Reflect(moveDirection, normal).normalized;

            // 3) Thêm lệch góc ±maxBounceAngle
            float theta = Random.Range(-maxBounceAngle, maxBounceAngle);
            moveDirection = Quaternion.Euler(0, 0, theta) * reflect;

            // 4) Cập nhật velocity ngay lập tức
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // Trường hợp hi hữu robot vẫn nằm trong collider thì nudge thêm
    void OnCollisionStay2D(Collision2D col)
    {
        if ((col.gameObject.CompareTag("Furniture") || col.gameObject.CompareTag("Wall"))
         && rb.velocity.magnitude < 0.1f)
        {
            // Chọn một hướng ngẫu nhiên nhỏ để thoát
            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            moveDirection = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }
}