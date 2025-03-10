using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Cow : MonoBehaviour
{
    [Header("Par�metros")]
    public float comida = 50f;
    public float lactancia = 50f;
    public float estres = 50f;
    public float resistencia = 50f;

    [Header("Puntos de Inter�s")]
    public Transform pastizal;
    public Transform establo;
    public Transform fabrica;

    [Header("Detecci�n de Lobos")]
    public float wolfDetectionRadius = 20f;

    [Header("Textos de Estado")]
    public TextMeshProUGUI textoIdle;
    public TextMeshProUGUI textoPastar;
    public TextMeshProUGUI textoJugar;
    public TextMeshProUGUI textoEscapar;
    public TextMeshProUGUI textoOrde�ar;
    public TextMeshProUGUI textoDescanso;

    private NavMeshAgent agent;
    private float defaultSpeed;

    public enum State { Idle, Pastar, Jugar, Escapar, Ordenar, Descanso, Estallar }
    private State currentState = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = agent.speed;
        ActualizarTextoEstado(); // Se llama al inicio para establecer el texto correcto
    }

    void Update()
    {
        float delta = Time.deltaTime;

        if (estres > 90)
        {
            currentState = State.Estallar;
            Explode();
            return;
        }

        if (currentState == State.Escapar)
        {
            if (!DetectWolf() && HasReachedDestination())
            {
                Debug.Log("La vaca ha llegado al establo y ahora descansa.");
                CambiarEstado(State.Descanso);
            }
        }

        switch (currentState)
        {
            case State.Idle:
                Debug.Log("La vaca est� en estado idle (reposo). ");
                UpdateIdle(delta);
                break;
            case State.Pastar:
                Debug.Log("La vaca est� pastando.");
                SetDestination(pastizal.position);
                UpdatePastar(delta);
                break;
            case State.Jugar:
                Debug.Log("La vaca est� jugando.");
                SetDestination(pastizal.position);
                UpdateJugar(delta);
                break;
            case State.Escapar:
                Debug.Log("�La vaca est� escapando!");
                SetDestination(establo.position);
                UpdateEscapar(delta);
                break;
            case State.Ordenar:
                Debug.Log("La vaca est� siendo orde�ada.");
                SetDestination(fabrica.position);
                UpdateOrdenar(delta);
                break;
            case State.Descanso:
                Debug.Log("La vaca est� descansando.");
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
                Debug.Log("�Un lobo ha sido detectado! La vaca est� escapando.");
                CambiarEstado(State.Escapar);
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
                if (comida < 30) CambiarEstado(State.Pastar);
                else if (estres > 70) CambiarEstado(State.Jugar);
                else if (lactancia > 80) CambiarEstado(State.Ordenar);
                break;
            case State.Pastar:
                if (comida > 95) CambiarEstado(State.Idle);
                else if (lactancia > 80) CambiarEstado(State.Ordenar);
                break;
            case State.Jugar:
                if (comida < 40) CambiarEstado(State.Pastar);
                else if (estres < 21) CambiarEstado(State.Idle);
                else if (resistencia < 30) CambiarEstado(State.Descanso);
                break;
            case State.Ordenar:
                if (lactancia < 30) CambiarEstado(State.Idle);
                else if (comida < 40) CambiarEstado(State.Pastar);
                break;
            case State.Descanso:
                if (resistencia > 85) CambiarEstado(State.Idle);
                else if (comida < 30) CambiarEstado(State.Pastar);
                else if (lactancia > 80) CambiarEstado(State.Ordenar);
                else if (estres > 60) CambiarEstado(State.Jugar);
                break;
        }
    }

    void CambiarEstado(State nuevoEstado)
    {
        currentState = nuevoEstado;
        Debug.Log("Nuevo estado de la vaca: " + currentState);
        ActualizarTextoEstado();
    }

    void ActualizarTextoEstado()
    {
        textoIdle.gameObject.SetActive(currentState == State.Idle);
        textoPastar.gameObject.SetActive(currentState == State.Pastar);
        textoJugar.gameObject.SetActive(currentState == State.Jugar);
        textoEscapar.gameObject.SetActive(currentState == State.Escapar);
        textoOrde�ar.gameObject.SetActive(currentState == State.Ordenar);
        textoDescanso.gameObject.SetActive(currentState == State.Descanso);
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

    bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
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
        Debug.Log("�La vaca ha estallado por estr�s extremo!");
        Destroy(gameObject);
    }
}

