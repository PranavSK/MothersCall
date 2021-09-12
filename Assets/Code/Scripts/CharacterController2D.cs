using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float friction;
    
    private Character2D _character;

    private Vector2 _desiredMoveDirection;

    private void Awake()
    {
        _character = GetComponent<Character2D>();
    }

    private void FixedUpdate()
    {
        var desiredVelocity = _desiredMoveDirection * maxSpeed * Time.fixedDeltaTime;
        if (!_character.IsGrounded)
        {
            desiredVelocity += Physics2D.gravity;
        }
        _character.Move(desiredVelocity);
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        var xDirection = context.ReadValue<float>();
        _desiredMoveDirection = new Vector2(xDirection, 0.0f);
    } 
}