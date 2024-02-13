using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    public static ButtonsManager Instance = null;

    public KeyCode jumpButton = KeyCode.Space;
    public KeyCode runningButton = KeyCode.LeftShift;

    private void Awake()
    {
        Instance = this;
    }
}
