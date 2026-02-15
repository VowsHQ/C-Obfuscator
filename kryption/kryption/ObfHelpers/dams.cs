using dnlib.DotNet;
using System.Runtime.Remoting.Contexts;

namespace kryption.ObfuscationAddons
{
    internal class AntiIldasm
    {
        public static void Execute(Context ctx)
        {
          
            foreach (ModuleDefMD module in ctx.assemblyDef.Modules)
            {
                TypeRef attrRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "SuppressIldasmAttribute");
                var ctorRef = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef);

                var attr = new CustomAttribute(ctorRef);
                module.CustomAttributes.Add(attr);
            }
        }
    }
}