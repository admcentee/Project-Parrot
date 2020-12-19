using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RigidbodyFPSController : MonoBehaviour
{

    public float walkSpeed = 20f;
    public float sprintMultiplier = 1.75f;
    public float maxVelocity = 100;
    public float jumpVelocity = 50f;
    public float jumpRate = 1f;
    public int jumpLimit = 2;
    public float jumpDecayWeight = 0.75f;
    public Transform groundCheck;
    public float groundDistance = 0.25f;
    public LayerMask playerMask;
    public LayerMask groundMask;
    public AudioClip walk;
    public AudioClip landing;

    // public AudioClip sliding;

    private Rigidbody rb;
    private float x = 0;
    private float z = 0;
    private bool sprintFlag = false;
    private bool jumpFlag = false;
    private bool grounded = false;
    private AudioSource audioSource;
    private float curSpeed;
    private float jumpCooldownCounter = 0f;

    private new Collider collider;
    private bool pulled = false;
    private bool active = false;
    private bool initialized = true;
    private Vector3 lastDir = Vector3.zero;
    // private float elapsedTime = 0;
    private int jumpCounter = 0;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider>();
        rb.useGravity = false;
    }
    void Update()
    {
        if (jumpCooldownCounter >= 0) jumpCooldownCounter -= Time.deltaTime;
    }
    void FixedUpdate()
    {
            if (!active && initialized)
            {
                StartCoroutine(processInputs());
            }
    }

    IEnumerator processInputs()
    {
        // elapsedTime = 0;
        active = true;
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        sprintFlag = Input.GetKey(KeyCode.LeftShift);
        jumpFlag = Input.GetKey(KeyCode.Space);
        float y = 0f;
        if (Input.GetKey(KeyCode.Space) && !rb.useGravity) y = 1f;
        else if (Input.GetKey(KeyCode.X) && !rb.useGravity) y = -1f;

        yield return null;

        // Determine walk speed
        curSpeed = walkSpeed;
        if (sprintFlag) curSpeed *= sprintMultiplier;

        // Check for ground contact

        // RaycastHit[] collisions;
        // if(Physics.SphereCast(groundCheck.position, groundDistance, -Vector3.up, out collisions, 0, ~gameObject.layer));
        // Collider[] cols = Physics.OverlapSphere(groundCheck.position, groundDistance, ~playerMask);
        bool temp = Physics.CheckSphere(groundCheck.position, groundDistance, ~playerMask);
        // Vector3 normal = (groundCheck.position - cols[0].ClosestPoint(groundCheck.position)).normalized;

        // Play landing on change
        if (temp != grounded)
        {
            if (temp && landing != null)
            {
                audioSource.volume = 0.75f;
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(landing);
            }
            // else
            // {
            //     audioSource.Stop();
            // }

        }
        grounded = temp;
        if (grounded) jumpCounter = 0;
        else if (audioSource != null) audioSource.Stop();


        Vector3 curDir;
        curDir = x * transform.right + y * transform.up + z * transform.forward;
        curDir *= curSpeed * Time.fixedDeltaTime;

        // Player Mode
        if (rb.useGravity)
        {
            // Jump if applicable

            if (jumpFlag && jumpCooldownCounter <= 0 && jumpCounter < jumpLimit)
            {
                if (!grounded && jumpCounter == 0) jumpCounter = 1;
                jumpCounter++;
                jumpCooldownCounter = 1f / jumpRate;
                Jump(Vector3.up, jumpCounter);
                yield return null;
            }

            // Moving
            if (x != 0 || z != 0)
            {
                RaycastHit destination;
                //* curDir.magnitude / curSpeed
                if (grounded && Physics.Raycast(transform.position + curDir + transform.up * collider.bounds.size.y * 1.5f, -transform.up, out destination, collider.bounds.size.y * 3f, ~playerMask))
                {
                    MovePos(destination.point, sprintFlag);

                    yield return null;
                }
                else if (!Physics.Raycast(transform.position, curDir.normalized, curDir.magnitude))
                {
                    MoveDir(curDir / 1.5f, sprintFlag);
                }

            }

            // Standing still
            else if (grounded || (pulled && !grounded))
            {
                if (grounded && !pulled) rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 10f * Time.fixedDeltaTime);
                if (audioSource != null) audioSource.Stop();
            }
            if (rb.velocity.magnitude > maxVelocity) rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.normalized * maxVelocity, 20f * Time.deltaTime);

        }
        // Admin Mode
        else
        {
            // Moving
            if ((x != 0 || y != 0 || z != 0))
            {
                MoveDir(curDir, sprintFlag);
            }
            else if (!jumpFlag && audioSource != null)
            {
                audioSource.Stop();
            }
        }
        active = false;
        yield break;
    }
    public void Initialize()
    {
        initialized = true;
    }
    void MoveDir(Vector3 input, bool sf)
    {
        // Pitch footsteps according to walk speed
        if (audioSource != null){
            float pitchMod = rb.velocity.magnitude / 25f;
            if (pitchMod > sprintMultiplier) audioSource.pitch = Mathf.Clamp(rb.velocity.magnitude / 25f, 1f, sprintMultiplier * 2);
            else if (sf) audioSource.pitch = sprintMultiplier;
            else audioSource.pitch = 1f;
        }
        // yield return null;
        if (audioSource != null && walk != null)
        {
            if (!audioSource.isPlaying && grounded) audioSource.PlayOneShot(walk);
        }

        rb.MovePosition(transform.position + input);
    }
    void MovePos(Vector3 destination, bool sf)
    {
        // Pitch footsteps according to walk speed
        float pitchMod = rb.velocity.magnitude / 25f;
        if (pitchMod > sprintMultiplier) audioSource.pitch = Mathf.Clamp(rb.velocity.magnitude / 25f, 1f, sprintMultiplier * 2);
        else if (sf) audioSource.pitch = sprintMultiplier;
        else audioSource.pitch = 1f;
        // yield return null;
        if (audioSource != null && walk != null)
        {
            if (!audioSource.isPlaying && grounded) audioSource.PlayOneShot(walk);
        }
        rb.MovePosition(destination);
    }
    void Jump(Vector3 normal, int jumpCounter)
    {
        float vel = jumpVelocity / (jumpDecayWeight * jumpCounter);
        if (jumpCounter == 1) vel = jumpVelocity;
        if (pulled)
        {
            rb.AddForce(normal * vel, ForceMode.VelocityChange);
            grounded = false;
        }
        else
        {
            rb.AddForce(Vector3.up * vel, ForceMode.VelocityChange);
            grounded = false;
        }

    }
    public void setPullState(bool state)
    {
        pulled = state;
    }
}

