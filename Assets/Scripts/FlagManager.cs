using System;
using DigitalDouble.Scripts;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;
using Ray = UnityEngine.Ray;
using RaycastHit = Unity.Physics.RaycastHit;

public class FlagManager : MonoBehaviour
{
    public UnityEvent OnInitialized;

    public event TotalCount OnTotalUpdated;
    public delegate void TotalCount(int total);
    
    public static FlagManager Instance { get; private set; }
    
    public int Total
    {
        get => flagGrid.Length;
    }
    
    public int SizeX
    {
        get => x;
        set
        {
            DestroyFlag();
            InstantiateFlag(value, y, spacing);
        }
    }
    
    public int SizeY
    {
        get => y;
        set
        {
            DestroyFlag();
            InstantiateFlag(x, value, spacing);
        }
    }
    
    public float Spacing
    {
        get => spacing;
        set
        {
            DestroyFlag();
            InstantiateFlag(x, y, value);
        }
    }
    
    public EntityManager entityManager { get; private set; }

    [SerializeField]
    private float raycastDistance = 10000f;
    
    [SerializeField]
    private CameraMovement cameraMovement;    
    
    private Entity cubeEntity;
    private World world;
    private PhysicsWorld physicsWorld;
    private BlobAssetStore blobAssetStore;
    
    [SerializeField]
    private GameObject cubePrefab;

    private int x = 100;
    private int y = 100;
    private float spacing = 1;
    private Entity[,] flagGrid;

    public void CheckHit(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.started) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        Debug.Log(cameraMovement.PointerPos);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(cameraMovement.PointerPos.x, cameraMovement.PointerPos.y,
            0));
        
        RaycastInput raycastInput = new RaycastInput
        {
            Start =  ray.origin,
            End = ray.GetPoint(raycastDistance),
            Filter = CollisionFilter.Default
        };

        if (!physicsWorld.CastRay(raycastInput, out RaycastHit hit)) return;

        var selectedEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
        var renderMesh = entityManager.GetSharedComponentData<RenderMesh>(selectedEntity);
        var mat = new UnityEngine.Material(renderMesh.material);
        mat.SetColor("_Color", UnityEngine.Random.ColorHSV());
        renderMesh.material = mat;

        entityManager.DestroyEntity(selectedEntity);
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Допускается только 1 инстанс данного класса!");
        }
        else
        {
            Instance = this;
        }
        
        world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;
        physicsWorld = world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        blobAssetStore = new BlobAssetStore();
        
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(world, blobAssetStore);
        cubeEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(cubePrefab, settings);
        
        InstantiateFlag(x, y, spacing);
        OnInitialized?.Invoke();
    }

    private Entity InstantiateCube(float3 position)
    {
        Entity entity = entityManager.Instantiate(cubeEntity);
        entityManager.SetComponentData(entity, new Translation()
        {
            Value = position
        });
        entityManager.AddComponentData(entity, new MaterialColor()
        {
            Value = new Random(1).NextFloat4()
        });
        return entity;
    }

    private void DestroyFlag()
    {
        if (flagGrid == null) return;
        
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                entityManager.DestroyEntity(flagGrid[i, j]);
            }
        }
    }

    private void InstantiateFlag(int sizeX, int sizeY, float spacing)
    {
        x = sizeX;
        y = sizeY;
        flagGrid = new Entity[x,y];
        
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Entity newCube = InstantiateCube(new float3(0f, j * spacing, i * spacing));
                flagGrid[i, j] = newCube;
            }
        }
        
        OnTotalUpdated?.Invoke(Total);
    }
}
