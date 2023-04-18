using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class newSphere : MonoBehaviour
{
    List<GameObject> spheres;
    public GameObject spherePrefab;
    public int count = 300;
    public float radius = 0.5f;
    private float movementSpeed = 5f;
    public Vector3 center;
    public float width = 20f;
    public float height = 20f;
    public float depth = 20f;


    void Start()
    {
        spheres = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(center.x - width / 2f, center.x + width / 2f),
                Random.Range(center.y - height / 2f, center.y + height / 2f),
                Random.Range(center.z - depth / 2f, center.z + depth / 2f)
            );
            GameObject sphere = Instantiate(spherePrefab, randomPosition, Quaternion.identity);

            sphere.transform.localScale = new Vector3(radius, radius, radius);

            MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
            renderer.material.SetColor("Color", Color.white);

            Rigidbody rigidbody = sphere.AddComponent<Rigidbody>();
            rigidbody.mass = 1f;

            spheres.Add(sphere);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            foreach(GameObject sphere in spheres)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-width / 2f, width / 2f) * movementSpeed * Time.deltaTime,
                    Random.Range(100f, 500f) * movementSpeed * Time.deltaTime,
                    Random.Range(-depth / 2f, depth / 2f) * movementSpeed * Time.deltaTime
                );
                sphere.transform.position += offset;
                sphere.GetComponent<Rigidbody>().AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            }
        }
    }
}
