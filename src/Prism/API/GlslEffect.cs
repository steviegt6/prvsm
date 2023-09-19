using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Veldrid.SPIRV;

namespace Prism.API;

/// <summary>
///     A GLSL effect.
/// </summary>
public class GlslEffect {
    private class GlslEffectImpl : Effect {
        public GlslEffectImpl(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) {
            throw new InvalidOperationException("GlslEffectImpl should never be instantiated through a constructor.");
        }

        protected GlslEffectImpl(Effect cloneSource) : base(cloneSource) {
            throw new InvalidOperationException("GlslEffectImpl should never be instantiated through a constructor.");
        }

        protected internal void InitializeWithCompilation(VertexFragmentCompilationResult compilation) {
            // Parameters = new EffectParameterCollection(this, compilation.Parameters);
        }

        public override Effect Clone() {
            throw new InvalidOperationException("GlslEffectImpl cannot be cloned directly.");
        }

        protected sealed override void OnApply() {
            base.OnApply();
        }

        protected override void Dispose(bool disposing) {
            // base.Dispose(disposing);
            // TODO: Implement this.
        }
    }

    /// <summary>
    ///     The associated FNA effect.
    /// </summary>
    public Effect FnaEffect { get; }

    public GlslEffect(GraphicsDevice graphicsDevice, VertexFragmentCompilationResult compilation) {
        var objEffect = FormatterServices.GetSafeUninitializedObject(typeof(GlslEffectImpl));
        if (objEffect is not GlslEffectImpl effect)
            throw new InvalidOperationException("Failed to create a GlslEffectImpl instance.");

        var effectType = typeof(Effect);
        var graphicsDeviceField = effectType.GetField("graphicsDevice", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        graphicsDeviceField.SetValue(effect, graphicsDevice);
        effect.InitializeWithCompilation(compilation);
        effect.CurrentTechnique = effect.Techniques[0];

        FnaEffect = effect;
    }
}
