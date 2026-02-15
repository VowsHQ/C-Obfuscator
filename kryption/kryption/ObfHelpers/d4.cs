using dnlib.DotNet;
using kryption.ObfHelpers;

namespace kryption.ObfuscationAddons
{
    internal class AntiDe4dot
    {
        public static void Execute(Context context)
        {
            foreach (ModuleDef module in context.assemblyDef.Modules)
            {
                InterfaceImplUser invalidImpl = new InterfaceImplUser(module.GlobalType);

                for (int i = 100; i < 150; i++)
                {
                    TypeDefUser invalidType = new TypeDefUser("", Renamer.GenerateName(), module.CorLibTypes.GetTypeRef("System", "Attribute"));
                    InterfaceImplUser typeImpl = new InterfaceImplUser(invalidType);
                    module.Types.Add(invalidType);
                    invalidType.Interfaces.Add(typeImpl);
                    invalidType.Interfaces.Add(invalidImpl);
                }
            }
        }
    }
}