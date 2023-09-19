using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;

namespace Prism.API; 

/// <summary>
///     A GLSL effect.
/// </summary>
public class GlslEffect {
    /// <summary>
    ///     The associated FNA effect.
    /// </summary>
    public Effect FnaEffect { get; }

    private readonly GraphicsDevice graphicsDevice;

    public GlslEffect(GraphicsDevice graphicsDevice, byte[] effectCode) {
        this.graphicsDevice = graphicsDevice;
        
        var objEffect = FormatterServices.GetSafeUninitializedObject(typeof(Effect));
        if (objEffect is not Effect effect)
            throw new InvalidOperationException("Failed to create an Effect instance.");

        FnaEffect = effect;
    }
}
