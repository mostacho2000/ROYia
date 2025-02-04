using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Player : MonoBehaviour
{
    public Transform objetivo1;
    public Transform objetivo2;
    NavMeshAgent agente;
    
    bool CambiaObjetivo = false;
    float tiempo = 0;
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.destination = objetivo1.position;
    }
    void Update()
    {
        if (tiempo>14.0f) {

            if (CambiaObjetivo)
            {
                agente.destination = objetivo2.position;
                CambiaObjetivo = false;
            }
            else
            {
                agente.destination = objetivo1.position;
                CambiaObjetivo = true;
            }
            tiempo = 0;
        }
        tiempo += Time.deltaTime;
    }
}
