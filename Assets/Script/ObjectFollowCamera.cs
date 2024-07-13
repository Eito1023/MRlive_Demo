using UnityEngine;

public class ObjectFollowCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // カメラからの距離
    public float distanceFromCamera = 1.0f;
    // オブジェクトの回転オフセット
    public Quaternion rotationOffset = Quaternion.identity;
    // オブジェクトの横方向オフセット
    public float horizontalOffset = 0.0f;

    void Update()
    {
        // カメラの位置と回転を取得
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraPosition = cameraTransform.position;
        Quaternion cameraRotation = cameraTransform.rotation;

        // オブジェクトをカメラの正面に配置
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
        objectTransform.rotation = cameraRotation * rotationOffset;

        // オブジェクトの横方向オフセットを適用
        Vector3 rightVector = Quaternion.Euler(0.0f, cameraRotation.eulerAngles.y, 0.0f) * Vector3.right;
        objectTransform.position += rightVector * horizontalOffset;
    }
}
