namespace _3DRoomMazeWithCollision;

using OpenTK.Mathematics;
using System.Collections.Generic;

/// Represents any object in the 3D scene (player, walls, doors, obstacles, etc.)
public class GameObject
{
    public Transform Transform { get; set; }
    public string Tag { get; set; } = "Untagged"; // For identifying special objects (e.g., "Goal", "Door")
    
    private List<object> _components = new List<object>();

    public GameObject(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Transform = new Transform(position, rotation, scale);
    }

    /// Add a component to this GameObject (e.g., Renderer, Collider)
    public void AddComponent(object component)
    {
        _components.Add(component);
        
        // If the component implements IComponent, link it back to this GameObject
        if(component is IComponent c)
        {
            c.GameObject = this;
        }
    }

    /// Get a component of a specific type attached to this GameObject
    public T GetComponent<T>() where T : class
    {
        foreach(var component in _components)
        {
            if(component is T castedComponent)
            {
                return castedComponent;
            }
        }
        return null;
    }

    /// Check if this GameObject has a component of a specific type
    public bool HasComponent<T>() where T : class
    {
        return GetComponent<T>() != null;
    }
}

/// Interface for components that need a reference back to their GameObject
public interface IComponent
{
    GameObject GameObject { get; set; }
}

/// Represents the position, rotation, and scale of a GameObject in 3D space
public class Transform
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; } // Euler angles in degrees
    public Vector3 Scale { get; set; }

    public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    /// Calculates the model matrix (Scale -> Rotate -> Translate)
    public Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale) *
               Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
               Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
               Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z)) *
               Matrix4.CreateTranslation(Position);
    }
}