using dnlib.DotNet;
using dnlib.DotNet.Emit;
using kryption.ObfHelpers;
using kryption.Obfuscator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Protector.Protections
{
 
    internal class Stack
    {
        public static void Execute(Context context)
        {
            foreach (var type in context.module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var body = method.Body;
                    var target = body.Instructions[0];
                    var random = new Random();

           
                    for (int i = 0; i < 3; i++)
                    {
                        var randInstr = GetRandomInstruction(random);
                        body.Instructions.Insert(0, randInstr);
                    }


                    var opaquePredicate = Instruction.Create(OpCodes.Ldc_I4, 1);
                    var branch = Instruction.Create(OpCodes.Brfalse_S, target);
                    body.Instructions.Insert(0, opaquePredicate);
                    body.Instructions.Insert(1, branch);

            
                    var newItem = Instruction.Create(OpCodes.Br_S, target);
                    var popItem = Instruction.Create(OpCodes.Pop);
                    body.Instructions.Insert(0, newItem);
                    body.Instructions.Insert(0, popItem);

            
                    foreach (var handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart == target) handler.TryStart = newItem;
                        if (handler.HandlerStart == target) handler.HandlerStart = newItem;
                        if (handler.FilterStart == target) handler.FilterStart = newItem;
                    }
                }
            }
        
         }
         
        

        private static Instruction GetRandomInstruction(Random random)
        {
            var opcode = random.Next(0, 8); 
            switch (opcode)
            {
                case 0: return Instruction.Create(OpCodes.Ldnull);
                case 1: return Instruction.Create(OpCodes.Ldc_I4_0);
                case 2: return Instruction.Create(OpCodes.Ldstr, "Obfuscation");
                case 3: return Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                case 4: return Instruction.Create(OpCodes.Nop);
                case 5: return Instruction.Create(OpCodes.Ldc_I4, random.Next(0, 100));
                case 6: return Instruction.Create(OpCodes.Pop);
                case 7: return Instruction.Create(OpCodes.Ret);
                default: return Instruction.Create(OpCodes.Nop);
            }
        }

    }
        internal class SufConfusionProtection
        {
        public static void Execute(Context context)
        {
            foreach (var type in context.module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var body = method.Body;
                    var target = body.Instructions[0];

                   
                    var newItem = Instruction.Create(OpCodes.Br_S, target);
                    var popItem = Instruction.Create(OpCodes.Pop);
                    var random = new Random();
                    Instruction newTarget;
                    switch (random.Next(0, 5))
                    {
                        case 0:
                            newTarget = Instruction.Create(OpCodes.Ldnull);
                            break;
                        case 1:
                            newTarget = Instruction.Create(OpCodes.Ldc_I4_0);
                            break;
                        case 2:
                            newTarget = Instruction.Create(OpCodes.Ldstr, "Isolator");
                            break;
                        case 3:
                            newTarget = Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                            break;
                        default:
                            newTarget = Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                            break;
                    }

                 
                    body.Instructions.Insert(0, newTarget);
                    body.Instructions.Insert(1, popItem);
                    body.Instructions.Insert(2, newItem);

          
                    foreach (var handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart == target)
                            handler.TryStart = newItem;
                        if (handler.HandlerStart == target)
                            handler.HandlerStart = newItem;
                        if (handler.FilterStart == target)
                            handler.FilterStart = newItem;
                    }
                }
            }
        }
    }

 
    internal class Encryption
    {
        private static readonly Random random = new Random();

        public static bool ObfuscateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder not found.");
                return false;
            }

            var files = Directory.GetFiles(folderPath, "*.*")
                .Where(f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var managedAssemblies = new List<(ModuleDefMD module, string fileName)>();

            foreach (var file in files)
            {
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var module = ModuleDefMD.Load(fs);
                        managedAssemblies.Add((module, Path.GetFileName(file)));
                        Console.WriteLine("Managed assembly detected: " + Path.GetFileName(file));
                    }
                }
                catch
                {
                    Console.WriteLine("Skipped (not managed or load failed): " + Path.GetFileName(file));
                }
            }

            if (managedAssemblies.Count == 0)
            {
                Console.WriteLine("No managed assemblies found to obfuscate.");
                return false;
            }

            string outputFolder = folderPath + "_obfuscated"; 
            Directory.CreateDirectory(outputFolder); 

            foreach (var (module, fileName) in managedAssemblies)
            {
                Console.WriteLine("Obfuscating: " + fileName);

                var cryptoRandom = new MutationHelper.CryptoRandom();

           
                RenameTypes(module, cryptoRandom);

              
                ObfuscateConsoleCalls(module);

        
                ExecuteConstMelting(module);

      
                ExecuteMutation(module);

         
                AddAntiDebugCheck(module);

      
                string newPath = Path.Combine(outputFolder, fileName);
                Stage2.secondary(module, newPath);
                Console.WriteLine("Saved: " + newPath);
            }

            Console.WriteLine("Obfuscation complete.");
            return true;
        }
        public static void secondary(ModuleDefMD module, string fullPath)
        {
      
            var directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);

      
            FakeMethodCalls.Obfuscate(module);
            ControlFlow.Obfuscate(module);
      

            module.Write(fullPath);
        }
        private static void RenameTypes(ModuleDefMD module, MutationHelper.CryptoRandom cryptoRandom)
        {
            foreach (var type in module.Types.ToArray())
            {
                if (type.IsGlobalModuleType) continue;

                type.Namespace = RandomString(7, cryptoRandom);
                type.Name = RandomString(7, cryptoRandom);

                foreach (var method in type.Methods)
                {
                    if (!method.IsConstructor &&
                        !method.IsRuntimeSpecialName &&
                        !method.IsVirtual &&
                        method != module.EntryPoint)
                    {
                        method.Name = RandomString(7, cryptoRandom);
                    }
                }

                foreach (var field in type.Fields)
                    field.Name = RandomString(7, cryptoRandom);

                foreach (var prop in type.Properties)
                    prop.Name = RandomString(7, cryptoRandom);
            }


            for (int i = 500; i < 1000; i++)
            {
                var dummy = new TypeDefUser(
                    RandomString(7, cryptoRandom),
                    RandomString(7, cryptoRandom),
                    module.CorLibTypes.Object.TypeRef
                );
                module.Types.Add(dummy);

                for (int m = 0; m < 5; m++)
                {
                    var dummyMethod = new MethodDefUser(
                        RandomString(7, cryptoRandom),
                        MethodSig.CreateStatic(module.CorLibTypes.Void),
                        MethodAttributes.Public | MethodAttributes.Static
                    )
                    {
                        Body = new CilBody()
                    };
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 12345));
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, " ݁ ݁ ݁ ݁"));
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    dummy.Methods.Add(dummyMethod);
                }

                var stringTypeSig = module.ImportAsTypeSig(typeof(string));
                var dummyField = new FieldDefUser(
                    RandomString(7, cryptoRandom),
                    new FieldSig(stringTypeSig),
                    FieldAttributes.Public | FieldAttributes.Static
                );
                dummy.Fields.Add(dummyField);
            }
        }

        private static string RandomString(int length, MutationHelper.CryptoRandom cryptoRandom)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCD𝜗𝜚⋆⤷ ゛ ˎˊ˗EFGHIJKLM♡˚₊‧꒰ა ≽^•⩊•^≼ ໒꒱ ‧₊˚♡NOPQRSTUV. ݁₊ ⊹ . ݁ ✩ ݁ . ⊹ ₊ ݁.WXYZ!@#$%^&*()_+=-0987654321 ִ ࣪𖤐.ᐟ⠘⡀⠀⠀⠀⠀⢈⣷⠤⠴⢺⣀⠀⠀⠀┊         ┊       ┊   ┊    ┊        ┊⠀⢀⡇.° ༘🎧⋆🖇₊˚ෆ";
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
                buffer[i] = chars[cryptoRandom.Next(chars.Length)];
            return new string(buffer);
        }

        public static void ObfuscateConsoleCalls(ModuleDefMD module)
        {
            var consoleObfType = module.Types.FirstOrDefault(t => t.Name == "ConsoleObf");
            if (consoleObfType == null) return;

            var wlMethod = consoleObfType.Methods.FirstOrDefault(m => m.Name == "WL");
            var wMethod = consoleObfType.Methods.FirstOrDefault(m => m.Name == "W");
            if (wlMethod == null || wMethod == null) return;

            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instrs = method.Body.Instructions;
                    for (int i = 0; i < instrs.Count; i++)
                    {
                        var instr = instrs[i];
                        if (instr.OpCode == OpCodes.Call && instr.Operand is IMethod call)
                        {
                            if (call.DeclaringType.FullName == "System.Console")
                            {
                                if (call.Name == "WriteLine")
                                    instr.Operand = wlMethod;
                                else if (call.Name == "Write")
                                    instr.Operand = wMethod;
                            }
                        }
                    }
                }
            }
        }

        private static void ExecuteConstMelting(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types.ToArray())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    ReplaceStringLiterals(method);
                    ReplaceIntLiterals(method);
                }
            }
        }

        private static void ReplaceStringLiterals(MethodDef methodDef)
        {
            if (!CanObfuscate(methodDef)) return;
            foreach (Instruction instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Ldstr) continue;
                MethodDef replacementMethod = new MethodDefUser(
                    Renamer.GenerateName(),
                    MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.String),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig
                )
                {
                    Body = new CilBody()
                };
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldstr, instruction.Operand.ToString()));
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                methodDef.DeclaringType.Methods.Add(replacementMethod);
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = replacementMethod;
            }
        }

        private static void ReplaceIntLiterals(MethodDef methodDef)
        {
            if (!CanObfuscate(methodDef)) return;
            foreach (Instruction instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Ldc_I4) continue;
                MethodDef replacementMethod = new MethodDefUser(
                    Renamer.GenerateName(),
                    MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.Int32),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig
                )
                {
                    Body = new CilBody()
                };
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, instruction.GetLdcI4Value()));
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                methodDef.DeclaringType.Methods.Add(replacementMethod);
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = replacementMethod;
            }
        }

        public static bool CanObfuscate(MethodDef methodDef)
        {
            if (!methodDef.HasBody) return false;
            if (!methodDef.Body.HasInstructions) return false;
            if (methodDef.DeclaringType.IsGlobalModuleType) return false;
            return true;
        }

   
        private static void ExecuteMutation(ModuleDefMD module)
        {
            var cryptoRandom = new MutationHelper.CryptoRandom();

            foreach (var type in module.GetTypes())
            {
                var listMethod = new List<MethodDef>();
                foreach (var method in type.Methods.Where(x => x.HasBody))
                {
                    var instructions = method.Body.Instructions;
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        if (instructions[i].IsLdcI4() && IsSafe(instructions.ToList(), i))
                        {
                            MethodDef refMethod = null;
                            int operand = instructions[i].GetLdcI4Value();
                            instructions[i].OpCode = OpCodes.Ldc_R8;

                            switch (cryptoRandom.Next(0, 3))
                            {
                                case 0:
                                    refMethod = GenerateRefMethod(module, "Floor");
                                    instructions[i].Operand = Convert.ToDouble(operand + cryptoRandom.NextDouble());
                                    break;
                                case 1:
                                    refMethod = GenerateRefMethod(module, "Sqrt");
                                    instructions[i].Operand = Math.Pow(Convert.ToDouble(operand), 2);
                                    break;
                                case 2:
                                    refMethod = GenerateRefMethod(module, "Round");
                                    instructions[i].Operand = Convert.ToDouble(operand);
                                    break;
                            }

                            instructions.Insert(i + 1, OpCodes.Call.ToInstruction(refMethod));
                            instructions.Insert(i + 2, OpCodes.Conv_I4.ToInstruction());
                            i += 2;
                            listMethod.Add(refMethod);
                        }
                    }
                    method.Body.SimplifyMacros(method.Parameters);
                }
                foreach (var method in listMethod)
                    type.Methods.Add(method);
            }
        }

        private static MethodDef GenerateRefMethod(ModuleDefMD module, string methodName)
        {
            var refMethod = new MethodDefUser(
                "_" + Guid.NewGuid().ToString("D").ToUpper().Substring(2, 5),
                MethodSig.CreateStatic(module.ImportAsTypeSig(typeof(double))),
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig
            );
            refMethod.Body = new CilBody();
            refMethod.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
            refMethod.Body.Instructions.Add(OpCodes.Call.ToInstruction(GetMethod(module, typeof(Math), methodName, new[] { typeof(double) })));
            refMethod.Body.Instructions.Add(OpCodes.Stloc_0.ToInstruction());
            refMethod.Body.Instructions.Add(OpCodes.Ldloc_0.ToInstruction());
            refMethod.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            return refMethod;
        }

        private static bool IsSafe(List<Instruction> instructions, int i)
        {
            return !new[] { -2, -1, 0, 1, 2 }.Contains(instructions[i].GetLdcI4Value());
        }

        private static IMethod GetMethod(ModuleDefMD module, Type type, string methodName, Type[] parameterTypes)
        {
            return module.Import(type.GetMethod(methodName, parameterTypes));
        }

        private static void AddAntiDebugCheck(ModuleDefMD module)
        {
            var type = new TypeDefUser("AntiDebug", "AntiDebug", module.CorLibTypes.Object.TypeRef);
            module.Types.Add(type);
            var method = new MethodDefUser("CheckDebug", MethodSig.CreateStatic(module.CorLibTypes.Void),
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig);
            type.Methods.Add(method);
            var body = new CilBody();
            var instrs = body.Instructions;

            var getIsAttachedMethod = typeof(System.Diagnostics.Debugger).GetProperty("IsAttached").GetGetMethod();
            var importedGetIsAttached = module.Import(getIsAttachedMethod);

            instrs.Add(OpCodes.Call.ToInstruction(importedGetIsAttached));
            var labelNoDebug = Instruction.Create(OpCodes.Nop);
            instrs.Add(OpCodes.Brfalse_S.ToInstruction(labelNoDebug));
            var ctor = typeof(System.Exception).GetConstructor(new[] { typeof(string) });
            var importedCtor = module.Import(ctor);
            instrs.Add(OpCodes.Ldstr.ToInstruction("Debugging detected!"));
            instrs.Add(OpCodes.Newobj.ToInstruction(importedCtor));
            instrs.Add(OpCodes.Throw.ToInstruction());
            instrs.Add(labelNoDebug);
            instrs.Add(OpCodes.Ret.ToInstruction());
            method.Body = body;
        }
    }


    namespace Protections
    {
        internal class ImportProtection
        {
            public static FieldDefUser CreateField(FieldSig sig)
            {
                return new FieldDefUser(Renamer.GenerateName(), sig, FieldAttributes.Public | FieldAttributes.Static);
            }

            public static void Execute(Context ctx)
            {
                var module = ctx.moduleDef;
                var brigdes = new Dictionary<IMethod, MethodDef>();
                var methods = new Dictionary<IMethod, TypeDef>();
                var field = CreateField(new FieldSig(module.ImportAsTypeSig(typeof(object[]))));
                var cctor = ctx.typeDef.FindOrCreateStaticConstructor();
                foreach (TypeDef type in module.GetTypes().ToArray())
                {
                    if (type.IsDelegate)
                        continue;
                    if (type.IsGlobalModuleType)
                        continue;
                    if (type.Namespace == "Costura")
                        continue;
                    foreach (MethodDef method in type.Methods.ToArray())
                    {
                        if (!method.HasBody)
                            continue;
                        if (!method.Body.HasInstructions)
                            continue;
                        if (method.IsConstructor)
                            continue;

                        var instrs = method.Body.Instructions;

                        for (int i = 0; i < instrs.Count; i++)
                        {
                            if (instrs[i].OpCode != OpCodes.Call && instrs[i].OpCode == OpCodes.Callvirt)
                                continue;
                            if (instrs[i].Operand is IMethod idef)
                            {
                                if (!idef.IsMethodDef)
                                    continue;

                                var def = idef.ResolveMethodDef();

                                if (def == null)
                                    continue;
                                if (def.HasThis)
                                    continue;

                                if (brigdes.ContainsKey(idef))
                                {
                                    instrs[i].OpCode = OpCodes.Call;
                                    instrs[i].Operand = brigdes[idef];
                                    continue;
                                }

                                var sig = CreateProxySignature(module, def);
                                var delegateType = CreateDelegateType(module, sig);
                                module.Types.Add(delegateType);

                                var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                                var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                                var brigde = new MethodDefUser(Renamer.GenerateName(), sig, methImplFlags, methFlags);
                                brigde.Body = new CilBody();

                                brigde.Body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(field));
                                brigde.Body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(methods.Count));
                                brigde.Body.Instructions.Add(OpCodes.Ldelem_Ref.ToInstruction());
                                foreach (var parameter in brigde.Parameters)
                                {
                                    parameter.Name = Renamer.GenerateName();
                                    brigde.Body.Instructions.Add(OpCodes.Ldarg.ToInstruction(parameter));
                                }
                                brigde.Body.Instructions.Add(OpCodes.Call.ToInstruction(delegateType.Methods[1]));
                                brigde.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

                                delegateType.Methods.Add(brigde);

                                instrs[i].OpCode = OpCodes.Call;
                                instrs[i].Operand = brigde;

                                if (idef.IsMethodDef)
                                    methods.Add(def, delegateType);
                                else if (idef.IsMemberRef)
                                    methods.Add(idef as MemberRef, delegateType);

                                brigdes.Add(idef, brigde);
                            }
                        }
                    }
                }

                module.GlobalType.Fields.Add(field);

                var instructions = new List<Instruction>();
                var current = cctor.Body.Instructions.ToList();
                cctor.Body.Instructions.Clear();

                instructions.Add(OpCodes.Ldc_I4.ToInstruction(methods.Count));
                instructions.Add(OpCodes.Newarr.ToInstruction(module.CorLibTypes.Object));
                instructions.Add(OpCodes.Dup.ToInstruction());

                var index = 0;

                foreach (var entry in methods)
                {
                    instructions.Add(OpCodes.Ldc_I4.ToInstruction(index));
                    instructions.Add(OpCodes.Ldnull.ToInstruction());
                    instructions.Add(OpCodes.Ldftn.ToInstruction(entry.Key));
                    instructions.Add(OpCodes.Newobj.ToInstruction(entry.Value.Methods[0]));
                    instructions.Add(OpCodes.Stelem_Ref.ToInstruction());
                    instructions.Add(OpCodes.Dup.ToInstruction());
                    index++;
                }

                instructions.Add(OpCodes.Pop.ToInstruction());
                instructions.Add(OpCodes.Stsfld.ToInstruction(field));

                foreach (var instr in instructions)
                    cctor.Body.Instructions.Add(instr);
                foreach (var instr in current)
                    cctor.Body.Instructions.Add(instr);
            }

            public static TypeDef CreateDelegateType(ModuleDef module, MethodSig sig)
            {
                var ret = new TypeDefUser(Renamer.GenerateName(), module.CorLibTypes.GetTypeRef("System", "MulticastDelegate"));
                ret.Attributes = TypeAttributes.Public | TypeAttributes.Sealed;

                var ctor = new MethodDefUser(".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object, module.CorLibTypes.IntPtr));
                ctor.Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
                ctor.ImplAttributes = MethodImplAttributes.Runtime;
                ret.Methods.Add(ctor);

                var invoke = new MethodDefUser("Invoke", sig.Clone());
                invoke.MethodSig.HasThis = true;
                invoke.Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
                invoke.ImplAttributes = MethodImplAttributes.Runtime;
                ret.Methods.Add(invoke);

                return ret;
            }

            public static MethodSig CreateProxySignature(ModuleDef module, IMethod method)
            {
                IEnumerable<TypeSig> paramTypes = method.MethodSig.Params.Select(type =>
                {
                    if (type.IsClassSig && method.MethodSig.HasThis)
                        return module.CorLibTypes.Object;
                    return type;
                });
                if (method.MethodSig.HasThis && !method.MethodSig.ExplicitThis)
                {
                    TypeDef declType = method.DeclaringType.ResolveTypeDefThrow();
                    if (!declType.IsValueType)
                        paramTypes = new[] { module.CorLibTypes.Object }.Concat(paramTypes);
                    else
                        paramTypes = new[] { declType.ToTypeSig() }.Concat(paramTypes);
                }
                TypeSig retType = method.MethodSig.RetType;
                if (retType.IsClassSig)
                    retType = module.CorLibTypes.Object;
                return MethodSig.CreateStatic(retType, paramTypes.ToArray());
            }
        }
    }


    internal class ConstMelting
    {
        public static void Execute(Context context)
        {
            foreach (TypeDef type in context.moduleDef.Types.ToArray())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    ReplaceStringLiterals(method);
                    ReplaceIntLiterals(method);
                }
            }
        }

        private static void ReplaceStringLiterals(MethodDef methodDef)
        {
            if (!CanObfuscate(methodDef)) return;
            foreach (Instruction instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Ldstr) continue;
                MethodDef replacementMethod = new MethodDefUser(
                    Renamer.GenerateName(),
                    MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.String),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig
                )
                {
                    Body = new CilBody()
                };
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldstr, instruction.Operand.ToString()));
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                methodDef.DeclaringType.Methods.Add(replacementMethod);
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = replacementMethod;
            }
        }

        private static void ReplaceIntLiterals(MethodDef methodDef)
        {
            if (!CanObfuscate(methodDef)) return;
            foreach (Instruction instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Ldc_I4) continue;
                MethodDef replacementMethod = new MethodDefUser(
                    Renamer.GenerateName(),
                    MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.Int32),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig
                )
                {
                    Body = new CilBody()
                };
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, instruction.GetLdcI4Value()));
                replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                methodDef.DeclaringType.Methods.Add(replacementMethod);
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = replacementMethod;
            }
        }

        public static bool CanObfuscate(MethodDef methodDef)
        {
            if (!methodDef.HasBody) return false;
            if (!methodDef.Body.HasInstructions) return false;
            if (methodDef.DeclaringType.IsGlobalModuleType) return false;
            return true;
        }
    }

   
    internal class IntV2
    {
        public static void Execute(Context ctx)
        {
            int Amount = 0;

            IMethod absMethod = ctx.moduleDef.Import(typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }));
            IMethod minMethod = ctx.moduleDef.Import(typeof(Math).GetMethod("Min", new Type[] { typeof(int), typeof(int) }));

            foreach (TypeDef type in ctx.moduleDef.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].IsLdcI4())
                        {
                            int operand = method.Body.Instructions[i].GetLdcI4Value();
                            if (operand <= 0)
                                continue;

                            method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(absMethod));
                            int negCount = Next(StringLength(), 8);
                            if (negCount % 2 != 0) negCount += 1;

                            for (var j = 0; j < negCount; j++)
                                method.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));

                            if (operand < int.MaxValue)
                            {
                                method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
                                method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(minMethod));
                            }

                            ++Amount;
                        }
                }
            }
        }

        private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();

        public static string String(int size)
        {
            return Encoding.UTF7.GetString(RandomBytes(size))
                .Replace("\0", ".")
                .Replace("\n", ".")
                .Replace("\r", ".");
        }

        public static int Next()
        {
            return BitConverter.ToInt32(RandomBytes(sizeof(int)), 0);
        }

        private static uint RandomUInt()
        {
            return BitConverter.ToUInt32(RandomBytes(sizeof(uint)), 0);
        }

        private static byte[] RandomBytes(int bytes)
        {
            byte[] buffer = new byte[bytes];
            csp.GetBytes(buffer);
            return buffer;
        }

        public static int Next(int maxValue, int minValue = 0)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            long diff = (long)maxValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;
            uint ui;
            do { ui = RandomUInt(); } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        public static int StringLength()
        {
            return Next(120, 30);
        }
    }
}