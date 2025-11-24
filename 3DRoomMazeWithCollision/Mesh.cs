namespace _3DRoomMazeWithCollision;

public class Mesh
{
    public float[] Vertices { get; private set; }
    public uint[] Indices { get; private set; }

    public Mesh(float[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }

    // Cube with ONLY positions 
    public static Mesh CreateCube()
    {
        float[] vertices = {
            // Front face
            -0.5f, -0.5f,  0.5f,
            0.5f, -0.5f,  0.5f,
            0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            // Back face
            -0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f
        };

        uint[] indices = {
            // Front
            0, 1, 2, 2, 3, 0,
            // Back
            4, 6, 5, 6, 4, 7,
            // Left
            4, 0, 3, 3, 7, 4,
            // Right
            1, 5, 6, 6, 2, 1,
            // Top
            3, 2, 6, 6, 7, 3,
            // Bottom
            4, 5, 1, 1, 0, 4
        };

        return new Mesh(vertices, indices);
    }
}