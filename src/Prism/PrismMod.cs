using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Veldrid;

namespace Prism;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class PrismMod : Mod {
    private const string lib_veldrid_spirv_name = "libveldrid-spirv";
    private static readonly string[] native_extensions = { "dll", "so", "dylib", };

    private static string PrismDir => Path.Combine(Main.SavePath, "prvsm");

    public override void Load() {
        base.Load();

        var asm = typeof(PrismMod).Assembly;
        var rid = GetRid();
        var found = false;
        string realExt = null!;

        foreach (var ext in native_extensions) {
            var path = $"Prism.lib.{rid.os}_{rid.arch}.{lib_veldrid_spirv_name}.{ext}";
            var stream = asm.GetManifestResourceStream(path);
            if (stream is null)
                continue;

            found = true;
            realExt = ext;

            stream.Dispose();
        }

        if (!found)
            throw new PlatformNotSupportedException("No libveldrid-spirv native library found for this platform.");

        Directory.CreateDirectory(PrismDir);

        var count = 0;
        var fileName = $"{lib_veldrid_spirv_name}.{realExt}";
        var filePath = Path.Combine(PrismDir, fileName);

        while (File.Exists(filePath)) {
            try {
                File.Delete(filePath);
            }
            catch {
                filePath = Path.Combine(PrismDir, $"{lib_veldrid_spirv_name}.{count++}.{realExt}");
            }
        }

        {
            using (var stream = asm.GetManifestResourceStream($"Prism.lib.{rid.os}_{rid.arch}.{lib_veldrid_spirv_name}.{realExt}"))
            using (var file = File.OpenWrite(filePath))
                stream!.CopyTo(file);

            var ptr = NativeLibrary.Load(filePath);
            Logger.Info($"Loaded libveldrid-spirv native library @ 0x{ptr:X8}.");
        }

        var a = typeof(Effect);
        // TODO: What options matter for shader compilation?
        /*var veldridGdOptions = new GraphicsDeviceOptions();
        var veldridGd = VeldridStartup.CreateDefaultOpenGlGraphicsDevice(veldridGdOptions, Main.instance.Window.Handle);*/
    }

    private static (string os, string arch) GetRid() {
        var arch = RuntimeInformation.ProcessArchitecture switch {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            Architecture.Wasm => "wasm",
            Architecture.S390x => "s390x",
            _ => throw new NotSupportedException("Unknown process architecture"),
        };

        string os;
        if (OperatingSystem.IsWindows())
            os = "win";
        else if (OperatingSystem.IsLinux())
            os = "linux";
        else if (OperatingSystem.IsMacOS())
            os = "osx";
        else if (OperatingSystem.IsFreeBSD())
            os = "freebsd";
        else if (OperatingSystem.IsAndroid())
            os = "android";
        else if (OperatingSystem.IsIOS())
            os = "ios";
        else if (OperatingSystem.IsBrowser())
            os = "browser";
        else
            throw new NotSupportedException("Unknown operating system");

        return (os, arch);
    }
}
