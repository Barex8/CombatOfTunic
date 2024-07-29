using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damage;
    public bool doDamage;

    private void OnTriggerStay(Collider collider)
        {
        if (doDamage)
            {
            Damageable damageable = null;
            if (collider.transform.GetComponent<Damageable>()) damageable = collider.GetComponent<Damageable>();
            if (damageable != null && damageable.hitable && !collider.CompareTag(transform.tag))
                {
                print("Dañando: " + collider.name);
                collider.transform.GetComponent<Damageable>().GetDamage(damage);
                damageable.GetHit(transform.position);
                }
            }
        }
    }
