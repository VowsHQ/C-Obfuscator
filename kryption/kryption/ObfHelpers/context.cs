using dnlib.DotNet;

public class Context
{
    public AssemblyDef assemblyDef;
    public ModuleDef moduleDef;
    public ModuleDefMD moduleDefMD;
    public TypeDef typeDef;
    public Importer importer;
    public MethodDef cctor;
    public ModuleDef Module { get; set; }
    public ModuleDef module { get; set; }

    public Context(AssemblyDef asm)
    {
        assemblyDef = asm;
        moduleDef = asm.ManifestModule;
        typeDef = moduleDef.GlobalType;
        importer = new Importer(moduleDef);
        cctor = typeDef.FindOrCreateStaticConstructor();
    }
}