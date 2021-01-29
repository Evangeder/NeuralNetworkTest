using UnityEngine;

public class CantTouch : MonoBehaviour
{
    public string With;
    public string OnEnter;
    public string OnLeave;
    public WalkAi Master;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.StartsWith(With))
            if (!string.IsNullOrWhiteSpace(OnEnter))
                Master.SendMessage(OnEnter);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name.StartsWith(With))
            if (!string.IsNullOrWhiteSpace(OnLeave))
                Master.SendMessage(OnLeave);
    }
}
