using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Player : MonoBehaviour
{
    public Transform[] objetivos; // Array con los 7 objetivos
    private NavMeshAgent agente;
    private float tiempo = 0;
    private float tiempoEspera = 14.0f; // Tiempo para cambiar de objetivo

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        ElegirNuevoDestino();
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        if (tiempo > tiempoEspera)
        {
            ElegirNuevoDestino();
            tiempo = 0;
        }
    }

    void ElegirNuevoDestino()
    {
        if (objetivos.Length == 0) return; // Evitar errores si no hay objetivos

        int indiceAleatorio = Random.Range(0, objetivos.Length);
        agente.destination = objetivos[indiceAleatorio].position;
    }
}

