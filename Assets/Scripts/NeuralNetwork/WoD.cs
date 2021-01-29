using UnityEngine;

public class WoD : MonoBehaviour
{
    public static float Pos = -10f;

    public void Restart() => transform.position = new Vector2(-10, 0);

    void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime * 3f);
        Pos = transform.position.x;
    }
}
