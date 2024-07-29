using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Damageable
{
    private Animator anim;
    private Vector3 mov;
    [SerializeField] private Vector3 boxCheckGroundSize;
    [SerializeField]private LayerMask groundLayerMask;

    [SerializeField] float runSpeed;
    [SerializeField] private float forceDashAttack;
    float currentSpeed;

    private bool canAttack = true;
    private bool canMove = true;

    
    [SerializeField] private float timeDashingAttack;

    private bool canDash = true;
    private bool isDashing = false;
    [SerializeField] private float dashingTime;
    [SerializeField] private float forceDashing;
    Damageable damageable;

    float bufferAttack;

    Vector3 mouseScreenPosition;

    [SerializeField]private bool frozen=false;

    private void Start()
        {
        damageable = GetComponent<Damageable>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentSpeed = runSpeed;

        hitable = true;
        normalColor = sMR.material.color;

        }

    private void Update()
        {
        CoolDownBuffers();
        
        if (frozen) return;
        
        if (!checkGround())
            {
            rb.velocity = new Vector3(0, -9.8f, 0);
            return;
            }

        
        if(canMove)Move();

        if (isDashing)return;

        if ((Input.GetKeyDown("mouse 0")&&canAttack) || bufferAttack > 0)
            {
            StartCoroutine(Attack());
            }
        if (Input.GetKeyDown("space") && canDash)
            {
            StartCoroutine(Dash());
            }
        }

    bool checkGround()
        {
        if (Physics.CheckBox(transform.position,boxCheckGroundSize,Quaternion.identity,groundLayerMask))
            {
            return true;
            }
        return false;
        }

    void CoolDownBuffers()
        {
        if (Input.GetKeyDown("mouse 0"))
            {
            bufferAttack = 0.6f;
            }
        bufferAttack = bufferAttack > 0 ? bufferAttack -= Time.deltaTime : 0;
        }

    void Move()
        {
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");
        mov = new Vector3(axisX,0f, axisY).normalized;

        if (mov != Vector3.zero)
            {
            anim.SetBool("Run", true);
            Quaternion newRotation = Quaternion.LookRotation(mov);
            rb.rotation = newRotation;
            }
        else
            {
            anim.SetBool("Run", false);
            }

        if (!CheckGroundForward()) return;
        mov = AdjustVelocityToSlope(mov);
        rb.velocity = mov*currentSpeed;        
        }

    bool CheckGroundForward()
        {
        Vector3 originRayGroundForward = new Vector3(transform.position.x, transform.position.y + heightRayCheckGroundForward, transform.position.z);
        Vector3 directionRayGroundForward = new Vector3(transform.forward.x, -transform.up.y, transform.forward.z);
        Debug.DrawRay(originRayGroundForward, directionRayGroundForward, Color.red);
        if (Physics.Raycast(originRayGroundForward, directionRayGroundForward, out RaycastHit hit))
            {
            if (hit.transform.CompareTag("Water"))
                {
                rb.velocity = Vector3.zero;
                return false;
                }
            }
        return true;
        }

    IEnumerator Attack()
        {
        bufferAttack = 0;
        canMove = false;
        canAttack = false;
        rb.velocity = Vector3.zero;
        anim.SetTrigger("Attack");
        LookToMouseWorldSpace();
        float elapsedTime = 0;
        while (elapsedTime < timeDashingAttack)
            {
            if (isDashing) break;
            elapsedTime += Time.deltaTime;
            yield return null;
            }
        canAttack = true;
        canMove = true;
        }

    IEnumerator Dash()
        {
        canDash = false;
        isDashing = true;
        damageable.hitable = false;
        

        float elapsedTime = 0;
        anim.SetTrigger("Roll");
        yield return new WaitForSeconds(0.1f);
        currentSpeed = forceDashing;
        while (elapsedTime < dashingTime)
            {
            if (!CheckGroundForward()) break;
            rb.velocity = transform.forward * currentSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
            }
        canDash = true;
        isDashing = false;
        damageable.hitable = true;
        currentSpeed = runSpeed;
        }

    void LookToMouseWorldSpace()
        {
        mouseScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
            Vector3 pointWS = hit.point;
            pointWS.y = transform.position.y;
            transform.LookAt(pointWS);
            }
        }

    public override IEnumerator PushBack(Vector3 posHit)
        {
        float elapsedTime = 0;
        anim.SetTrigger("GetHit");
        posHit.y = transform.position.y;
        while (elapsedTime < pushBackTime)
            {
            frozen = true;

            if (CheckGroundBackward())
                {
                if (rb != null) rb.velocity = (transform.position - posHit) * pushBackForce;
                }
            else
                {
                rb.velocity = Vector3.zero;
                }
            elapsedTime += Time.deltaTime;
            transform.LookAt(posHit);
            yield return null;
            }
        rb.velocity = Vector3.zero;
        frozen = false;
        yield return null;
        }


    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
        {
        var ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray,out RaycastHit hitInfo,0.2f))
            {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            var adjustedVelocity = slopeRotation * velocity;
            if (adjustedVelocity.y <0)
                {
                return adjustedVelocity;
                }
            }
        return velocity;
        }

    private void OnDrawGizmos()
        {
        Gizmos.DrawWireCube(transform.position, boxCheckGroundSize);
        }
    }


    
