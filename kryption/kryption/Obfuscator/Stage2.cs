using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using kryption.ObfHelpers;
using kryption.ObfuscationAddons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kryption.Obfuscator
{
    internal class Stage2
    {
        public static void secondary(ModuleDefMD module, string fullPath)
        {

            var directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);




            FakeMethodCalls.Obfuscate(module);

            ControlFlow.Obfuscate(module);

            Anti_Debug.Execute(module);


            module.Write(fullPath);
        }
    }

    public static class FakeMethodCalls
    {
        private static readonly Random random = new Random();

        private static readonly List<string> FakeMethodNames = new List<string>
        {
            "InitializeCryptoEngine",
            "ValidateProtectionScheme",
            "LoadAntiDebugToolkit",
            "ExecuteRuntimeValidation",
            "StartObfuscationProcess",
            "InjectSecurityMarker",
            "VerifyIntegrityHash",
            "LaunchProtectionPayload",
            "RunVirtualMachine",
            "ActivateShieldingMechanism"
        };

        public static void Obfuscate(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                AddFakeMethods(type);
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body.Instructions.Count < 5) continue;
                    InsertFakeMethodCalls(method, type);
                }
            }
        }

        private static void AddFakeMethods(TypeDef type)
        {
            foreach (var fakeName in FakeMethodNames)
            {
                var fakeMethod = new MethodDefUser(
                    fakeName,
                    MethodSig.CreateStatic(type.Module.CorLibTypes.Void),
                    MethodAttributes.Public | MethodAttributes.Static);

                var body = new CilBody();
                body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "Fake method invoked"));
                body.Instructions.Add(Instruction.Create(OpCodes.Pop));
                body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                fakeMethod.Body = body;

                type.Methods.Add(fakeMethod);
            }
        }

        private static void InsertFakeMethodCalls(MethodDef method, TypeDef type)
        {
            var instructions = method.Body.Instructions;

            var fakeMethod = type.Methods[random.Next(type.Methods.Count)];
            if (!FakeMethodNames.Contains(fakeMethod.Name)) return;

            var fakeBlock = GenerateUnreachableBlock(fakeMethod);
            int insertionIndex = random.Next(0, instructions.Count);
            foreach (var instr in fakeBlock)
            {
                instructions.Insert(insertionIndex, instr);
                insertionIndex++;
            }

            method.Body.SimplifyBranches();
            method.Body.OptimizeBranches();
        }

        private static List<Instruction> GenerateUnreachableBlock(MethodDef fakeMethod)
        {
            var nopInstruction = Instruction.Create(OpCodes.Nop);
            return new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Brtrue, nopInstruction),
                Instruction.Create(OpCodes.Call, fakeMethod),
                nopInstruction
            };
        }
    }

    public static class ControlFlow
    {
        private static readonly Random random = new Random();

        public static void Obfuscate(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body.Instructions.Count < 5) continue;
                    ObfuscateMethod(method);
                }
            }
        }

        private static void ObfuscateMethod(MethodDef method)
        {
            var body = method.Body;
            var instructions = body.Instructions;
            var newInstructions = new List<Instruction>(instructions.Count);

            var shuffledBlocks = SplitInstructionsIntoBlocks(instructions);

            foreach (var block in shuffledBlocks)
            {
                var randomValue = random.Next(0, 2);
                newInstructions.Add(Instruction.Create(OpCodes.Ldc_I4, randomValue));
                var branchTarget = block[0];
                var branchInstruction = Instruction.Create(OpCodes.Brfalse, branchTarget);
                newInstructions.Add(branchInstruction);

                newInstructions.AddRange(block);
                branchInstruction.Operand = branchTarget;
            }

            if (method.ReturnType.ElementType == ElementType.Void)
            {
                newInstructions.Add(Instruction.Create(OpCodes.Ret));
            }

            instructions.Clear();
            foreach (var instr in newInstructions)
            {
                instructions.Add(instr);
            }

            body.SimplifyBranches();
            body.OptimizeBranches();
        }

        private static List<List<Instruction>> SplitInstructionsIntoBlocks(IList<Instruction> instructions)
        {
            var blocks = new List<List<Instruction>>();
            var currentBlock = new List<Instruction>();

            foreach (var instr in instructions)
            {
                currentBlock.Add(instr);
                if (random.Next(0, 3) == 0 && currentBlock.Count > 0)
                {
                    blocks.Add(new List<Instruction>(currentBlock));
                    currentBlock.Clear();
                }
            }

            if (currentBlock.Count > 0)
            {
                blocks.Add(currentBlock);
            }

            return blocks;
        }
    }

    internal class AntiDecompile
    {
        public static void Execute(AssemblyDef mod)
        {
            foreach (var module in mod.Modules)
            {
                var interfaceM = new InterfaceImplUser(module.GlobalType);
                for (var i = 0; i < 1; i++)
                {
                    var typeDef1 = new TypeDefUser(string.Empty, $"Form{i}", module.CorLibTypes.GetTypeRef("System", "Attribute"));
                    var interface1 = new InterfaceImplUser(typeDef1);
                    module.Types.Add(typeDef1);
                    typeDef1.Interfaces.Add(interface1);
                    typeDef1.Interfaces.Add(interfaceM);
                }
            }
        }
    }
}

public static class Anti_Debug
{
    public static void Execute(ModuleDef module)
    {
        var typeModule = ModuleDefMD.Load(typeof(AntiDebugSafe).Module);
        var cctor = module.GlobalType.FindOrCreateStaticConstructor();
        var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(AntiDebugSafe).MetadataToken));
        var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
        var init = (MethodDef)members.Single(method => method.Name == "Initialize");
        cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
        foreach (var md in module.GlobalType.Methods)
        {
            if (md.Name != ".ctor") continue;
            module.GlobalType.Remove(md);
            break;
        }
    }
}


namespace NoisetteCore.Protection.ConstantDecomposition.Decompositor
{
    internal class DecompositorProcess
    {
        public static string[] test_list = new string[] { };

        public DecompositorProcess()
        {
            List<string> list = new List<string>();
            list.Add("1");
            list.Add("2");
            test_list = list.ToArray();
            tok_ = "123";
        }

        public static string tok_;

        public void test()
        {
            string test = tok_;
          
        }
    }
}
