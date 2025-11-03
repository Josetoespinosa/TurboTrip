using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputSimple : MonoBehaviour
{
    [Header("Teclas")]
    public Key moveLeft = Key.A;
    public Key moveRight = Key.D;
    public Key jumpKey = Key.Space;
    public Key dashKey = Key.LeftShift;
    public Key interactKey = Key.E;
    public Key wallPassKey = Key.Q;

    [Header("Lecturas (solo lectura)")]
    public float moveAxis;  
    public bool jumpPressed;
    public bool jumpHeld;
    public bool jumpReleased;
    public bool dashPressed;
    public bool interactPressed;

    void Update()
    {
        moveAxis = 0f;
        if (Keyboard.current[moveLeft].isPressed) moveAxis -= 1f;
        if (Keyboard.current[moveRight].isPressed) moveAxis += 1f;

        jumpPressed = Keyboard.current[jumpKey].wasPressedThisFrame;
        jumpHeld = Keyboard.current[jumpKey].isPressed;
        jumpReleased = Keyboard.current[jumpKey].wasReleasedThisFrame;
        dashPressed = Keyboard.current[dashKey].wasPressedThisFrame;
        interactPressed = Keyboard.current[interactKey].wasPressedThisFrame;
    }
}
