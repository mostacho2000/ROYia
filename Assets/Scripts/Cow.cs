using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cow : MonoBehaviour
{
    [Header("Parámetros")]
    public float comida = 50f;
    public float lactancia = 50f;
    public float estres = 50f;
    public float resistencia = 50f;

    [Header("Puntos de Interés")]
    public Transform pastizal;
    public Transform establo;
    public Transform fabrica;

    [Header("Detección de Lobos")]
    public float wolfDetectionRadius = 10f;

    private NavMeshAgent agent;
    private float defaultSpeed;

    private enum State { Idle, Pastar, Jugar, Escapar, Ordenar, Descanso, Estallar }
    private State currentState = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = agent.speed;
    }

    void Update()
    {
        float delta = Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                Debug.Log("La vaca está en estado idle (reposo). ");
                UpdateIdle(delta);
                break;
            case State.Pastar:
                Debug.Log("La vaca está pastando.");
                SetDestination(pastizal.position);
                UpdatePastar(delta);
                break;
            case State.Jugar:
                Debug.Log("La vaca está jugando.");
                SetDestination(pastizal.position);
                UpdateJugar(delta);
                break;
            case State.Escapar:
                Debug.Log("¡La vaca está escapando!");
                SetDestination(establo.position);
                UpdateEscapar(delta);
                break;
            case State.Ordenar:
                Debug.Log("La vaca está siendo ordeñada.");
                SetDestination(fabrica.position);
                UpdateOrdenar(delta);
                break;
            case State.Descanso:
                Debug.Log("La vaca está descansando.");
                SetDestination(establo.position);
                UpdateDescanso(delta);
                break;
            case State.Estallar:
                Explode();
                return;
        }

        ClampValues();

        if (currentState != State.Escapar && currentState != State.Estallar)
        {
            if (DetectWolf())
            {
                Debug.Log("¡Un lobo ha sido detectado! La vaca está escapando.");
                currentState = State.Escapar;
                SetDestination(establo.position);
            }
        }

        EvaluateTransitions();
    }

    void UpdateIdle(float delta)
    {
        comida -= 3f * delta;
        estres += 1f * delta;
        UpdateLactancia(delta);
    }

    void UpdatePastar(float delta)
    {
        comida += 7f * delta;
        estres -= 0.3f * delta;
        UpdateLactancia(delta);
    }

    void UpdateJugar(float delta)
    {
        estres -= 5f * delta;
        comida -= 3f * delta;
        resistencia -= 1f * delta;
        UpdateLactancia(delta);
    }

    void UpdateEscapar(float delta)
    {
        estres += 5f * delta;
        resistencia -= 5f * delta;
        comida -= 2f * delta;
    }

    void UpdateOrdenar(float delta)
    {
        lactancia -= 1f * delta;
        comida -= 2f * delta;
    }

    void UpdateDescanso(float delta)
    {
        resistencia += 7f * delta;
        estres -= 1f * delta;
        comida -= 1f * delta;
        UpdateLactancia(delta);
    }

    void UpdateLactancia(float delta)
    {
        if (comida > 77)
            lactancia += 3f * delta;
        else if (comida > 40 && comida <= 77)
            lactancia += 1f * delta;
    }

    void EvaluateTransitions()
    {
        switch (currentState)
        {
            case State.Idle:
                if (comida < 30) currentState = State.Pastar;
                else if (estres > 70) currentState = State.Jugar;
                else if (lactancia > 80) currentState = State.Ordenar;
                break;
            case State.Pastar:
                if (comida > 95) currentState = State.Idle;
                else if (lactancia > 80) currentState = State.Ordenar;
                break;
            case State.Jugar:
                if (comida < 40) currentState = State.Pastar;
                else if (estres < 21) currentState = State.Idle;
                else if (resistencia < 30) currentState = State.Descanso;
                break;
            case State.Escapar:
                if (estres > 90 || (estres > 60 && comida < 50)) currentState = State.Estallar;
                break;
            case State.Ordenar:
                if (lactancia < 30) currentState = State.Idle;
                else if (comida < 40) currentState = State.Pastar;
                break;
            case State.Descanso:
                if (resistencia > 85) currentState = State.Idle;
                else if (comida < 30) currentState = State.Pastar;
                else if (lactancia > 80) currentState = State.Ordenar;
                else if (estres > 60) currentState = State.Jugar;
                break;
        }
    }

    void SetDestination(Vector3 destination)
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
    }

    bool DetectWolf()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, wolfDetectionRadius);
        foreach (Collider col in hits)
        {
            if (col.CompareTag("Lobo"))
                return true;
        }
        return false;
    }

    void ClampValues()
    {
        comida = Mathf.Clamp(comida, 0, 100);
        lactancia = Mathf.Clamp(lactancia, 0, 100);
        estres = Mathf.Clamp(estres, 0, 100);
        resistencia = Mathf.Clamp(resistencia, 0, 100);
    }

    void Explode()
    {
        Destroy(gameObject);
    }
}

