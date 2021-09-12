using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Character2D : MonoBehaviour
{
    private struct RaycastOrigins
    {
        public Vector2 TopLeft;
        public Vector2 BottomRight;
        public Vector2 BottomLeft;
    }

    public struct CollisionState2D
    {
        [Flags]
        public enum DirectionFlag
        {
            Right = 1,
            Left = 2,
            Above = 4,
            Below = 8
        }

        public DirectionFlag CollidedDirections;
        public bool IsGroundedThisFrame;
        public bool IsGroundedLastFrame;
        public bool IsMovingDownSlope;
        public float SlopeAngle;

        public bool HasCollision => CollidedDirections > 0;
        public bool IsGrounded => (CollidedDirections & DirectionFlag.Below) == DirectionFlag.Below;

        public void Reset()
        {
            CollidedDirections = 0;
            IsGroundedThisFrame = IsMovingDownSlope = false;
            SlopeAngle = 0.0f;
        }

        public override string ToString()
        {
            return
                "[CharacterCollisionState2D]\n" +
                $"\tDirectionState: {CollidedDirections}\n" +
                $"\tIs moving down a slope?: {IsMovingDownSlope}\n" + $"\tSlope angle: {SlopeAngle}\n" +
                $"\tGrounded last frame?: {IsGroundedLastFrame}\n" + $"\tGrounded this frame?: {IsGroundedThisFrame}";
        }
    }

    // public event Action<RaycastHit2D> OnCharacterCollidedEvent;
    // public event Action<Collider2D> OnTriggerEnterEvent;
    // public event Action<Collider2D> OnTriggerStayEvent;
    // public event Action<Collider2D> OnTriggerExitEvent;

    [SerializeField]
    [Tooltip("Defines how far in from the edges of the collider rays are cast from.")]
    [Range(0.001f, 0.3f)]
    private float skinWidth = 0.02f;

    [SerializeField] [Tooltip("Mask with all layers that the player should interact with.")]
    private LayerMask platformMask = 0;

    [SerializeField] [Tooltip("mask with all layers that trigger events should fire when intersected")]
    private LayerMask triggerMask = 0;

    [SerializeField]
    [Tooltip("Mask with all layers that should act as one-way platforms. Note that one-way platforms should always " +
             "be EdgeCollider2Ds.This is because it does not support being updated anytime outside of the inspector.")]
    private LayerMask oneWayPlatformMask = 0;

    [SerializeField] [Tooltip("The max slope angle that the character can climb.")] [Range(0.01f, 89.99f)]
    private float slopeLimitAngle = 3.0f;

    [SerializeField] [Range(2, 20)] private int totalHorizontalRays = 8;
    [SerializeField] [Range(2, 20)] private int totalVerticalRays = 4;

    private const float SkinWidthFloatNudge = 0.001f;

    private BoxCollider2D _collider;
    private Rigidbody2D _body;

    private float _slopeLimit;
    private CollisionState2D _collisionState;
    private RaycastOrigins _raycastOrigins;

    // stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
    // horizontally and vertically so that we can send the events after all collision state is set
    private List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

    // horizontal/vertical movement data
    private float _verticalDistanceBetweenRays;
    private float _horizontalDistanceBetweenRays;

    public Vector2 Velocity { get; private set; }
    public CollisionState2D CollisionState => _collisionState;
    public IReadOnlyList<RaycastHit2D> RaycastHitsThisFrame => _raycastHitsThisFrame;


    /// <summary>
    /// Defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often
    /// result in ray hits that are not desired (for example a foot collider casting horizontally from directly on the
    /// surface can result in a hit)
    /// </summary>
    public float SkinWidth
    {
        get => skinWidth;
        set
        {
            skinWidth = value;
            RecalculateDistanceBetweenRays();
        }
    }

    public bool IsGrounded => _collisionState.IsGrounded;
    public bool IgnoreOneWayPlatformsThisFrame { get; set; }

    private void Awake()
    {
        // add our one-way platforms to our normal platform mask so that we can land on them from above
        platformMask |= oneWayPlatformMask;

        // cache some components
        _collider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();
        _body.bodyType = RigidbodyType2D.Kinematic;

        // here, we trigger our properties that have setters with bodies
        RecalculateDistanceBetweenRays();

        // we want to set our character to ignore all collision layers except what is in our triggerMask
        for (var i = 0; i < 32; i++)
        {
            // see if our triggerMask contains this layer and if not ignore it
            if ((triggerMask.value & 1 << i) == 0)
                Physics2D.IgnoreLayerCollision(gameObject.layer, i);
        }
    }

    // private void OnTriggerEnter2D(Collider2D col)
    // {
    //     OnTriggerEnterEvent?.Invoke(col);
    // }
    //
    //
    // private void OnTriggerStay2D(Collider2D col)
    // {
    //     OnTriggerStayEvent?.Invoke(col);
    // }
    //
    //
    // private void OnTriggerExit2D(Collider2D col)
    // {
    //     OnTriggerExitEvent?.Invoke(col);
    // }

    public void Move(Vector2 velocity)
    {
        // save off our current grounded state which we will use for IsGroundedLastFrame and IsGroundedThisFrame
        _collisionState.IsGroundedLastFrame = _collisionState.IsGrounded;

        // clear the state
        _collisionState.Reset();
        _raycastHitsThisFrame.Clear();

        var motion = velocity * Time.deltaTime;

        CalculateRaycastOrigins();
        // first, we check for a slope below us before moving
        // only check slopes if we are going down and grounded
        if (motion.y < 0.0f && _collisionState.IsGroundedLastFrame)
            HandleVerticalSlope(ref motion);

        // now we check movement in the horizontal dir
        if (motion.x != 0.0f)
            MoveHorizontally(ref motion);

        // next, check movement in the vertical dir
        if (motion.y != 0.0f)
            MoveVertically(ref motion);

        // move then update our state
        _body.MovePosition(_body.position + motion);

        // only calculate velocity if we have a non-zero deltaTime (when timeScale is greater than zero).
        if (Time.deltaTime > 0f)
            Velocity = motion / Time.deltaTime;

        // set our becameGrounded state based on the previous and current collision state
        if (!_collisionState.IsGroundedLastFrame && _collisionState.IsGrounded)
            _collisionState.IsGroundedThisFrame = true;

        // send off the collision events if we have a listener
        // if (OnCharacterCollidedEvent != null)
        // {
        //     foreach (var hit in RaycastHitsThisFrame)
        //         OnCharacterCollidedEvent(hit);
        // }

        IgnoreOneWayPlatformsThisFrame = false;
    }

    /// <summary>
    /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance
    /// between the rays used for collision detection.
    /// It is also used in the skinWidth setter in case it is changed at runtime.
    /// </summary>
    public void RecalculateDistanceBetweenRays()
    {
        // figure out the distance between our rays in both directions
        // horizontal
        var localScale = transform.localScale;
        var colliderUsableHeight = _collider.size.y * Mathf.Abs(localScale.y) - 2f * skinWidth;
        _verticalDistanceBetweenRays = colliderUsableHeight / (totalHorizontalRays - 1);

        // vertical
        var colliderUsableWidth = _collider.size.x * Mathf.Abs(localScale.x) - 2f * skinWidth;
        _horizontalDistanceBetweenRays = colliderUsableWidth / (totalVerticalRays - 1);
    }

    private void CalculateRaycastOrigins()
    {
        // resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
        // to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
        // our raycasts need to be fired from the bounds inset by the skinWidth
        var modifiedBounds = _collider.bounds;
        modifiedBounds.Expand(-2.0f * skinWidth);

        _raycastOrigins.TopLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.BottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _raycastOrigins.BottomLeft = modifiedBounds.min;
    }

    private void HandleVerticalSlope(ref Vector2 motion)
    {
        // checks the center point under the BoxCollider2D for a slope. If it finds one then the motion is adjusted so
        // that the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.

        // slope check from the center of our collider
        var centerOfCollider = (_raycastOrigins.BottomLeft.x + _raycastOrigins.BottomRight.x) * 0.5f;
        var rayDirection = -Vector2.up;

        // the ray distance is based on our slopeLimit
        var slopeCheckRayDistance = _slopeLimit * (_raycastOrigins.BottomRight.x - centerOfCollider);

        var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.BottomLeft.y);

        DebugDrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);

        var raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
        // Ensure valid raycast result.
        if (!raycastHit) return;
        // bail out if we have no slope
        var angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if (angle == 0) return;

        // we are moving down the slope if our normal and movement direction are in the same x direction
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Mathf.Sign(raycastHit.normal.x) != Mathf.Sign(motion.x)) return;
        // we add the extra downward movement here to ensure we "stick" to the surface below
        motion.y += raycastHit.point.y - slopeRay.y - skinWidth;
        motion = new Vector3(0, motion.y, 0) +
                 Quaternion.AngleAxis(-angle, Vector3.forward) * new Vector3(motion.x, 0, 0);
        _collisionState.IsMovingDownSlope = true;
        _collisionState.SlopeAngle = angle;
    }

    private void MoveHorizontally(ref Vector2 motion)
    {
        // we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
        // collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
        // we have to increase the ray distance skinWidth then remember to remove skinWidth from motion before
        // actually moving the player

        var isGoingRight = motion.x > 0;
        var rayDistance = Mathf.Abs(motion.x) + skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var initialRayOrigin = isGoingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;

        for (var i = 0; i < totalHorizontalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

            DebugDrawRay(ray, rayDirection * rayDistance, Color.red);

            // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will
            // allow us to walk up sloped oneWayPlatforms
            RaycastHit2D raycastHit;
            if (i == 0 && _collisionState.IsGroundedLastFrame)
                raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
            else
                raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask);

            // Ensure valid raycast result.
            if (!raycastHit) continue;

            // the bottom ray can hit a slope but no other ray can so we have special handling for these cases
            if (i == 0 && HandleHorizontalSlope(ref motion, Vector2.Angle(raycastHit.normal, Vector2.up)))
            {
                _raycastHitsThisFrame.Add(raycastHit);
                // if we weren't grounded last frame, that means we're landing on a slope horizontally.
                // this ensures that we stay flush to that slope
                if (!_collisionState.IsGroundedLastFrame)
                {
                    var flushDistance = Mathf.Sign(motion.x) * (raycastHit.distance - skinWidth);
                    _body.MovePosition(transform.TransformPoint(new Vector2(flushDistance, 0)));
                }

                break;
            }

            // set our new motion and recalculate the rayDistance taking it into account
            motion.x = raycastHit.point.x - ray.x;
            rayDistance = Mathf.Abs(motion.x);

            // remember to remove the skinWidth from our motion
            if (isGoingRight)
            {
                motion.x -= skinWidth;
                // Set right direction bit to true.
                _collisionState.CollidedDirections |= CollisionState2D.DirectionFlag.Right;
            }
            else
            {
                motion.x += skinWidth;
                // Set left direction bit to true.
                _collisionState.CollidedDirections |= CollisionState2D.DirectionFlag.Left;
            }

            _raycastHitsThisFrame.Add(raycastHit);

            // we add a small nudge for the float operations here. if our rayDistance is smaller
            // than the width + nudge bail out because we have a direct impact
            if (rayDistance < skinWidth + SkinWidthFloatNudge)
                break;
        }
    }

    private bool HandleHorizontalSlope(ref Vector2 motion, float angle)
    {
        // handles adjusting motion if we are going up a slope. Returns true, if horizontal slope was handled,
        // false otherwise.

        // disregard 90 degree angles (walls)
        if (Mathf.Approximately(angle, 90.0f)) return false;

        // if we can walk on slopes and our angle is small enough we need to move up
        if (angle < slopeLimitAngle)
        {
            // we only need to adjust the motion if we are not jumping
            // TODO: this uses a magic number which isn't ideal!
            // The alternative is to have the user pass in if there is a jump this frame
            // if (!(motion.y < jumpingThreshold)) return true;

            // we dont set collisions on the sides for this since a slope is not technically a side collision.
            // smooth y movement when we climb. we make the y movement equivalent to the actual y location that
            // corresponds to our new x location.
            motion.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * motion.x);
            var isGoingRight = motion.x > 0;

            // safety check. we fire a ray in the direction of movement just in case the diagonal we calculated above
            // ends up going through a wall. If the ray hits, we back off the horizontal movement to stay in bounds.
            var ray = isGoingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;
            var raycastHit = _collisionState.IsGroundedLastFrame
                ? Physics2D.Raycast(ray, motion.normalized, motion.magnitude, platformMask)
                : Physics2D.Raycast(ray, motion.normalized, motion.magnitude,
                    platformMask & ~oneWayPlatformMask);

            if (raycastHit)
            {
                // we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the
                // ray hit location
                motion = raycastHit.point - ray;
                if (isGoingRight)
                    motion.x -= skinWidth;
                else
                    motion.x += skinWidth;
            }

            // Set below direction bit to true.
            _collisionState.CollidedDirections |= CollisionState2D.DirectionFlag.Below;
            _collisionState.SlopeAngle = -angle;
        }
        else // too steep. get out of here
        {
            motion.x = 0;
        }

        return true;
    }

    private void MoveVertically(ref Vector2 motion)
    {
        var isGoingUp = motion.y > 0;
        var rayDistance = Mathf.Abs(motion.y) + skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var initialRayOrigin = isGoingUp ? _raycastOrigins.TopLeft : _raycastOrigins.BottomLeft;

        // apply our horizontal motion here so that we do our raycast from the actual position we would be in
        // if we had moved. Assumes horizontal movement is processed 1st.
        initialRayOrigin.x += motion.x;

        // if we are moving up, we should ignore the layers in oneWayPlatformMask
        var mask = platformMask;
        if (isGoingUp && !_collisionState.IsGroundedLastFrame || IgnoreOneWayPlatformsThisFrame)
            mask &= ~oneWayPlatformMask;

        for (var i = 0; i < totalVerticalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

            DebugDrawRay(ray, rayDirection * rayDistance, Color.red);
            var raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

            // Ensure valid raycast result.
            if (!raycastHit) continue;

            // set our new motion and recalculate the rayDistance taking it into account
            motion.y = raycastHit.point.y - ray.y;
            rayDistance = Mathf.Abs(motion.y);

            // remember to remove the skinWidth from our motion
            if (isGoingUp)
            {
                motion.y -= skinWidth;
                // Set above direction bit to true.
                _collisionState.CollidedDirections |= CollisionState2D.DirectionFlag.Above;
            }
            else
            {
                motion.y += skinWidth;
                // Set above direction bit to true.
                _collisionState.CollidedDirections |= CollisionState2D.DirectionFlag.Below;
            }

            _raycastHitsThisFrame.Add(raycastHit);

            // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a
            // situation where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame
            // due to residual velocity.
            if (!isGoingUp && motion.y > 0.00001f)
                motion.y = 0.0f;

            // we add a small nudge for the float operations here. if our rayDistance is smaller
            // than the width + nudge bail out because we have a direct impact
            if (rayDistance < skinWidth + SkinWidthFloatNudge)
                break;
        }
    }

    [Conditional("DEBUG")]
    private static void DebugDrawRay(Vector2 start, Vector2 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }
}