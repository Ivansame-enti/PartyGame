using UnityEngine;

public class ImpactPosition : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 direction;

    private void Start()
    {
        // Guarda la posici�n inicial del meteorito
        initialPosition = transform.position;

        // Define la direcci�n del meteorito (puede ser una direcci�n aleatoria o predefinida)
        direction = new Vector3(0, -25, 12).normalized; // Por ejemplo, diagonal hacia abajo y a la derecha
    }

    private void Update()
    {
        // Mueve el meteorito en la direcci�n definida
        transform.position += direction * Time.deltaTime * 10;

        // Lanza un raycast desde la posici�n inicial del meteorito en la direcci�n de movimiento
        RaycastHit hit;
        if (Physics.Raycast(initialPosition, direction, out hit))
        {
            // Verifica si el raycast ha golpeado el suelo
            if (hit.collider.CompareTag("Ground"))
            {
                // Acci�n a realizar cuando el meteorito impacta en el suelo
                Debug.Log("El meteorito ha impactado en el suelo en la posici�n: " + hit.point);

                // Puedes realizar cualquier acci�n adicional aqu�, como generar un efecto visual en el punto de impacto.
            }
        }
    }
}