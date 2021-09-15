using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character2D), typeof(Animator), typeof(SpriteRenderer))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] public float maxSpeed;
    // [SerializeField] private float acceleration;
    // [SerializeField] private float friction;

    private static readonly int AnimIdSpeed = Animator.StringToHash("speed");
    
    private Character2D _character;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _desiredMoveDirection;

    private void Awake()
    {
        _character = GetComponent<Character2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        var desiredVelocity = _desiredMoveDirection * maxSpeed * Time.fixedDeltaTime;
        if (!_character.IsGrounded)
        {
            desiredVelocity += Physics2D.gravity;
        }

        _character.Move(desiredVelocity);
        _animator.SetFloat(AnimIdSpeed, Mathf.Abs(_character.Velocity.x));
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        var xDirection = context.ReadValue<float>();
        _desiredMoveDirection = new Vector2(xDirection, 0.0f);

        // Set sprite direction - Ignore if zero input
        if (xDirection < 0) _spriteRenderer.flipX = true;
        else if (xDirection > 0) _spriteRenderer.flipX = false;
    }
}