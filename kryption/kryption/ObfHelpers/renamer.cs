using dnlib.DotNet;
using dnlib.DotNet.Emit;
using kryption;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace kryption.ObfHelpers
{
    internal class Renamer
    {
        public static int SpamCount
        {
            get
            {
                return 4;
            }
        }
        public static string InvisibleName
        {
            get
            {
                return GenerateName();
            }
        }

        private static readonly string[] WordList = new string[] {
            "AccessModifier",
            "ArrayType",
            "Assembly",
            "Attribute",
            "Boolean",
            "Byte",
            "Callback",
            "Char",
            "Checked",
            "Class",
            "Collection",
            "Compilation",
            "Constructor",
            "Delegate",
            "Derived",
            "Disposable",
            "Double",
            "Dynamic",
            "Enum",
            "Equals",
            "Event",
            "Exception",
            "Execute",
            "False",
            "Field",
            "Float",
            "Generic",
            "Hashtable",
            "Identity",
            "Implement",
            "Indexer",
            "Inherit",
            "Initialize",
            "Instance",
            "Interface",
            "Invalid",
            "Iterator",
            "KeyValuePair",
            "LinkedList",
            "List",
            "Literal",
            "Lock",
            "Long",
            "Managed",
            "Member",
            "Metadata",
            "Method",
            "Module",
            "Multicast",
            "Namespace",
            "Nested",
            "Nullable",
            "Object",
            "Operator",
            "Overload",
            "Override",
            "Parameter",
            "Parse",
            "Partial",
            "Platform",
            "Pointer",
            "Predicate",
            "Private",
            "Property",
            "Protected",
            "Public",
            "Query",
            "Random",
            "Readonly",
            "Refactoring",
            "Reflection",
            "Register",
            "Release",
            "Remove",
            "SafeHandle",
            "Scalar",
            "Sealed",
            "Section",
            "Serial",
            "Serialize",
            "Short",
            "Signed",
            "Single",
            "SizeOf",
            "Stack",
            "Static",
            "String",
            "Struct",
            "Subclass",
            "Subroutine",
            "Switch",
            "Synchronized",
            "Syntax",
            "Thread",
            "Throw",
            "True",
            "Type",
            "Typecasting",
            "Unbox",
            "Unicode",
            "Unchecked",
            "Unit",
            "Unsafe",
            "Unwrap",
            "Ushort",
            "Using",
            "Value",
            "Variable",
            "Variant",
            "Virtual",
            "Volatile",
            "WebClient",
            "While",
            "Xor",
            "Yield",
            "Zone"
        };
        public static string GenerateName()
        {
            StringBuilder nameBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                int index = rnd.Next(WordList.Length);
                string word = WordList[index];
                nameBuilder.Append(word);
            }
            return nameBuilder.ToString();
        }

        private static Random rnd = new Random();
        public static void Execute(Context ctx)
        {
            string jName = null;

   
            ModuleDef module = ctx.module;

            foreach (TypeDef typeDef in module.Types)
            {
                if (typeDef.IsPublic)
                    jName = typeDef.Name;

                if (CanRename(typeDef))
                {
                    foreach (MethodDef methodDef in typeDef.Methods)
                    {
                        if (CanRename(methodDef))
                        {
                            TypeRef typeRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
                            MemberRefUser ctor = new MemberRefUser(
                                module,
                                ".ctor",
                                MethodSig.CreateInstance(module.Import(typeof(void)).ToTypeSig(true)),
                                typeRef
                            );
                            CustomAttribute item = new CustomAttribute(ctor);
                            methodDef.CustomAttributes.Add(item);
                            methodDef.Name = InvisibleName;
                        }

                        foreach (Parameter parameter in methodDef.Parameters)
                        {
                            parameter.Name = InvisibleName;
                        }
                    }
                }

                foreach (FieldDef fieldDef in typeDef.Fields)
                {
                    if (CanRename(fieldDef))
                        fieldDef.Name = InvisibleName;
                }

                foreach (EventDef eventDef in typeDef.Events)
                {
                    if (CanRename(eventDef))
                        eventDef.Name = InvisibleName;
                }

                if (typeDef.IsPublic && !string.IsNullOrEmpty(jName))
                {
                    foreach (Resource resource in module.Resources)
                    {
                        if (resource.Name.Contains(jName))
                        {
                            resource.Name = resource.Name.Replace(jName, typeDef.Name);
                        }
                    }
                }
            }
        }


        private static bool CanRename(TypeDef type)
        {
            if (type.IsGlobalModuleType)
                return false;

            try
            {
                if (type.Name.Contains("My"))
                    return false;
            }
            catch (Exception ex)
            {

            }

            if (type.Interfaces.Count > 0)
                return false;

            if (type.IsSpecialName)
                return false;

            if (type.IsRuntimeSpecialName)
                return false;

            bool isSat = type.Name.Contains("Sat");
            return !isSat;


        }

        private static bool CanRename(EventDef ev)
        {
            bool isForwarder = ev.DeclaringType.IsForwarder;
            bool result;
            if (isForwarder)
            {
                result = false;
            }
            else
            {
                bool isRuntimeSpecialName = ev.IsRuntimeSpecialName;
                result = !isRuntimeSpecialName;
            }
            return result;
        }
        private static bool CanRename(FieldDef field)
        {
            bool flag = field.IsLiteral && field.DeclaringType.IsEnum;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool isForwarder = field.DeclaringType.IsForwarder;
                if (isForwarder)
                {
                    result = false;
                }
                else
                {
                    bool isRuntimeSpecialName = field.IsRuntimeSpecialName;
                    if (isRuntimeSpecialName)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag2 = field.IsLiteral && field.DeclaringType.IsEnum;
                        if (flag2)
                        {
                            result = false;
                        }
                        else
                        {
                            bool flag3 = field.Name.Contains("Sugar");
                            result = !flag3;
                        }
                    }
                }
            }
            return result;
        }


        private static bool CanRename(MethodDef method)
        {
            if (method.IsConstructor)
                return false;
            if (method.DeclaringType.IsForwarder)
                return false;
            if (method.IsFamily)
                return false;
            if (method.IsConstructor || method.IsStaticConstructor)
                return false;
            if (method.IsRuntimeSpecialName)
                return false;
            if (method.DeclaringType.IsGlobalModuleType)
                return false;

            return true;
        }
    }
}