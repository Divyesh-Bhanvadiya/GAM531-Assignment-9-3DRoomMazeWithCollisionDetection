namespace _3DRoomMazeWithCollision;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

/// Player controller with first-person movement, mouse look, and collision detection
public class Player
{
    public Camera Camera { get; private set; }
    public Transform Transform { get; private set; }
    public AABBCollider Collider { get; private set; }
    public bool HasWon { get; private set; } = false;
    
    private float _speed = 3.0f; // 3 meters per second
    private float _mouseSensitivity = 0.1f;
    private float _interactionRange = 1.0f; // Can interact with doors within 1m
    
    // For mouse delta calculation
    private Vector2 _lastMousePos;
    private bool _firstMove = true;

    public Player(Vector3 startPosition, float aspectRatio)
    {
        Transform = new Transform(startPosition, Vector3.Zero, Vector3.One);
        
        // Camera positioned at eye height (player height 1.5m, eyes at 1.4m from feet)
        Camera = new Camera(startPosition + new Vector3(0, 0.65f, 0), aspectRatio);
        
        // Player collider: 0.2m wide x 1.5m tall x 0.2m deep 
        Collider = new AABBCollider(Transform, new Vector3(0.2f, 1.5f, 0.2f));
    }

    /// Update player movement and camera with collision detection
    public void Update(double deltaTime, KeyboardState keyboard, MouseState mouse, List<GameObject> sceneObjects)
    {
        // Don't allow movement if player has won
        if (HasWon) return;

        // === MOUSE LOOK ===
        HandleMouseLook(mouse);

        // === DOOR INTERACTION (Press E) ===
        if (keyboard.IsKeyPressed(Keys.E))
        {
            TryInteractWithDoor(sceneObjects);
        }

        // === KEYBOARD MOVEMENT WITH COLLISION ===
        Vector3 moveDirection = Vector3.Zero;

        // Forward/Backward (W/S)
        if (keyboard.IsKeyDown(Keys.W))
            moveDirection += Camera.Front;
        if (keyboard.IsKeyDown(Keys.S))
            moveDirection -= Camera.Front;

        // Left/Right (A/D) - 
        if (keyboard.IsKeyDown(Keys.A))
            moveDirection -= Camera.Right;
        if (keyboard.IsKeyDown(Keys.D))
            moveDirection += Camera.Right;

        // Normalize and apply speed
        if (moveDirection.LengthSquared > 0)
        {
            moveDirection.Normalize();
            
            // Flatten to XZ plane (no vertical movement)
            moveDirection.Y = 0;
            if (moveDirection.LengthSquared > 0)
                moveDirection.Normalize();
            
            Vector3 velocity = moveDirection * _speed * (float)deltaTime;
            
            // Apply collision resolution
            Vector3 newPosition = ResolveCollisions(Transform.Position, velocity, sceneObjects);
            Transform.Position = newPosition;
        }

        // === GOAL DETECTION ===
        CheckGoalCollision(sceneObjects);

        // Update camera position to follow player
        Camera.Position = Transform.Position + new Vector3(0, 0.65f, 0);
    }

    /// Handle mouse look (first-person camera rotation)
    private void HandleMouseLook(MouseState mouse)
    {
        if (_firstMove)
        {
            _lastMousePos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
            return;
        }

        // Calculate mouse delta
        float deltaX = mouse.X - _lastMousePos.X;
        float deltaY = mouse.Y - _lastMousePos.Y;
        _lastMousePos = new Vector2(mouse.X, mouse.Y);

        // Update camera rotation
        Camera.UpdateMouseLook(deltaX, deltaY, _mouseSensitivity);
    }

    /// Try to interact with nearby door (toggle open/closed)
    /// Checks distance to door's CLOSED position (so it works even when door is underground)
    private void TryInteractWithDoor(List<GameObject> sceneObjects)
    {
        foreach (var obj in sceneObjects)
        {
            if (obj.Tag == "Door")
            {
                var door = obj.GetComponent<Door>();
                if (door != null)
                {
                    // Check distance to the door's closed position 
                    Vector3 doorPosition = door.ClosedPosition != Vector3.Zero 
                        ? door.ClosedPosition 
                        : obj.Transform.Position;
                    
                    float distance = (doorPosition - Transform.Position).Length;
                    
                    if (distance <= _interactionRange)
                    {
                        door.Toggle(obj);
                        return; // Only interact with one door at a time
                    }
                }
            }
        }
    }

    /// Check if player is close enough to the goal object
    private void CheckGoalCollision(List<GameObject> sceneObjects)
    {
        foreach (var obj in sceneObjects)
        {
            if (obj.Tag == "Goal")
            {
                // Check distance 
                float distance = (obj.Transform.Position - Transform.Position).Length;
                
                if (distance <= 0.5f) // Within 0.5 meter
                {
                    HasWon = true;
                    return;
                }
            }
        }
    }

    /// Resolve collisions by checking X and Z axes separately (sliding behavior)
    /// This allows the player to slide along walls instead of getting stuck
    /// Skips collision check for open doors
    private Vector3 ResolveCollisions(Vector3 currentPos, Vector3 velocity, List<GameObject> sceneObjects)
    {
        Vector3 nextPos = currentPos;

        // === CHECK X-AXIS MOVEMENT ===
        nextPos.X += velocity.X;
        Collider.UpdatePosition(nextPos);
        
        bool xCollision = false;
        foreach(var obj in sceneObjects)
        {
            var otherCollider = obj.GetComponent<AABBCollider>();
            if(otherCollider != null)
            {
                // Skip collision check for open doors
                var door = obj.GetComponent<Door>();
                if (door != null && door.IsOpen)
                    continue;

                if(Collider.Intersects(otherCollider))
                {
                    nextPos.X = currentPos.X; // Revert X movement
                    xCollision = true;
                    break;
                }
            }
        }

        // === CHECK Z-AXIS MOVEMENT ===
        nextPos.Z += velocity.Z;
        Collider.UpdatePosition(nextPos);
        
        foreach(var obj in sceneObjects)
        {
            var otherCollider = obj.GetComponent<AABBCollider>();
            if(otherCollider != null)
            {
                // Skip collision check for open doors
                var door = obj.GetComponent<Door>();
                if (door != null && door.IsOpen)
                    continue;

                if(Collider.Intersects(otherCollider))
                {
                    nextPos.Z = currentPos.Z; // Revert Z movement
                    break;
                }
            }
        }

        // Update collider to final position
        Collider.UpdatePosition(nextPos);
        
        return nextPos;
    }
}