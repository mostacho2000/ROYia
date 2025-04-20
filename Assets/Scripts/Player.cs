using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Player : MonoBehaviour
{
    public Transform[] objetivos;
    private NavMeshAgent agente;
    private float tiempo = 0;
    private float tiempoEspera = 14.0f;

    // Estados del agente
    private enum Estado { Normal, Panico, PanicoContagio }
    private Estado estadoActual = Estado.Normal;

    // Variables de pánico
    private Vector3 posicionExplosion;
    public float radioContagio = 5f;
    public float frecuenciaChequeoContagio = 1f;
    private float ultimoChequeoContagio = 0f;

    // Velocidades según estado
    private float velocidadNormal;
    public float velocidadPanico = 8f;

    // Colores por estado
    public Color colorNormal = Color.green;
    public Color colorPanico = Color.red;
    public Color colorPanicoContagio = new Color(1f, 0.5f, 0f);

    // Variables para la huida
    public float tiempoReevaluacionHuida = 2f;
    public float distanciaMinimaHuida = 5f;
    public float distanciaMaximaHuida = 25f;
    public float anguloAleatoriedadHuida = 45f;

    private Renderer rend;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        velocidadNormal = agente.speed;
        rend = GetComponent<Renderer>();
        ActualizarColor();
        ElegirNuevoDestino();
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.Normal:
                tiempo += Time.deltaTime;
                if (tiempo > tiempoEspera)
                {
                    ElegirNuevoDestino();
                    tiempo = 0;
                }
                break;

            case Estado.Panico:
            case Estado.PanicoContagio:
                // Chequear contagio periódicamente
                ultimoChequeoContagio += Time.deltaTime;
                if (ultimoChequeoContagio >= frecuenciaChequeoContagio)
                {
                    ChequearContagio();
                    ultimoChequeoContagio = 0f;
                }

                // Actualizar huida continuamente
                tiempo += Time.deltaTime;
                if (tiempo > tiempoReevaluacionHuida || agente.remainingDistance < 2f)
                {
                    HuirDeExplosion();
                    tiempo = 0f;
                }
                break;
        }
    }

    // Método para resetear el estado
    public void ResetearEstado()
    {
        estadoActual = Estado.Normal;
        agente.speed = velocidadNormal;
        ActualizarColor();
        ElegirNuevoDestino();
    }

    // Método para actualizar el color según el estado
    private void ActualizarColor()
    {
        if (rend == null) return;

        switch (estadoActual)
        {
            case Estado.Normal:
                rend.material.color = colorNormal;
                break;
            case Estado.Panico:
                rend.material.color = colorPanico;
                break;
            case Estado.PanicoContagio:
                rend.material.color = colorPanicoContagio;
                break;
        }
    }

    // Método para activar pánico por contagio
    public void ActivarPanicoContagio(Vector3 posicionContagio)
    {
        if (estadoActual != Estado.Normal) return;

        estadoActual = Estado.PanicoContagio;
        posicionExplosion = posicionContagio;
        agente.speed = velocidadPanico;
        ActualizarColor();
        HuirDeExplosion();
    }

    // Método para activar pánico por explosión
    public void ActivarPanico(Vector3 explosionPos)
    {
        if (estadoActual != Estado.Normal) return;

        estadoActual = Estado.Panico;
        posicionExplosion = explosionPos;
        agente.speed = velocidadPanico;
        ActualizarColor();
        HuirDeExplosion();
    }

    private void HuirDeExplosion()
    {
        // Calcular dirección opuesta a la explosión con aleatoriedad
        Vector3 direccionHuir = (transform.position - posicionExplosion).normalized;
        direccionHuir = Quaternion.Euler(0, Random.Range(-anguloAleatoriedadHuida, anguloAleatoriedadHuida), 0) * direccionHuir;

        // Intentar varias distancias de huida
        float[] distancias = { distanciaMaximaHuida, distanciaMaximaHuida * 0.75f, distanciaMinimaHuida };

        foreach (float distancia in distancias)
        {
            Vector3 destinoHuir = transform.position + direccionHuir * distancia;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(destinoHuir, out hit, distancia, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                return;
            }
        }

        // Si no encuentra punto válido, quedarse quieto
        agente.ResetPath();
    }

    private void ElegirNuevoDestino()
    {
        if (objetivos.Length == 0) return;

        int indiceAleatorio = Random.Range(0, objetivos.Length);
        agente.destination = objetivos[indiceAleatorio].position;
    }

    void ChequearContagio()
    {
        // Solo contagiar si estamos en algún estado de pánico
        if (estadoActual == Estado.Normal) return;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radioContagio, Vector3.up, 0f);

        foreach (RaycastHit hit in hits)
        {
            Player otroAgente = hit.collider.GetComponent<Player>();
            if (otroAgente != null && otroAgente != this && otroAgente.estadoActual == Estado.Normal)
            {
                otroAgente.ActivarPanicoContagio(transform.position);
            }
        }
    }

    // Visualización del radio de contagio en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioContagio);
    }
}

