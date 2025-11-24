namespace _3DRoomMazeWithCollision;

using OpenTK.Graphics.OpenGL4;

public class Renderer : IComponent
{
    public GameObject GameObject { get; set; }
    
    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _indexCount;

    public Renderer(Mesh mesh)
    {
        _indexCount = mesh.Indices.Length;

        // Generate buffers
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        // Upload vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), 
            mesh.Vertices, BufferUsageHint.StaticDraw);

        // Upload index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indices.Length * sizeof(uint), 
            mesh.Indices, BufferUsageHint.StaticDraw);

        // Position attribute 
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    public void Draw()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
}