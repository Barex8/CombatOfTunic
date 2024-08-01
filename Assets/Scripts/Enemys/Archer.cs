using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Archer : MonoBehaviour
    {
    private Rigidbody rb;
    private Animator anim;
    private Vector3 mov = new Vector3(0, 0, 0);

    private Vector3 initialPosition;
    [SerializeField] private float maxLenghtToMove;

    [Header("MoveIdle")]
    [SerializeField] private float velocity;
    [SerializeField] private float moveForward;
    [SerializeField] private float maxTimeToTarget;
    [SerializeField] private float distanceWithTarget;
    [SerializeField] private float TimeBetweenJumps;


    [Header("MoveToPlayer")]
    [SerializeField] private float velocityToPlayer;
    [SerializeField] private float distanceLocalizedPlayer;

    [SerializeField] private float waitAfterAttack;

    [SerializeField] private float attackCooldown;

    [SerializeField] bool playerLocalized;
    [SerializeField] private LayerMask playerLayer;
    bool moveStarted = false;

    Transform player;
    Damageable damageable;

    [Header("NavMesh")]
    private NavMeshAgent agent;
    [SerializeField] private float closeEnoughMeters = 4f;

    [SerializeReference] private GameObject arrow;
    [SerializeField] private float speedBullet;

    private void Start()
        {
        damageable = transform.GetComponent<Damageable>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        initialPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        }

    private void Update()
        {
        Vector3 originRayToPlayer = new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z);
        if (Physics.Raycast(originRayToPlayer, player.position - transform.position, out RaycastHit rayToPlayer, distanceLocalizedPlayer, playerLayer))
            {
            playerLocalized = true;
            }
        else playerLocalized = false;
        Debug.DrawRay(originRayToPlayer, player.position - transform.position, Color.yellow);

        if (!moveStarted)
            {
            if (playerLocalized) StartCoroutine(MoveToPlayer());
            else StartCoroutine(MoveIdle());
            }
        rb.velocity = mov;
        }

    private IEnumerator MoveToPlayer()
        {
        moveStarted = true;
        anim.SetBool("Run", true);

        agent.isStopped = false;
        agent.speed = velocityToPlayer;

        while (Vector3.Distance(transform.position, player.position) > closeEnoughMeters)
            {

            initialPosition = transform.position;
            if (!playerLocalized || damageable.hitRecived)
                {
                moveStarted = false;
                damageable.hitRecived = false;
                print("Adioooooooos");
                anim.SetTrigger("GetHit");
                yield break;
                }
            agent.destination = player.position;
            yield return null;
            }
        StartCoroutine(Attack());
        }

    private IEnumerator Attack()
        {
        anim.SetBool("Run", false);
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        transform.LookAt(player);
        anim.SetTrigger("Attack");
        float elapsedTime = 0;
        GameObject bullet = Instantiate(arrow, transform.position, transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = transform.forward * speedBullet;
        while (elapsedTime < waitAfterAttack)
            {
            elapsedTime += Time.deltaTime;
            if (damageable.hitRecived)
                {
                damageable.hitRecived = false;
                anim.ResetTrigger("Attack");
                anim.SetTrigger("GetHit");
                yield return new WaitForSeconds(.3f);
                moveStarted = false;
                yield break;
                }
            yield return null;
            }
        damageable.hitRecived = false;

        moveStarted = false;
        }

    private IEnumerator MoveIdle()
        {
        moveStarted = true;
        anim.SetBool("Run", true);
        Vector3 targetPos;

        bool targetGrounded = true;
        do
            {
            targetPos = GetRandomPosition(transform.position);                //Bucle Infinito
            if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit))
                {
                targetGrounded = hit.transform.CompareTag("Water");
                }
            agent.speed = velocity;
            } while (Vector3.Distance(targetPos, initialPosition) > maxLenghtToMove && targetGrounded);

        transform.LookAt(targetPos);
        float elapsedTime = 0;
        agent.isStopped = false;

        while (Vector3.Distance(transform.position, targetPos) > distanceWithTarget && elapsedTime <= maxTimeToTarget)
            {
            if (damageable.hitRecived)
                {
                damageable.hitRecived = false;
                yield break;
                }
            agent.destination = targetPos;
            elapsedTime += Time.deltaTime;
            yield return null;
            }
        anim.SetBool("Run", false);
        agent.isStopped = true;
        elapsedTime = 0;
        while (elapsedTime < TimeBetweenJumps)
            {
            if (playerLocalized) break;
            elapsedTime += Time.deltaTime;
            yield return null;
            }

        moveStarted = false;
        }


    Vector3 GetRandomPosition(Vector3 origin)
        {
        // Obtener una dirección aleatoria
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.Normalize();

        // Calcular la nueva posición
        Vector3 randomPosition = origin + (randomDirection * moveForward);
        randomPosition.y = transform.position.y;
        //print(randomPosition);

        return randomPosition;
        }
    }
