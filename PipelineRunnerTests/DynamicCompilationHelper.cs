// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicCompilationHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The dynamic compilation helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CSharp;

    /// <summary>
    /// The dynamic compilation helper.
    /// </summary>
    public class DynamicCompilationHelper
    {
        /// <summary>
        /// The compile source code dom.
        /// from: https://benohead.com/three-options-to-dynamically-execute-csharp-code/
        /// </summary>
        /// <param name="sourceCode">
        /// The source code.
        /// </param>
        /// <returns>
        /// The <see cref="Assembly"/>.
        /// </returns>
        public static Assembly CompileSourceCodeDom(string sourceCode)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.GenerateExecutable = false;
            CompilerResults cr = cpd.CompileAssemblyFromSource(cp, sourceCode);

            return cr.CompiledAssembly;
        }

        /// <summary>
        /// The compile source roslyn.
        /// from: http://www.tugberkugurlu.com/archive/compiling-c-sharp-code-into-memory-and-executing-it-with-roslyn
        /// </summary>
        /// <param name="fooSource">
        /// The foo source.
        /// </param>
        /// <returns>
        /// The <see cref="Assembly"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        public static Assembly CompileSourceRoslyn(string fooSource)
        {
            using (var ms = new MemoryStream())
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(fooSource);

                CSharpCompilation compilation = CSharpCompilation.Create(
                    "assemblyName",
                    new[] { syntaxTree },
                    new[]
                        {
                            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
                        },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    return assembly;
                }
            }
        }

        /// <summary>
        /// The execute from assembly.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        public static void ExecuteFromAssembly(Assembly assembly)
        {
            Type fooType = assembly.GetType("Foo");
            MethodInfo printMethod = fooType.GetMethod("Print");
            object foo = assembly.CreateInstance("Foo");
            printMethod.Invoke(foo, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// The executre from roslyn assembly.
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        public static void ExecutreFromRoslynAssembly(Assembly assembly)
        {
            Type type = assembly.GetType("RoslynCompileSample.Writer");
            object obj = Activator.CreateInstance(type);
            type.InvokeMember(
                "Write",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                obj,
                new object[] { "Hello World" });
        }

    }
}