using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radioExplosion = 15f;
    public float fuerzaExplosion = 1000f;
    public GameObject efectoExplosion;

    [Header("Tiempo de Explosi�n")]
    public float minTiempoExplosion = 2f;
    public float maxTiempoExplosion = 10f;

    private bool haExplotado = false;

    void Start()
    {
        // Configurar tiempo aleatorio de explosi�n
        float tiempoAleatorio = Random.Range(minTiempoExplosion, maxTiempoExplosion);
        Invoke("Detonar", tiempoAleatorio);
    }

    public void Detonar()
    {
        if (haExplotado) return;
        haExplotado = true;

        // Mostrar efecto visual
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);
        }

        // Aplicar f�sica de explosi�n
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioExplosion);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(fuerzaExplosion, transform.position, radioExplosion);
            }

            // Activar p�nico en agentes dentro del radio
            Player agente = hit.GetComponent<Player>();
            if (agente != null)
            {
                agente.ActivarPanico(transform.position);
            }
        }

        // Destruir el objeto de explosi�n despu�s de detonar
        Destroy(gameObject);
    }

    // Dibujar gizmo para ver el radio en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}
