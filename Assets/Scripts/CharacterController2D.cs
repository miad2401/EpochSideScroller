using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float jumpForce = 400f;                                        // Amount of force added when jumping.
    [Range(0, 1)] [SerializeField] private float crouchSpeed = .36f;                        // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;                // How much to smooth out the movement.
    [SerializeField] private bool airControl = false;                                       // Whether or not a player cna steer while jumping.
    [SerializeField] private Transform groundCheck;                                         // A position marking to check if the player is grounded.
    [SerializeField] private Transform ceilingCheck;                                        // A position marking where to check for ceilings.
    [SerializeField] private LayerMask whatIsGround;                                        // A mask determinging what is ground to the character.
    [SerializeField] private Collider2D crouchDisableCollider;                              // A collider that will be sidabled when crouching.

    const float groundedRadius = .2f;               // Radius of the overlap circle to determine if grounded.
    private bool grounded;                          // Whether or not the player is grounded.
    const float ceilingRadius = .2f;                // Radius of the overlap circle to determine if the player can stand up.
    private Rigidbody2D rigidbody;
    private bool facingRight = true;                // For determining which way the player is currently facing.
    private Vector3 velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool wasCrouching = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
        {
            OnLandEvent = new UnityEvent();
        }

        if (OnCrouchEvent == null)
        {
            OnCrouchEvent = new BoolEvent();
        }
    }

    private void FixedUpdate()
    {
        bool wasGrounded = grounded;
        grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
                if (!wasGrounded)
                {
                    OnLandEvent.Invoke();
                }
            }
        }
    }

    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
            {
                crouch = true;
            }
        }

        // Only control the player if grounded or airControl is turned on
        if (grounded || airControl)
        {
            // If crouching
            if (crouch)
            {
                if (!wasCrouching)
                {
                    wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= crouchSpeed;

                // Disable one of the colliders when crouching
                if (crouchDisableCollider != null)
                {
                    crouchDisableCollider.enabled = false;
                }
            } else
            {
                if (crouchDisableCollider != null)
                {
                    crouchDisableCollider.enabled = true;
                }

                if (wasCrouching)
                {
                    wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, rigidbody.velocity.y);
            // And then smoothing it out and applying it to the character
            rigidbody.velocity = Vector3.SmoothDamp(rigidbody.velocity, targetVelocity, ref velocity, movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !facingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && facingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (grounded && jump)
        {
            // Add a vertical force to the player.
            grounded = false;
            rigidbody.AddForce(new Vector2(0f, jumpForce));
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
