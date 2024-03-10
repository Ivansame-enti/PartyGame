using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollector : MonoBehaviour
{
    ParticleSystem ps;
    List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        Debug.Log("Entro");
        // Obtener las part�culas que han sido detectadas
        int triggeredParticles = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        // Recorrer todas las part�culas detectadas
        for (int i = 0; i < triggeredParticles; i++)
        {
            Debug.Log("Entro FOr");
            ParticleSystem.Particle p = particles[i];
            // Establecer la vida restante de la part�cula a cero para destruirla
            p.remainingLifetime = 0f;
            particles[i] = p;
        }

        // Actualizar la lista de part�culas en el sistema de part�culas
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
    }
}