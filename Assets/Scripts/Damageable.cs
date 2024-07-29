using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("General")]
    public int health;
    public bool hitable = true;
    [SerializeField] private float WaitToDeath;

    [Header("Invencibility")]
    [SerializeField]bool invincibilityWhenHit;
    [SerializeField]float invincibleTime;
    [HideInInspector] public bool hitRecived = false;

    [Header("PushBack")]
    [SerializeField] bool pushBack;
    [SerializeField]public float pushBackTime;
    [SerializeField] public float pushBackForce;
    [SerializeField] public float heightRayCheckGroundForward;
    [HideInInspector]public Rigidbody rb;
    public Color colorHit;
    [HideInInspector]public Color normalColor;

    [SerializeField] MeshRenderer mR;
    [SerializeField] public SkinnedMeshRenderer sMR;  

    private void Start()
        {
        hitable = true;
        if(mR!=null)normalColor = mR.material.color;
        if (sMR != null) normalColor = sMR.material.color;

        try { rb = GetComponent<Rigidbody>(); }
        catch (System.Exception){throw;}
        }
    public virtual void GetDamage(int damage)
        {
        
        if (health > damage)
            {
            health -= damage;
            StartCoroutine(Hit());
            }
        else
            {
            StartCoroutine(Hit());
            StartCoroutine(Death());}
        }

    public virtual IEnumerator Hit()
        {
        hitRecived = true;
        if (invincibilityWhenHit)
            {
            hitable = false;
            float elipsedTime = 0;
            float lastPeriod = 0;
            
            try
                {
                transform.GetChild(0).GetComponent<Animator>().SetTrigger("GetHit");
                }
            catch{}

            while (elipsedTime < invincibleTime)
                {
                elipsedTime += Time.deltaTime;
                if (elipsedTime > lastPeriod)
                    {

                    if (mR != null)
                        {
                        if (mR.material.color != normalColor) mR.material.color = normalColor;
                        else mR.material.color = colorHit;
                        }
                    else if (sMR != null)
                        {
                         if (sMR.material.color != normalColor) sMR.material.color = normalColor;
                         else sMR.material.color = colorHit;
                        }                    
                    lastPeriod += .1f;
                    }
                yield return null;
                
                }
            if (mR != null) mR.material.color = normalColor;
            else if (sMR != null) sMR.material.color = normalColor;

            hitable = true;
            }
        yield return null;
        }
    public virtual IEnumerator Death()
        {
        yield return new WaitForSeconds(WaitToDeath);
         Destroy(gameObject);
         yield return null;
        }
    public void GetHit(Vector3 pos)
        {
        StartCoroutine(PushBack(pos));
        }
    public virtual IEnumerator PushBack(Vector3 posHit)
        {
        if (pushBack)
            {
            transform.LookAt(posHit);
            float elapsedTime = 0;
            while (elapsedTime < pushBackTime)
                {
                if (CheckGroundBackward())
                    {
                    if (rb != null) rb.velocity = (transform.position - posHit) * pushBackForce;
                    }
                else
                    {
                    rb.velocity = Vector3.zero;
                    }
                elapsedTime += Time.deltaTime;
                yield return null;
                }
            }
        }
   public bool CheckGroundBackward()
        {
        Vector3 originRayGroundForward = new Vector3(transform.position.x, transform.position.y + heightRayCheckGroundForward, transform.position.z);
        Vector3 directionRayGroundForward = new Vector3(-transform.forward.x, -transform.up.y, -transform.forward.z);
        Debug.DrawRay(originRayGroundForward, directionRayGroundForward, Color.blue);
        if (Physics.Raycast(originRayGroundForward, directionRayGroundForward, out RaycastHit hit))
            {
            if (hit.transform.CompareTag("Water"))
                {
                //rb.velocity = Vector3.zero;
                return false;
                }
            }
        return true;
        }

    }
