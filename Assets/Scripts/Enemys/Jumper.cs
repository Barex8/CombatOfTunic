using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Jumper : MonoBehaviour
{
    private Rigidbody rb;
    private Animator anim;
    private Vector3 mov = new Vector3(0,0,0);

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
    [SerializeField] private float maxTimeToTargetPlayer;
    [SerializeField] private float distanceWithPlayer;
    [SerializeField] private float minDistanceWithPlayer;
    [SerializeField] private float TimeBetweenJumpsToPlayer;

    [SerializeField] bool playerLocalized;
    [SerializeField] private LayerMask playerLayer;
    bool moveStarted = false;

    public Transform player;

    private NavMeshAgent agent;


    private void Start()
        {
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        initialPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        }

    private void Update()
        {
        Vector3 originRayToPlayer = new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z);
        if (Physics.Raycast(originRayToPlayer, player.position-transform.position, out RaycastHit rayToPlayer,distanceWithPlayer,playerLayer))
            {
            playerLocalized = true;
            }
        else playerLocalized = false;
        Debug.DrawRay(originRayToPlayer, player.position - transform.position, Color.yellow);

        if (!moveStarted)
            {
            if (playerLocalized)StartCoroutine(MoveToPlayer());
            else StartCoroutine(MoveIdle());
            }
        rb.velocity = mov;
        }

    private IEnumerator MoveToPlayer()
        {
        moveStarted = true;
        anim.SetTrigger("Jump");

        float elapsedTime = 0;
        agent.isStopped = false;
        agent.speed = velocityToPlayer;

        while (Vector3.Distance(transform.position, player.position) > minDistanceWithPlayer && elapsedTime <= maxTimeToTargetPlayer)
            {
            
            initialPosition = transform.position;
            if (!playerLocalized)
                {
                moveStarted = false;
                yield break;
                }
            agent.destination = player.position;
            elapsedTime += Time.deltaTime;
            yield return null;
            }
        agent.isStopped = true;
        yield return new WaitForSeconds(TimeBetweenJumpsToPlayer);
        moveStarted = false;

        }

    private IEnumerator MoveIdle()
        {
        moveStarted = true;
        anim.SetTrigger("Jump");
        Vector3 targetPos;

        bool targetGrounded = true;
        do
            {
            targetPos = GetRandomPosition(transform.position);                //Bucle Infinito
            if (Physics.Raycast(targetPos, Vector3.down,out RaycastHit hit))
                {
                 targetGrounded = hit.transform.CompareTag("Water");
                }
            agent.speed = velocity;
            } while (Vector3.Distance(targetPos, initialPosition) > maxLenghtToMove && targetGrounded);

        transform.LookAt(targetPos);
        float elapsedTime = 0;
        agent.isStopped = false;
        while (Vector3.Distance(transform.position,targetPos)>distanceWithTarget && elapsedTime <= maxTimeToTarget)
            {
            //mov = MoveTowardsTarget(targetPos)*velocity;
            agent.destination = targetPos;
            elapsedTime += Time.deltaTime;
            yield return null;
            }
        //mov = Vector3.zero;
        agent.isStopped = true;
        elapsedTime = 0;
        while(elapsedTime < TimeBetweenJumps)
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
        Vector3 randomPosition = origin + (randomDirection*moveForward);
        randomPosition.y = transform.position.y;
        //print(randomPosition);
        
        return randomPosition;
        }
    }
