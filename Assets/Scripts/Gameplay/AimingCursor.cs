using UnityEngine;

// Kept as stub — functionality moved to DirectionArrow.cs
public class AimingCursor : MonoBehaviour
{
    public static AimingCursor Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartPreAim() { }
    public void OnFirstTap(Vector2 screenPos) { }
    public void OnSecondTap(Vector2 screenPos) { }
    public void HideAll() { }
}
