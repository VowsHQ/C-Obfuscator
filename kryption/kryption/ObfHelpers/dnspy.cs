using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace kryption.ObfuscationAddons
{
    internal class AntiDnspy
    {
        public static void Execute(Context ctx)
        {
           

            foreach (TypeDef type in ctx.assemblyDef.ManifestModule.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null) continue;
                    for (int x = 0; x < 33333; x++)
                    {
                        method.Body.Instructions.Insert(x, new Instruction(OpCodes.Nop));
                    }
                }
            }
        }
    }
}