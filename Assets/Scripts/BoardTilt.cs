using UnityEngine;
using UnityEngine.InputSystem;

// Tilts the board with WASD / arrow keys or a gamepad left stick.
// Put this on the "Board" parent object. The floor and walls are its children.
// The ball is NOT a child of the board.
[RequireComponent(typeof(Rigidbody))]
public class BoardTilt : MonoBehaviour
{
    [Header("Tilt")]
    [Tooltip("Max tilt angle in degrees")]
    public float maxTilt = 12f;

    [Tooltip("How fast the board turns toward the target angle (degrees per second)")]
    public float tiltSpeed = 60f;

    [Tooltip("Gamepad stick values smaller than this are ignored")]
    public float deadzone = 0.15f;

    Rigidbody rb;
    Quaternion startRotation;
    Vector2 input;        // x = left/right, y = forward/back
    Vector2 currentTilt;  // x = angle around X axis, y = angle around Z axis

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Kinematic: the board is moved by this script, not pushed by physics.
        // Moving it with MoveRotation keeps collisions with the ball smooth.
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startRotation = transform.rotation;
    }

    void Update()
    {
        input = ReadInput();
    }

    void FixedUpdate()
    {
        // Forward input lowers the far side, right input lowers the right side.
        Vector2 target = new Vector2(input.y * maxTilt, -input.x * maxTilt);
        currentTilt = Vector2.MoveTowards(currentTilt, target, tiltSpeed * Time.fixedDeltaTime);

        Quaternion tilt = Quaternion.Euler(currentTilt.x, 0f, currentTilt.y);
        rb.MoveRotation(startRotation * tilt);
    }

    Vector2 ReadInput()
    {
        Vector2 v = Vector2.zero;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 stick = pad.leftStick.ReadValue();
            if (stick.magnitude > deadzone) v += stick;
        }

        return Vector2.ClampMagnitude(v, 1f);
    }

    // Snap the board back to flat (useful when the level restarts).
    public void ResetTilt()
    {
        currentTilt = Vector2.zero;
        rb.MoveRotation(startRotation);
    }
}