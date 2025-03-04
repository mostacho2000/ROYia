using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class wolf : MonoBehaviour
{
    public enum EstadoLobo { Idle, Cazando, Mordiendo, Muerte }
    private EstadoLobo estadoActual;

    private NavMeshAgent agente;
    public float radioDeteccion = 500f;
    public float radioDeteccion2 = 25f;
    public float comida = 100f;
    public float resistencia = 100f;

    private Transform objetivo;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        comida = 100f; // Asegurar que inicia con 100 de comida
        CambiarEstado(EstadoLobo.Idle);
    }

    void Update()
    {
        if (comida < 5)
        {
            CambiarEstado(EstadoLobo.Muerte);
           
        }

        EjecutarEstado();
    }

    void DetectarVacas()
    {
        Collider[] vacas = Physics.OverlapSphere(transform.position, radioDeteccion);
        foreach (var vaca in vacas)
        {
            if (vaca.CompareTag("Cow"))
            {
                objetivo = vaca.transform;
                break;
            }
        }
    }

    void CambiarEstado(EstadoLobo nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        estadoActual = nuevoEstado;
        Debug.Log("Lobo cambia a estado: " + estadoActual);
    }

    void EjecutarEstado()
    {
        switch (estadoActual)
        {
            case EstadoLobo.Idle:
                Idle();
                break;
            case EstadoLobo.Cazando:
                Cazando();
                break;
            case EstadoLobo.Mordiendo:
                Mordiendo();
                break;
            case EstadoLobo.Muerte:
                Muerte();
                break;
        }
    }

    void Idle()
    {
        Debug.Log("Estoy en estado Idle");
        resistencia = Mathf.Min(resistencia + 1 * Time.deltaTime, 100);
        comida = Mathf.Max(comida - 2 * Time.deltaTime, 0);

        if (comida < 30)
        {
            DetectarVacas();
            if (objetivo != null)
            {
                CambiarEstado(EstadoLobo.Cazando);
            }
        }
    }

    void Cazando()
    {
        Debug.Log("Modo Negro");
        if (objetivo != null)
        {
            agente.SetDestination(objetivo.position);
            resistencia = Mathf.Max(resistencia - 2 * Time.deltaTime, 0);
            comida = Mathf.Min(comida - 1 * Time.deltaTime, 100);

            if (Vector3.Distance(transform.position, objetivo.position) < radioDeteccion2)
            {
                CambiarEstado(EstadoLobo.Mordiendo);
            }
        }
        else
        {
            Debug.Log("No hay vacas detectadas, volviendo a Idle.");
            CambiarEstado(EstadoLobo.Idle);
        }
    }

    void Mordiendo()
    {
        Debug.Log("Estado Degustando");
        comida = Mathf.Min(comida + 5 * Time.deltaTime, 100);
        resistencia = Mathf.Max(resistencia - 1 * Time.deltaTime, 0);

        if (comida > 90)
        {
            CambiarEstado(EstadoLobo.Idle);
        }
    }

    void Muerte()
    {
        Debug.Log("El lobo ha muerto de hambre.");
        Destroy(gameObject);
    }
}

