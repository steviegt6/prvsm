using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace Prism.API;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class EffectHooker : ModSystem {
    private ILHook? hookEffectCtorGraphicsDeviceByteArray;
    private ILHook? hookEffectCtorEffect;

    public override void Load() {
        base.Load();

        var effectType = typeof(Effect);
        var effectCtorGraphicsDeviceByteArray = effectType.GetConstructor(new[] { typeof(GraphicsDevice), typeof(byte[]) });
        var effectCtorEffect = effectType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, new[] { typeof(Effect) });

        hookEffectCtorGraphicsDeviceByteArray = new ILHook(effectCtorGraphicsDeviceByteArray!, EffectCtorTransformerCreateEffect);
        hookEffectCtorEffect = new ILHook(effectCtorEffect!, EffectCtorTransformerCloneEffect);
    }

    private void EffectCtorTransformerCreateEffect(ILContext il) {
        var c = new ILCursor(il);

        // var label = il.DefineLabel();
        if (!c.TryGotoNext(MoveType.Before, x => x.MatchCall(typeof(FNA3D), "FNA3D_CreateEffect")))
            throw new Exception("Failed to find FNA3D_CreateEffect call.");

        c.Remove();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(CreateEffect);

        /*c.Emit(OpCodes.Brtrue_S, label);

        if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(FNA3D), "FNA3D_CreateEffect")))
            throw new Exception("Failed to find FNA3D_CreateEffect call.");

        c.MarkLabel(label);*/
    }

    private void EffectCtorTransformerCloneEffect(ILContext il) {
        var c = new ILCursor(il);

        // var label = il.DefineLabel();
        if (!c.TryGotoNext(MoveType.Before, x => x.MatchCall(typeof(FNA3D), "FNA3D_CloneEffect")))
            throw new Exception("Failed to find FNA3D_CloneEffect call.");

        c.Remove();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(CloneEffect);

        /*c.Emit(OpCodes.Brtrue_S, label);

        if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(FNA3D), "FNA3D_CloneEffect")))
            throw new Exception("Failed to find FNA3D_CloneEffect call.");

        c.MarkLabel(label);*/
    }

    private static void CreateEffect(
        nint device,
        byte[] effectCode,
        int length,
        out nint effect,
        out nint effectData,
        Effect @this
    ) {
        FNA3D.FNA3D_CreateEffect(
            device,
            effectCode,
            length,
            out effect,
            out effectData
        );
    }

    private static void CloneEffect(
        IntPtr device,
        IntPtr cloneSource,
        out IntPtr effect,
        out IntPtr effectData,
        Effect @this
    ) {
        FNA3D.FNA3D_CloneEffect(
            device,
            cloneSource,
            out effect,
            out effectData
        );
    }
}
