using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class ModuleWeaver : BaseModuleWeaver
{



    FieldDefinition ifobj;
    MethodDefinition _methodcall;
    public ModuleWeaver()
    {

    }
    public override void Execute()
    {
              

        var allinterface = GetBuildInterfaces();

        foreach (var iface in allinterface)
        {
            var newType = new TypeDefinition(iface.Namespace, iface.Name + "_Builder_Implementation", TypeAttributes.Public| TypeAttributes.BeforeFieldInit, TypeSystem.ObjectReference);
            newType.Interfaces.Add(new InterfaceImplementation(iface));
            // ifobj = new FieldDefinition("ifobj", FieldAttributes.Private, ModuleDefinition.ImportReference(typeof(IFodyCall)));
            ifobj = new FieldDefinition("ifobj", FieldAttributes.Private, ModuleDefinition.ImportReference(typeof(Func<int, Type, System.Object[], System.Object>)));
            newType.Fields.Add(ifobj);
            AddConstructor(newType);
            AddMethodForCall(newType);
            var allRpc = iface.GetMethods().Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG") != null);

            if (allRpc.FirstOrDefault(p => p.ContainsGenericParameter) != null)
            {
                LogInfo($"not make build the '{iface.FullName}',is have generic method.");
                continue;
            }

            foreach (var rpc in allRpc)
            {
                AddRpc(newType, rpc);
            }

            ModuleDefinition.Types.Add(newType);

            LogInfo($"Added Packer Type '{newType.FullName}' with Interface '{iface.FullName}'.");
        }


    }

    TypeDefinition[] GetBuildInterfaces()
    {
        return ModuleDefinition.GetAllTypes().Where(p => p.IsInterface && p.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "Build") != null).ToArray();
    }



    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }


    void AddConstructor(TypeDefinition newType)
    {
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName|MethodAttributes.HideBySig, TypeSystem.VoidReference);
        var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
        //method.Parameters.Add(new ParameterDefinition(ModuleDefinition.ImportReference(typeof(IFodyCall))));
        method.Parameters.Add(new ParameterDefinition(ModuleDefinition.ImportReference(typeof(Func<int,Type,System.Object[],System.Object>))));
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Nop);
        processor.Emit(OpCodes.Nop);
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Stfld, ifobj);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    void AddMethodForCall(TypeDefinition newType)
    {
        _methodcall = new MethodDefinition("_call_", MethodAttributes.Private | MethodAttributes.HideBySig, TypeSystem.VoidReference);

        _methodcall.Parameters.Add(new ParameterDefinition(ModuleDefinition.ImportReference(typeof(int))));
        _methodcall.Parameters.Add(new ParameterDefinition(ModuleDefinition.ImportReference(typeof(Type))));
        _methodcall.Parameters.Add(new ParameterDefinition(ModuleDefinition.ImportReference(typeof(System.Object[]))));

        var objs = new VariableDefinition(ModuleDefinition.ImportReference(typeof(System.Object)));

        var processor = _methodcall.Body.GetILProcessor();
        processor.Body.Variables.Add(objs);
        processor.Emit(OpCodes.Nop);
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Ldfld, ifobj);
        processor.Emit(OpCodes.Ldarg_1);
        processor.Emit(OpCodes.Ldarg_2);
        processor.Emit(OpCodes.Ldarg_3);
        //processor.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(ifobj.FieldType.Resolve().Methods.First(p => p.Name == "Call")));
        processor.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(MakeGeneric(ifobj.FieldType.Resolve().Methods.First(p => p.Name == "Invoke"), ModuleDefinition.ImportReference(typeof(int)), ModuleDefinition.ImportReference(typeof(Type)), ModuleDefinition.ImportReference(typeof(object[])), ModuleDefinition.ImportReference(typeof(object)))));
        processor.Emit(OpCodes.Stloc, objs);
        processor.Emit(OpCodes.Ldloc, objs);
        processor.Emit(OpCodes.Ret);

        _methodcall.ReturnType = ModuleDefinition.ImportReference(typeof(System.Object));
        newType.Methods.Add(_methodcall);
    }

    public MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments)
    {
        var reference = new MethodReference(self.Name, self.ReturnType)
        {
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            DeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments),
            CallingConvention = self.CallingConvention,
        };

        foreach (var parameter in self.Parameters)
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

        foreach (var genericParameter in self.GenericParameters)
            reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));

        return reference;
    }


    void AddRpc(TypeDefinition newType, MethodDefinition irpc)
    {

        var tag = irpc.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "TAG");

        if (tag is null)
            return;

        var cmd = (int)tag.ConstructorArguments.First().Value;

        var method = new MethodDefinition(irpc.Name, MethodAttributes.Public | MethodAttributes.HideBySig| MethodAttributes.NewSlot|MethodAttributes.Virtual|MethodAttributes.Final, irpc.ReturnType);

        var il = method.Body.GetILProcessor();
       
        var parameters = irpc.Parameters;
        var paramTypes = ParamTypes(parameters.ToArray(), false);
        foreach (var param in paramTypes)
        {
            method.Parameters.Add(new ParameterDefinition(param));
        }

        if (irpc.ContainsGenericParameter)
        {
            throw new Exception($"not have generic parameter{irpc.FullName}");
            //var ts = irpc.GenericParameters;           
            //for (int i = 0; i < ts.Count; i++)
            //{
            //    method.GenericParameters.Add(ts[i]);          
            //}           
        }

        ParametersArray args = new ParametersArray(this, il, paramTypes);

        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, cmd);
        il.Emit(OpCodes.Ldtoken, irpc.ReturnType);

        var ptype = ModuleDefinition.ImportReference(GetMethodInfo.GetTypeofHandler());
        il.Emit(OpCodes.Call, ModuleDefinition.ImportReference(ptype));


        GenericArray<System.Object> argsArr = new GenericArray<System.Object>(this, il, ParamTypes(parameters.ToArray(), true).Length);

        for (int i = 0; i < parameters.Count; i++)
        {
            // args[i] = argi;
            if (!parameters[i].IsOut)
            {
                argsArr.BeginSet(i);
                args.Get(i);
                argsArr.EndSet(parameters[i].ParameterType);
            }
        }
        argsArr.Load();

        il.Emit(OpCodes.Call, _methodcall);

        if (irpc.ReturnType.Name == "Void")
        {
            il.Emit(OpCodes.Pop);
        }
        else
        {
            var res = new VariableDefinition(irpc.ReturnType);
            method.Body.Variables.Add(res);
            Convert(il, ModuleDefinition.ImportReference(typeof(System.Object)), irpc.ReturnType, false);
            il.Emit(OpCodes.Stloc,res);
            il.Emit(OpCodes.Ldloc,res);

           
        }
        il.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    private static TypeReference[] ParamTypes(ParameterDefinition[] parms, bool noByRef)
    {
        TypeReference[] types = new TypeReference[parms.Length];
        for (int i = 0; i < parms.Length; i++)
        {
            types[i] = parms[i].ParameterType;
            if (noByRef && types[i].IsByReference)
                types[i] = types[i].GetElementType();
        }
        return types;
    }


    public override bool ShouldCleanReference => true;
}

