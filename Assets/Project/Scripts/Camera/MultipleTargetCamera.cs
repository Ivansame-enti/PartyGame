using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MultipleTargetCamera : MonoBehaviour
{
    //[HideInInspector]
    public List<Transform> targets;

    [SerializeField] private Vector3 offset;
	[SerializeField] private float smoothTime = 0.5f;
	[SerializeField] private float minZoom = 40f;
	[SerializeField] private float maxZoom = 10f;
	[SerializeField] private float zoomLimiter = 50f;

    private Camera cam;
	private Vector3 velocity;

	private void Start()
    {
        targets.Clear();
        cam = GetComponent<Camera>();
        GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playersArray)
        {
            targets.Add(player.transform);
        }
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
        {
            return;
        }
        Move();
        Zoom();
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    private float GetGreatestDistance()
    {
        if (targets.Count == 0)
        {
            return 0;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }
        return bounds.size.x;
    }

    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 0)
        {
            return Vector3.zero;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }

        return bounds.center;
    }
}