using System.Text;

namespace Prism.API;

public readonly struct ShaderByteRepresentation {
    public byte[] Bytes { get; }

    public ShaderByteRepresentation(byte[] bytes) {
        Bytes = bytes;
    }

    public ShaderByteRepresentation(string source) {
        Bytes = Encoding.ASCII.GetBytes(source);
    }

    public static implicit operator ShaderByteRepresentation(byte[] bytes) => new(bytes);

    public static implicit operator ShaderByteRepresentation(string source) => new(source);

    public static implicit operator byte[](ShaderByteRepresentation sbr) => sbr.Bytes;
}
