// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicCompilationTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DynamicCompilationTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The dynamic compilation tests.
    /// </summary>
    [TestClass]
    public class DynamicCompilationTests
    {
        /// <summary>
        /// The test method 1.
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            string source = @"
    using System;

    namespace RoslynCompileSample
    {
        public class Writer
        {
            public void Write(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
            ";
            var assembly = DynamicCompilationHelper.CompileSourceRoslyn(source);
            DynamicCompilationHelper.ExecutreFromRoslynAssembly(assembly);
            
        }
    }
}
