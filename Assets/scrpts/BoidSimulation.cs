using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoidSimulationControl : MonoBehaviour
{
    public enum ControlMode
    {
        Seek,
        Pursue,
        Food,
        Obstacle
    }
    public static BoidSimulationControl Instance;
    public ControlMode controlMode = ControlMode.Seek;

    public GameObject boidPrefab = null;
    public GameObject foodPrefab;
    public GameObject obstaclePrefab;

    public int numBoidToSpawn = 4;
    public List<Boid> boids = new List<Boid>();

    private Camera mainCamera;

    public LayerMask groundLayer;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        for (int i = 0; i < numBoidToSpawn; i++)
        {
            mainCamera = Camera.main;
            Vector3 position = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0, 1.4f), Random.Range(-0.9f, 0.9f));
            Quaternion rotation = Random.rotation;
            GameObject spawnedBoid = Instantiate(boidPrefab, position, rotation);
            boids.Add(spawnedBoid.GetComponent<Boid>());
            spawnedBoid.transform.localScale *= Random.Range(0.9f, 3f);

        }
    }

    private void Update()
    {    // 1. 持续更新鼠标位置
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            MouseTarget.Position = hit.point;
        }

        // 2. 处理鼠标点击（放置物体）
        if (Input.GetMouseButtonDown(0)) HandleMouseClick(true);   // 左键：放食物
        if (Input.GetMouseButtonDown(1)) HandleMouseClick(false);  // 右键：放障碍

        // 3. 显示调试信息
        Debug.Log("Mouse World Position: " + MouseTarget.Position);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            controlMode = ControlMode.Seek;
            Debug.Log("Switched to Seek");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            controlMode = ControlMode.Pursue;
            Debug.Log("Switched to Pursue");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            controlMode = ControlMode.Food;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            controlMode = ControlMode.Obstacle;
        }
       
    }



    private void HandleMouseClick(bool leftClick)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPos = hit.point;

            // Save global mouse target
            //MouseTarget.Position = targetPos;//
            MouseTarget.LeftClick = leftClick;
          

            // Based on mode, place food or obstacle
            if (controlMode == ControlMode.Food && leftClick)
             {
                 GameObject food = Instantiate(foodPrefab, targetPos, Quaternion.identity);
                 food.tag = "Food";
             }
             else if (controlMode == ControlMode.Obstacle && leftClick)
             {
                 GameObject obstacle = Instantiate(obstaclePrefab, targetPos, Quaternion.identity);
                 obstacle.tag = "Obstacle";
             }
        }
    }
}


