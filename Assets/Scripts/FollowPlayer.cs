using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset = new Vector3(0, 5, -10);

    void LateUpdate()
    {
        Vector3 rotatedOffset = player.transform.rotation * offset;
        transform.position = player.transform.position + rotatedOffset;

        transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }
}
