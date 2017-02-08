﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cauldron.Interception.Fody
{
    internal static class Extensions
    {
        private static ModuleDefinition _moduleDefinition;
        private static IEnumerable<AssemblyDefinition> allAssemblies;
        private static IEnumerable<TypeDefinition> allTypes;

        public static ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
            set
            {
                _moduleDefinition = value;
                allAssemblies = value.AssemblyReferences.GetAll().Select(x => value.AssemblyResolver.Resolve(x)).ToArray();
                allTypes = allAssemblies.SelectMany(x => x.Modules).Where(x => x != null).SelectMany(x => x.Types).Where(x => x != null).ToArray();
            }
        }

        public static IEnumerable<AssemblyDefinition> AllReferencedAssemblies(this ModuleDefinition target) => allAssemblies;

        public static void Append(this ILProcessor processor, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                processor.Append(instruction);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="IEnumerable"/>
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable"/></param>
        /// <returns>The total count of items in the <see cref="IEnumerable"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
        public static int Count_(this IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int count = 0;

            if (source.GetType().IsArray)
                return (source as Array).Length;

            var collection = source as ICollection;
            if (collection != null)
                return collection.Count;

            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
                count++;

            (enumerator as IDisposable)?.Dispose();

            return count;
        }

        public static IEnumerable<AssemblyNameReference> GetAll(this IEnumerable<AssemblyNameReference> target)
        {
            var result = new List<AssemblyNameReference>();
            result.AddRange(target);

            foreach (var item in target)
            {
                var assembly = ModuleDefinition.AssemblyResolver.Resolve(item);

                if (assembly == null)
                    continue;

                if (assembly.MainModule.HasAssemblyReferences)
                    result.AddRange(assembly.MainModule.AssemblyReferences);
            }

            return result.Distinct(new AssemblyNameReferenceEqualityComparer()).OrderBy(x => x.FullName);
        }

        public static MethodReference GetMethodReference(this Type type, string methodName, Type[] parameterTypes)
        {
            var definition = type.GetTypeDefinition();
            var result = definition.Methods.FirstOrDefault(x => x.Name == methodName && parameterTypes.Select(y => y.FullName).SequenceEqual(x.Parameters.Select(y => y.ParameterType.FullName)));

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{type.FullName}' does not contain a method '{methodName}'");
        }

        public static MethodReference GetMethodReference(this TypeReference typeReference, string methodName, int parameterCount)
        {
            var definition = typeReference.Resolve();
            var result = definition.Methods.FirstOrDefault(x => x.Name == methodName && x.Parameters.Count == parameterCount);

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{typeReference.FullName}' does not contain a method '{methodName}'");
        }

        public static MethodReference GetMethodReference(this Type type, string methodName, int parameterCount)
        {
            var definition = type.GetTypeDefinition();
            var result = definition.Methods.FirstOrDefault(x => x.Name == methodName && x.Parameters.Count == parameterCount);

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{type.FullName}' does not contain a method '{methodName}'");
        }

        public static PropertyDefinition GetPropertyDefinition(this MethodDefinition method) =>
                                                    method.DeclaringType.Properties.FirstOrDefault(x => x.GetMethod == method || x.SetMethod == method);

        /// <summary>
        /// Gets the stacktrace of the exception and the inner exceptions recursively
        /// </summary>
        /// <param name="e">The exception with the stack trace</param>
        /// <returns>A string representation of the stacktrace</returns>
        public static string GetStackTrace(this Exception e)
        {
            var sb = new StringBuilder();
            var ex = e;

            do
            {
                sb.AppendLine("Exception Type: " + ex.GetType().Name);
                sb.AppendLine("Source: " + ex.Source);
                sb.AppendLine(ex.Message);
                sb.AppendLine("------------------------");
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine("------------------------");

                ex = ex.InnerException;
            } while (ex != null);

            return sb.ToString();
        }

        public static TypeDefinition GetTypeDefinition(this Type type)
        {
            var result = allTypes.FirstOrDefault(x => x.FullName == type.FullName);

            if (result == null)
                throw new Exception($"Unable to proceed. The type '{type.FullName}' was not found.");

            return result;
        }

        public static TypeReference GetTypeReference(this Type type)
        {
            var result = allTypes.FirstOrDefault(x => x.FullName == type.FullName);

            if (result == null)
                throw new Exception($"Unable to proceed. The type '{type.FullName}' was not found.");

            return result;
        }

        public static IEnumerable<TypeDefinition> GetTypesThatImplementsInterface(this TypeDefinition typeDefinitionOfInterface) =>
             allTypes.Where(x => x.Implements(typeDefinitionOfInterface.Name)).ToArray();

        public static bool Implements(this TypeDefinition typeDefinition, string interfaceName)
        {
            while (typeDefinition != null)
            {
                if (typeDefinition.Interfaces != null && typeDefinition.Interfaces.Any(x => x.Name == interfaceName))
                    return true;

                typeDefinition = typeDefinition.BaseType?.Resolve();
            }

            return false;
        }

        public static TypeReference Import(this TypeReference value) => ModuleDefinition.Import(value);

        public static MethodReference Import(this System.Reflection.MethodBase value) => ModuleDefinition.Import(value);

        public static TypeReference Import(this Type value) => ModuleDefinition.Import(value);

        public static MethodReference Import(this MethodReference value) => ModuleDefinition.Import(value);

        public static FieldReference Import(this FieldReference value) => ModuleDefinition.Import(value);

        public static void InsertBefore(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                processor.InsertBefore(target, instruction);
        }

        public static bool IsAttribute(this TypeDefinition typeDefinition)
        {
            typeDefinition = typeDefinition.BaseType?.Resolve();

            while (typeDefinition != null)
            {
                if (typeDefinition.FullName == "System.Attribute")
                    return true;

                typeDefinition = typeDefinition.BaseType?.Resolve();
            }

            return false;
        }

        public static TypeDefinition Resolve(this string typeName) => allTypes.FirstOrDefault(x => x.FullName == typeName || x.Name == typeName);

        /// <summary>
        /// Converts a <see cref="IEnumerable"/> to an array
        /// </summary>
        /// <typeparam name="T">The type of elements the <see cref="IEnumerable"/> contains</typeparam>
        /// <param name="items">The <see cref="IEnumerable"/> to convert</param>
        /// <returns>An array of <typeparamref name="T"/></returns>
        public static T[] ToArray_<T>(this IEnumerable items)
        {
            if (items == null)
                return new T[0];

            T[] result = new T[items.Count_()];
            int counter = 0;

            foreach (T item in items)
            {
                result[counter] = item;
                counter++;
            }

            return result;
        }

        public static TypeDefinition ToTypeDefinition(this string typeName) => allTypes.FirstOrDefault(x => x.FullName == typeName || x.FullName.EndsWith(typeName));

        public static IEnumerable<Instruction> TypeOf(this ILProcessor processor, TypeReference type)
        {
            return new Instruction[] {
                processor.Create(OpCodes.Ldtoken, type),
                processor.Create(OpCodes.Call, ModuleDefinition.Import(typeof(Type).GetMethodReference("GetTypeFromHandle", 1)))
            };
        }
    }
}