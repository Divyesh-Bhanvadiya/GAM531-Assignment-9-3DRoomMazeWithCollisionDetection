namespace _3DRoomMazeWithCollision;

using OpenTK.Mathematics;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Front { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }

    private float _yaw = -90.0f;
    private float _pitch = 0.0f;
    private float _fov = 45.0f;
    private float _aspectRatio;

    public Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        _aspectRatio = aspectRatio;
        UpdateVectors();
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(_fov), _aspectRatio, 0.1f, 100.0f);
    }

    public void UpdateMouseLook(float deltaX, float deltaY, float sensitivity = 0.1f)
    {
        _yaw += deltaX * sensitivity;
        _pitch -= deltaY * sensitivity;

        // Clamp pitch
        if (_pitch > 89.0f) _pitch = 89.0f;
        if (_pitch < -89.0f) _pitch = -89.0f;

        UpdateVectors();
    }

    private void UpdateVectors()
    {
        Front = Vector3.Normalize(new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch))
        ));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}