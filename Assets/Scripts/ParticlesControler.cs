using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesControler : MonoBehaviour
{
    public GameObject sistemaParticulasPrefab; // Asigna el prefab de las part�culas desde el Inspector
    public string tagEnemigo = "Enemy"; // Etiqueta de los enemigos
    public string tagJugador = "Player";
    public float delay = 1f;
    private List<Vector3> posicionesEnemigos = new List<Vector3>();

    // M�todo para agregar la posici�n de un enemigo a la lista
    public void AgregarPosicionEnemigo(Vector3 posicionEnemigo)
    {
        posicionesEnemigos.Add(posicionEnemigo);
    }

    // M�todo para instanciar las part�culas en la �ltima posici�n registrada
    public void InstanciarParticulas()
    {
        if (posicionesEnemigos.Count > 0)
        {
            Vector3 ultimaPosicion = posicionesEnemigos[posicionesEnemigos.Count - 1];
            Instantiate(sistemaParticulasPrefab,new Vector3(ultimaPosicion.x, ultimaPosicion.y+5, ultimaPosicion.z) , Quaternion.identity);

            ParticleSystem ps = sistemaParticulasPrefab.GetComponent<ParticleSystem>();
            GameObject player = GameObject.FindGameObjectWithTag(tagJugador);

            // Iniciar las part�culas despu�s de un tiempo
            StartCoroutine(MoveParticles(ps, player.transform, delay));
        }
    }
    private IEnumerator MoveParticles(ParticleSystem ps, Transform player, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        // Mover las part�culas hacia el jugador
        if (player != null)
        {
            ps.Simulate(0f, true, true);
            ps.Play();


            Vector3 direccion = (player.position - ps.transform.position).normalized;
            ps.GetComponent<Rigidbody>().velocity = direccion * ps.main.startSpeed.constant;
        }
    }
    // Llamado para buscar y registrar las posiciones de los enemigos al iniciar
    void Start()
    {
        BuscarEnemigos();
    }

    // M�todo para buscar enemigos por etiqueta y registrar sus posiciones
    void BuscarEnemigos()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

        foreach (GameObject enemigo in enemigos)
        {
            AgregarPosicionEnemigo(enemigo.transform.position);
        }
    }
}