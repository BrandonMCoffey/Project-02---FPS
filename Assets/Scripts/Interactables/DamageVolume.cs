using System.Collections;
using Assets.Scripts.Player;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Interactables {
    public class DamageVolume : MonoBehaviour {
        [SerializeField] private FloatReference _damage = new FloatReference(1);
        [SerializeField] private float _delay = 1;

        private bool _canDamage = true;

        private void OnTriggerStay(Collider other)
        {
            if (!_canDamage) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) {
                Debug.Log("Warning: Invalid Object on Player layer detected in Damage Volume: " + other.gameObject);
                return;
            } else {
                playerHealth.Damage(_damage);
            }

            StartCoroutine(DelayNextDamage());
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            StopCoroutine(DelayNextDamage());
            _canDamage = true;
        }

        private IEnumerator DelayNextDamage()
        {
            _canDamage = false;
            for (float t = 0; t <= _delay; t += Time.deltaTime)
            {
                yield return null;
            }
            _canDamage = true;
        }
    }
}