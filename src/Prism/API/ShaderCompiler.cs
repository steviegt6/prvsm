using Veldrid.SPIRV;

namespace Prism.API;

public static class ShaderCompiler {
    public static VertexFragmentCompilationResult CompileVertexFragment(ShaderByteRepresentation vsBytes, ShaderByteRepresentation fsBytes) {
        return SpirvCompilation.CompileVertexFragment(vsBytes, fsBytes, CrossCompileTarget.GLSL);
    }
}
