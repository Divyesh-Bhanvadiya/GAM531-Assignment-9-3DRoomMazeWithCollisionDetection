namespace _3DRoomMazeWithCollision;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

public class Shader
{
    public int Handle { get; private set; }

    public Shader(string vertPath, string fragPath)
    {
        string vertexCode = File.ReadAllText(vertPath);
        string fragmentCode = File.ReadAllText(fragPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexCode);
        GL.CompileShader(vertexShader);
        CheckShaderErrors(vertexShader, "VERTEX");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentCode);
        GL.CompileShader(fragmentShader);
        CheckShaderErrors(fragmentShader, "FRAGMENT");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        CheckProgramErrors(Handle);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, false, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, data);
    }

    private void CheckShaderErrors(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"ERROR::SHADER::{type}::COMPILATION_FAILED\n{infoLog}");
        }
    }

    private void CheckProgramErrors(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(program);
            throw new Exception($"ERROR::PROGRAM::LINKING_FAILED\n{infoLog}");
        }
    }
}