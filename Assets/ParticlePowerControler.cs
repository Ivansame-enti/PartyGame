using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePowerController : MonoBehaviour
{
    public ParticleSystem PowerParticleSystem;
    public float moveSpeed = 5f;
    public float attractionRadius = 2.0f; // Radio de atracci�n
    public float particleAttractionStrength = 1.0f; // Fuerza de atracci�n
    public float gatherRadius = 0.5f; // Radio de agrupaci�n
    public float destroyDistanceThreshold = 1.0f; // Umbral de distancia para destruir las part�culas

    private GameObject attractor; // Objeto atractor

    private void Start()
    {
        // Crear el objeto atractor
        attractor = new GameObject("Attractor");
        attractor.transform.position = transform.position; // Coloca el atractor en la posici�n inicial del controlador

        // Invocar la funci�n para configurar el destino despu�s de 2 segundos
        StartCoroutine(MoveParticlesTowardsNearestPlayer(1.2f));
    }

    IEnumerator MoveParticlesTowardsNearestPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Obtener al jugador m�s cercano
        GameObject closestPlayer = FindClosestPlayer(transform.position);

        // Mover las part�culas hacia el atractor primero
        while (PowerParticleSystem != null)
        {
            // Calcular el vector de direcci�n hacia el atractor
            Vector3 directionToAttractor = attractor.transform.position - PowerParticleSystem.transform.position;

            // Mover las part�culas hacia el atractor
            if (directionToAttractor.magnitude > gatherRadius)
            {
                PowerParticleSystem.transform.position += directionToAttractor.normalized * Time.deltaTime * moveSpeed;
            }

            // Mover las part�culas hacia el jugador m�s cercano si existe
            if (closestPlayer != null)
            {
                // Calcular el vector de direcci�n hacia el jugador
                Vector3 directionToPlayer = closestPlayer.transform.position - PowerParticleSystem.transform.position;

                // Mover las part�culas hacia el jugador
                PowerParticleSystem.transform.position += directionToPlayer.normalized * Time.deltaTime * moveSpeed;

                // Aplicar una fuerza de atracci�n entre las part�culas para mantenerlas juntas
                ApplyParticleAttraction(closestPlayer);

                // Verificar la distancia entre las part�culas y el jugador para destruirlas si est�n lo suficientemente cerca
                float distanceToPlayer = Vector3.Distance(PowerParticleSystem.transform.position, closestPlayer.transform.position);
                if (distanceToPlayer < destroyDistanceThreshold)
                {
                    Destroy(PowerParticleSystem.gameObject);
                    yield break;
                }
            }

            yield return null;
        }
    }

    void ApplyParticleAttraction(GameObject closestPlayer)
    {
        Vector3 totalAttractionForce = Vector3.zero;
        int numParticles = 0;

        Collider[] colliders = Physics.OverlapSphere(PowerParticleSystem.transform.position, attractionRadius); // Considera solo part�culas cercanas
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != PowerParticleSystem.gameObject)
            {
                Vector3 directionToParticle = collider.gameObject.transform.position - PowerParticleSystem.transform.position;

                // Sumar las posiciones de las part�culas cercanas
                totalAttractionForce += directionToParticle;
                numParticles++;
            }
        }

        // Calcular el punto central promedio
        if (numParticles > 0)
        {
            Vector3 averageDirection = totalAttractionForce / numParticles;
            totalAttractionForce = averageDirection.normalized * particleAttractionStrength;
        }

        // Aplicar una fuerza de atracci�n adicional hacia el jugador para juntar las part�culas
        if (closestPlayer != null)
        {
            Vector3 directionToPlayer = closestPlayer.transform.position - PowerParticleSystem.transform.position;
            totalAttractionForce += directionToPlayer.normalized * particleAttractionStrength;
        }

        PowerParticleSystem.transform.position += totalAttractionForce.normalized * Time.deltaTime * moveSpeed;
    }

    GameObject FindClosestPlayer(Vector3 position)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
}