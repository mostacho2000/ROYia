using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public float wolfDetectionRadius = 20f;

    [Header("Textos de Estado")]
    public TextMeshProUGUI textoIdle;
    public TextMeshProUGUI textoPastar;
    public TextMeshProUGUI textoJugar;
    public TextMeshProUGUI textoEscapar;
    public TextMeshProUGUI textoOrdear;
    public TextMeshProUGUI textoDescanso;

    private NavMeshAgent agent;
    private float defaultSpeed;
    private bool dentroDePastizal = false;
    private bool dentroDeOrdeña = false;
    private float lastStateChangeTime; // Tiempo desde el último cambio de estado
    private float stateChangeDelay = 2f; // Retraso mínimo entre cambios de estado

    public enum State { Idle, Pastar, Jugar, Escapar, Ordenar, Descanso, Estallar }
    private State currentState = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = agent.speed;
        ActualizarTextoEstado();
        lastStateChangeTime = Time.time; // Inicializar el tiempo del último cambio de estado
    }

    void Update()
    {
        float delta = Time.deltaTime;

        if (comida < 5)
        {
            CambiarEstado(State.Estallar);
            Explode();
            return;
        }

        if (estres > 90)
        {
            CambiarEstado(State.Estallar);
            Explode();
            return;
        }

        if (currentState == State.Escapar)
        {
            if (!DetectWolf() && HasReachedDestination())
            {
                CambiarEstado(State.Descanso);
            }
        }

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle(delta);
                break;
            case State.Pastar:
                SetDestination(pastizal.position);
                UpdatePastar(delta);
                break;
            case State.Jugar:
                SetDestination(pastizal.position);
                UpdateJugar(delta);
                break;
            case State.Escapar:
                SetDestination(establo.position);
                UpdateEscapar(delta);
                break;
            case State.Ordenar:
                SetDestination(fabrica.position);
                UpdateOrdenar(delta);
                break;
            case State.Descanso:
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
        estres -= 0.3f * delta;
        UpdateLactancia(delta);

        if (dentroDePastizal)
        {
            comida += 7f * delta;
        }
        else
        {
            comida -= 2f * delta;
        }
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
        comida -= 2f * delta;

        // Lactancia solo baja dentro del objeto "Ordeña"
        if (dentroDeOrdeña)
        {
            lactancia -= 1f * delta;
        }
    }

    void UpdateDescanso(float delta)
    {
        resistencia += 7f * delta;
        estres -= 1f * delta;
        comida -= 1f * delta;
        UpdateLactancia(delta);
    }

    // Funciones de membresía para la lactancia
    float MembershipLow(float value)
    {
        if (value <= 40) return 1f;
        if (value >= 41) return 0f;
        return 1f - (value - 0) / (40 - 0);
    }

    float MembershipMedium(float value)
    {
        if (value <= 40 || value >= 85) return 0f;
        if (value <= 60) return (value - 40) / (60 - 40);
        return (85 - value) / (85 - 60);
    }

    float MembershipHigh(float value)
    {
        if (value <= 85) return 0f;
        if (value >= 100) return 1f;
        return (value - 85) / (100 - 85);
    }

    // Actualizar lactancia usando lógica difusa
    void UpdateLactancia(float delta)
    {
        // Grados de pertenencia para la comida y el estrés
        float comidaBaja = MembershipLow(comida);
        float comidaMedia = MembershipMedium(comida);
        float comidaAlta = MembershipHigh(comida);

        float estresBajo = MembershipLow(estres);
        float estresMedio = MembershipMedium(estres);
        float estresAlto = MembershipHigh(estres);

        // Aplicar reglas difusas para determinar el incremento/decremento de lactancia
        float incrementoLactancia = Mathf.Max(
            Mathf.Min(comidaAlta, estresBajo), // Si comida es alta y estrés es bajo, incrementar lactancia
            Mathf.Min(comidaMedia, estresMedio) // Si comida es media y estrés es moderado, mantener lactancia
        );

        float decrementoLactancia = Mathf.Min(comidaBaja, estresAlto); // Si comida es baja y estrés es alto, decrementar lactancia

        // Calcular el cambio neto de lactancia
        float cambioLactancia = (incrementoLactancia - decrementoLactancia) * delta;

        // Aplicar el cambio a la lactancia
        lactancia += cambioLactancia * 10; // Escalar el cambio para que sea significativo

        // Mostrar el estado de la lactancia en la consola
        if (lactancia <= 40)
        {
            Debug.Log("Lactancia: Baja");
        }
        else if (lactancia > 40 && lactancia <= 85)
        {
            Debug.Log("Lactancia: Media");
        }
        else if (lactancia > 85)
        {
            Debug.Log("Lactancia: Alta");
        }
    }

    void EvaluateTransitions()
    {
        // Evitar cambios de estado demasiado rápidos
        if (Time.time - lastStateChangeTime < stateChangeDelay)
        {
            return;
        }

        switch (currentState)
        {
            case State.Idle:
                if (comida < 30) CambiarEstado(State.Pastar);
                else if (estres > 70) CambiarEstado(State.Jugar);
                else if (lactancia > 80) CambiarEstado(State.Ordenar);
                break;
            case State.Pastar:
                if (comida > 95) CambiarEstado(State.Idle);
                else if (lactancia > 80 && comida > 50) CambiarEstado(State.Ordenar); // Añadir condición adicional
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
        ActualizarTextoEstado();
        lastStateChangeTime = Time.time; // Registrar el tiempo del último cambio de estado
    }

    void ActualizarTextoEstado()
    {
        textoIdle.gameObject.SetActive(currentState == State.Idle);
        textoPastar.gameObject.SetActive(currentState == State.Pastar);
        textoJugar.gameObject.SetActive(currentState == State.Jugar);
        textoEscapar.gameObject.SetActive(currentState == State.Escapar);
        textoOrdear.gameObject.SetActive(currentState == State.Ordenar);
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
        Debug.Log("¡La vaca ha estallado!");
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pastar"))
        {
            dentroDePastizal = true;
        }
        else if (other.CompareTag("Ordeña"))
        {
            dentroDeOrdeña = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pastar"))
        {
            dentroDePastizal = false;
        }
        else if (other.CompareTag("Ordeña"))
        {
            dentroDeOrdeña = false;
        }
    }
}