namespace _3DRoomMazeWithCollision;

using OpenTK.Mathematics;


/// Axis-Aligned Bounding Box (AABB) collider for simple 3D collision detection
public class AABBCollider : IComponent
{
    public GameObject GameObject { get; set; }
    
    public Vector3 Center { get; private set; }
    public Vector3 Size { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }

    
    /// Constructor for static objects (size from transform scale).
    public AABBCollider(Transform transform)
    {
        Center = transform.Position;
        Size = transform.Scale;
        CalculateBounds();
    }
    
    /// Constructor for dynamic objects (like player) with fixed size
    public AABBCollider(Transform transform, Vector3 size)
    {
        Center = transform.Position;
        Size = size;
        CalculateBounds();
    }
    
    /// Update collider position (for moving objects like the player)
    public void UpdatePosition(Vector3 newCenter)
    {
        Center = newCenter;
        CalculateBounds();
    }

    /// Calculate Min and Max bounds from center and size
    private void CalculateBounds()
    {
        Min = Center - Size / 2.0f;
        Max = Center + Size / 2.0f;
    }

    /// Check if this AABB intersects with another AABB.
    /// Returns true if they overlap on ALL three axes (X, Y, Z)
    public bool Intersects(AABBCollider other)
    {
        return (this.Min.X <= other.Max.X && this.Max.X >= other.Min.X) &&
               (this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y) &&
               (this.Min.Z <= other.Max.Z && this.Max.Z >= other.Min.Z);
    }

    /// Debug visualization: Get the 8 corners of the AABB
    public Vector3[] GetCorners()
    {
        return new Vector3[]
        {
            new Vector3(Min.X, Min.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Max.Z)
        };
    }
}