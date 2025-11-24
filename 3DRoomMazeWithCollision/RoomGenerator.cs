namespace _3DRoomMazeWithCollision;


using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

/// Generates a 3x3 grid of rooms with walls, doors, and random obstacles
public class RoomGenerator
{
    private const float ROOM_SIZE = 3.0f;       // 3m x 3m rooms
    private const float ROOM_HEIGHT = 2.5f;     // 2.5m tall rooms
    private const float WALL_THICKNESS = 0.2f;  // 0.2m thick walls
    private const float DOOR_WIDTH = 1.0f;      // 1m wide doors
    private const float DOOR_HEIGHT = 2.0f;     // 2m tall doors

    private Random _random;
    private Shader _shader;

    public RoomGenerator(Shader shader, int seed = 42)
    {
        _shader = shader;
        _random = new Random(seed);
    }

    /// Generate the complete 3x3 room grid
    public void GenerateRooms(List<GameObject> sceneObjects, out GameObject goalObject)
    {
        // 1. Generate floors for all 9 rooms
        GenerateFloors(sceneObjects);

        // 2. Generate walls with doors
        GenerateWalls(sceneObjects);

        // 3. Generate random obstacles in each room
        GenerateObstacles(sceneObjects);

        // 4. Place goal object in a random room (not center room)
        goalObject = GenerateGoalObject(sceneObjects);
    }

    /// Generate floor tiles for 3x3 grid
    private void GenerateFloors(List<GameObject> sceneObjects)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 position = new Vector3(x * ROOM_SIZE, 0, z * ROOM_SIZE);
                Vector3 scale = new Vector3(ROOM_SIZE, 0.1f, ROOM_SIZE);

                var floor = new GameObject(position, Vector3.Zero, scale);
                floor.Tag = "Floor";
                floor.AddComponent(new Renderer(Mesh.CreateCube()));
                sceneObjects.Add(floor);
            }
        }
    }

    /// Generate walls with doors between rooms
    /// Walls on world edges have no doors
    private void GenerateWalls(List<GameObject> sceneObjects)
    {
        float halfRoom = ROOM_SIZE / 2.0f;
        float edgeOffset = 1.5f * ROOM_SIZE; // Distance to outer edge walls

        // === OUTER WALLS (World Boundaries) - NO DOORS ===
        
        // North wall (top)
        CreateWall(sceneObjects, 
            new Vector3(0, ROOM_HEIGHT / 2.0f, edgeOffset), 
            new Vector3(ROOM_SIZE * 3, ROOM_HEIGHT, WALL_THICKNESS), 
            "Wall");

        // South wall (bottom)
        CreateWall(sceneObjects, 
            new Vector3(0, ROOM_HEIGHT / 2.0f, -edgeOffset), 
            new Vector3(ROOM_SIZE * 3, ROOM_HEIGHT, WALL_THICKNESS), 
            "Wall");

        // East wall (right)
        CreateWall(sceneObjects, 
            new Vector3(edgeOffset, ROOM_HEIGHT / 2.0f, 0), 
            new Vector3(WALL_THICKNESS, ROOM_HEIGHT, ROOM_SIZE * 3), 
            "Wall");

        // West wall (left)
        CreateWall(sceneObjects, 
            new Vector3(-edgeOffset, ROOM_HEIGHT / 2.0f, 0), 
            new Vector3(WALL_THICKNESS, ROOM_HEIGHT, ROOM_SIZE * 3), 
            "Wall");

        // === INNER WALLS WITH DOORS ===
        
        // Vertical walls between rooms (running North-South, separating East-West)
        // towards East
        for (int z = -1; z <= 1; z++)
        {
            CreateWallWithDoor(sceneObjects, 
                new Vector3(-halfRoom, ROOM_HEIGHT / 2.0f, z * ROOM_SIZE), 
                true); // vertical
        }

        // towarsds West
        for (int z = -1; z <= 1; z++)
        {
            CreateWallWithDoor(sceneObjects, 
                new Vector3(halfRoom, ROOM_HEIGHT / 2.0f, z * ROOM_SIZE), 
                true); // vertical
        }

        // Horizontal walls between rooms (running East-West, separating North-South)
        // towards north
        for (int x = -1; x <= 1; x++)
        {
            CreateWallWithDoor(sceneObjects, 
                new Vector3(x * ROOM_SIZE, ROOM_HEIGHT / 2.0f, -halfRoom), 
                false); // horizontal
        }

        // towards south
        for (int x = -1; x <= 1; x++)
        {
            CreateWallWithDoor(sceneObjects, 
                new Vector3(x * ROOM_SIZE, ROOM_HEIGHT / 2.0f, halfRoom), 
                false); // horizontal
        }
    }

    /// Create a wall with a door in the center
    private void CreateWallWithDoor(List<GameObject> sceneObjects, Vector3 wallCenter, bool isVertical)
    {
        float wallLength = ROOM_SIZE;
        float sideLength = (wallLength - DOOR_WIDTH) / 2.0f;

        if (isVertical)
        {
            // Wall runs along Z-axis, door opens along X-axis
            // Left wall piece
            Vector3 leftPos = wallCenter + new Vector3(0, 0, -DOOR_WIDTH / 2.0f - sideLength / 2.0f);
            CreateWall(sceneObjects, leftPos, new Vector3(WALL_THICKNESS, ROOM_HEIGHT, sideLength), "Wall");

            // Right wall piece
            Vector3 rightPos = wallCenter + new Vector3(0, 0, DOOR_WIDTH / 2.0f + sideLength / 2.0f);
            CreateWall(sceneObjects, rightPos, new Vector3(WALL_THICKNESS, ROOM_HEIGHT, sideLength), "Wall");

            // Door (closed initially)
            Vector3 doorPos = wallCenter;
            CreateDoor(sceneObjects, doorPos, new Vector3(WALL_THICKNESS, DOOR_HEIGHT, DOOR_WIDTH), true);
        }
        else
        {
            // Wall runs along X-axis, door opens along Z-axis
            // Left wall piece
            Vector3 leftPos = wallCenter + new Vector3(-DOOR_WIDTH / 2.0f - sideLength / 2.0f, 0, 0);
            CreateWall(sceneObjects, leftPos, new Vector3(sideLength, ROOM_HEIGHT, WALL_THICKNESS), "Wall");

            // Right wall piece
            Vector3 rightPos = wallCenter + new Vector3(DOOR_WIDTH / 2.0f + sideLength / 2.0f, 0, 0);
            CreateWall(sceneObjects, rightPos, new Vector3(sideLength, ROOM_HEIGHT, WALL_THICKNESS), "Wall");

            // Door (closed initially)
            Vector3 doorPos = wallCenter;
            CreateDoor(sceneObjects, doorPos, new Vector3(DOOR_WIDTH, DOOR_HEIGHT, WALL_THICKNESS), false);
        }
    }

    /// Create a solid wall piece
    private void CreateWall(List<GameObject> sceneObjects, Vector3 position, Vector3 scale, string tag)
    {
        var wall = new GameObject(position, Vector3.Zero, scale);
        wall.Tag = tag;
        wall.AddComponent(new Renderer(Mesh.CreateCube()));
        wall.AddComponent(new AABBCollider(wall.Transform));
        sceneObjects.Add(wall);
    }

    /// Create a door (initially closed with collider enabled)
    private void CreateDoor(List<GameObject> sceneObjects, Vector3 position, Vector3 scale, bool isVertical)
    {
        var door = new GameObject(position, Vector3.Zero, scale);
        door.Tag = "Door";
        door.AddComponent(new Renderer(Mesh.CreateCube()));
        door.AddComponent(new AABBCollider(door.Transform));
        door.AddComponent(new Door(isVertical)); 
        sceneObjects.Add(door);
    }

    /// Generate random obstacles in each room (pillars and cubes)
    private void GenerateObstacles(List<GameObject> sceneObjects)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 roomCenter = new Vector3(x * ROOM_SIZE, 0, z * ROOM_SIZE);

                // Skip center room (player starts here)
                if (x == 0 && z == 0) continue;

                // 50% chance to spawn a pillar
                if (_random.NextDouble() < 0.5)
                {
                    Vector3 pillarOffset = new Vector3(
                        (float)(_random.NextDouble() - 0.5) * (ROOM_SIZE - 1.0f),
                        ROOM_HEIGHT / 2.0f,
                        (float)(_random.NextDouble() - 0.5) * (ROOM_SIZE - 1.0f)
                    );
                    Vector3 pillarPos = roomCenter + pillarOffset;
                    CreatePillar(sceneObjects, pillarPos);
                }

                // Spawn 0-2 random cubes (stones)
                int cubeCount = _random.Next(0, 3);
                for (int i = 0; i < cubeCount; i++)
                {
                    float cubeSize = (float)(_random.NextDouble() * 0.3 + 0.3); // 0.3m to 0.6m
                    Vector3 cubeOffset = new Vector3(
                        (float)(_random.NextDouble() - 0.5) * (ROOM_SIZE - 1.0f),
                        cubeSize / 2.0f,
                        (float)(_random.NextDouble() - 0.5) * (ROOM_SIZE - 1.0f)
                    );
                    Vector3 cubePos = roomCenter + cubeOffset;
                    CreateCube(sceneObjects, cubePos, cubeSize);
                }
            }
        }
    }

    /// Create a pillar (full height from floor to ceiling)
    private void CreatePillar(List<GameObject> sceneObjects, Vector3 position)
    {
        var pillar = new GameObject(position, Vector3.Zero, new Vector3(0.5f, ROOM_HEIGHT, 0.5f));
        pillar.Tag = "Pillar";
        pillar.AddComponent(new Renderer(Mesh.CreateCube()));
        pillar.AddComponent(new AABBCollider(pillar.Transform));
        sceneObjects.Add(pillar);
    }

    /// Create a small cube obstacle (stone)
    private void CreateCube(List<GameObject> sceneObjects, Vector3 position, float size)
    {
        var cube = new GameObject(position, Vector3.Zero, new Vector3(size, size, size));
        cube.Tag = "Obstacle";
        cube.AddComponent(new Renderer(Mesh.CreateCube()));
        cube.AddComponent(new AABBCollider(cube.Transform));
        sceneObjects.Add(cube);
    }

    /// Place goal object in a random room (not center)
    private GameObject GenerateGoalObject(List<GameObject> sceneObjects)
    {
        // Pick a random room (excluding center 0,0)
        int x, z;
        do
        {
            x = _random.Next(-1, 2);
            z = _random.Next(-1, 2);
        } while (x == 0 && z == 0);

        Vector3 goalPos = new Vector3(x * ROOM_SIZE, 0.5f, z * ROOM_SIZE);
        
        var goal = new GameObject(goalPos, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f));
        goal.Tag = "Goal";
        goal.AddComponent(new Renderer(Mesh.CreateCube()));
        // NO COLLIDER -  player should walk through it to trigger win
        sceneObjects.Add(goal);

        return goal;
    }
}

/// Door component - tracks door state (open/closed) and handles toggling
public class Door : IComponent
{
    public GameObject GameObject { get; set; }
    public bool IsOpen { get; private set; } = false;
    public bool IsVertical { get; private set; } // For rotation direction
    
    public Vector3 ClosedPosition { get; private set; } // Public so Player can check distance to this
    private Vector3 _openPosition;

    public Door(bool isVertical)
    {
        IsVertical = isVertical;
    }

    /// Initialize door positions (called once when door is first toggled)
    private void InitializePositions(GameObject doorObject)
    {
        if (ClosedPosition == Vector3.Zero)
        {
            ClosedPosition = doorObject.Transform.Position;
            // Open position = slide down by door height (2m)
            _openPosition = ClosedPosition - new Vector3(0, 2.0f, 0);
        }
    }

    /// Toggle door open/closed state
    /// When open, door slides down into the floor
    public void Toggle(GameObject doorObject)
    {
        InitializePositions(doorObject);
        
        IsOpen = !IsOpen;

        if (IsOpen)
        {
            // Slide door down into floor
            doorObject.Transform.Position = _openPosition;
        }
        else
        {
            // Slide door back up to closed position
            doorObject.Transform.Position = ClosedPosition;
        }
    }
}