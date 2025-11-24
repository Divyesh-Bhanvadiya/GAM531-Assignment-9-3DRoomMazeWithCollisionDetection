using OpenTK.Windowing.GraphicsLibraryFramework;

namespace _3DRoomMazeWithCollision;

using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Collections.Generic;

public class Game : GameWindow
{
    private List<GameObject> _sceneObjects = new List<GameObject>();
    private Player _player;
    private Shader _shader;
    private GameObject _goalObject;
    private bool _gameWon = false;

    public Game(int width, int height, string title) 
        : base(GameWindowSettings.Default, new NativeWindowSettings() 
        { 
            Size = (width, height), 
            Title = title,
            API = ContextAPI.OpenGL,
            APIVersion = new System.Version(3, 3)
        })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        // Capture mouse cursor 
        CursorState = CursorState.Grabbed;

        // Initialize Shader
        _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        // Setup Player at center room, standing on ground
        _player = new Player(new Vector3(0, 0.75f, 0), Size.X / (float)Size.Y);

        // Generate 3x3 room grid with doors, obstacles and goalObject
        var roomGen = new RoomGenerator(_shader);
        roomGen.GenerateRooms(_sceneObjects, out _goalObject);
    }

    // scene for testing
    private void CreateTestScene()
    {
        // Floor (large flat plane) - no collider needed, just visual
        var floor = new GameObject(new Vector3(0, 0, 0), Vector3.Zero, new Vector3(20, 0.1f, 20));
        floor.AddComponent(new Renderer(Mesh.CreateCube()));
        _sceneObjects.Add(floor);

        // A few test cubes to walk around - WITH COLLIDERS
        var cube1 = new GameObject(new Vector3(2, 0.5f, 0), Vector3.Zero, Vector3.One);
        cube1.AddComponent(new Renderer(Mesh.CreateCube()));
        cube1.AddComponent(new AABBCollider(cube1.Transform)); 
        _sceneObjects.Add(cube1);

        var cube2 = new GameObject(new Vector3(-2, 0.5f, 0), Vector3.Zero, Vector3.One);
        cube2.AddComponent(new Renderer(Mesh.CreateCube()));
        cube2.AddComponent(new AABBCollider(cube2.Transform)); 
        _sceneObjects.Add(cube2);

        var cube3 = new GameObject(new Vector3(0, 0.5f, -3), Vector3.Zero, Vector3.One);
        cube3.AddComponent(new Renderer(Mesh.CreateCube()));
        cube3.AddComponent(new AABBCollider(cube3.Transform)); 
        _sceneObjects.Add(cube3);

        // Add walls to test collision
        var wallLeft = new GameObject(new Vector3(-5, 1.25f, 0), Vector3.Zero, new Vector3(0.5f, 2.5f, 10));
        wallLeft.AddComponent(new Renderer(Mesh.CreateCube()));
        wallLeft.AddComponent(new AABBCollider(wallLeft.Transform));
        _sceneObjects.Add(wallLeft);

        var wallRight = new GameObject(new Vector3(5, 1.25f, 0), Vector3.Zero, new Vector3(0.5f, 2.5f, 10));
        wallRight.AddComponent(new Renderer(Mesh.CreateCube()));
        wallRight.AddComponent(new AABBCollider(wallRight.Transform));
        _sceneObjects.Add(wallRight);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        
        // Exit on ESC
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        // Update player movement and camera with collision detection
        _player.Update(e.Time, KeyboardState, MouseState, _sceneObjects);

        // Check if player won
        if (_player.HasWon && !_gameWon)
        {
            _gameWon = true;
            Console.WriteLine("=== YOU WIN! ===");
            Console.WriteLine("Goal reached! Press ESC to exit.");
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _shader.SetMatrix4("view", _player.Camera.GetViewMatrix());
        _shader.SetMatrix4("projection", _player.Camera.GetProjectionMatrix());

        // Render all scene objects
        foreach(var obj in _sceneObjects)
        {
            var renderer = obj.GetComponent<Renderer>();
            if(renderer != null)
            {
                _shader.SetMatrix4("model", obj.Transform.GetModelMatrix());
                
                // Color coding by tag
                Vector3 color;
                if (obj.Tag == "Floor")
                    color = new Vector3(0.2f, 0.2f, 0.25f); // Dark gray floor
                else if (obj.Tag == "Wall")
                    color = new Vector3(0.5f, 0.5f, 0.55f); // Light gray walls
                else if (obj.Tag == "Door")
                    color = new Vector3(0.6f, 0.4f, 0.2f); // Brown doors
                else if (obj.Tag == "Pillar")
                    color = new Vector3(0.4f, 0.3f, 0.25f); // Dark brown pillars
                else if (obj.Tag == "Obstacle")
                    color = new Vector3(0.6f, 0.5f, 0.4f); // Light brown cubes
                else if (obj.Tag == "Goal")
                    color = new Vector3(1.0f, 0.8f, 0.0f); // Gold goal object
                else
                    color = new Vector3(0.8f, 0.3f, 0.3f); // Default red
                
                _shader.SetVector3("objectColor", color);
                
                renderer.Draw();
            }
        }
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        CursorState = CursorState.Normal;
    }
}