using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Generates an Assembly from a script filename
    /// </summary>
    public class AssemblyFactory
    {
        private CompilerErrorCollection _compilerErrors;

        /// <summary>
        /// Generates an Assembly from a script filename
        /// </summary>
        /// <param name="filename">The filename of the script</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(string filename)
        {
            return CreateAssembly(filename, new ArrayList());
        }

        /// <summary>
        /// Generates an Assembly from a script filename
        /// </summary>
        /// <param name="filename">The filename of the script</param>
        /// <param name="references">Assembly references for the script</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(string filename, IList references)
        {
            // ensure that compilerErrors is null
            _compilerErrors = null;

            string extension = Path.GetExtension(filename);

            // Select the correct CodeDomProvider based on script file extension
            CodeDomProvider codeProvider;
            switch (extension)
            {
                case ".cs":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case ".vb":
                    codeProvider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case ".js":
                    codeProvider = new Microsoft.JScript.JScriptCodeProvider();
                    break;
                default:
                    throw new InvalidOperationException("Script files must have a .cs, .vb, or .js extension, for C#, Visual Basic.NET, or JScript respectively.");
            }

            //ICodeCompiler compiler = codeProvider.CreateCompiler();

            // Set compiler parameters
            var compilerParams = new CompilerParameters { CompilerOptions = "/target:library /optimize", GenerateExecutable = false, GenerateInMemory = true, IncludeDebugInformation = false };

            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");

            // Add custom references
            foreach (string reference in references.Cast<string>().Where(reference => !compilerParams.ReferencedAssemblies.Contains(reference)))
            {
                compilerParams.ReferencedAssemblies.Add(reference);
            }

            // Do the compilation
            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParams, filename);

            //Do we have any compiler errors
            if (results.Errors.Count > 0)
            {
                _compilerErrors = results.Errors;
                throw new Exception(
                    "Compiler error(s) encountered and saved to AssemblyFactory.CompilerErrors");
            }

            Assembly createdAssembly = results.CompiledAssembly;
            return createdAssembly;
        }

        /// <summary>
        /// Generates an Assembly from a list of script fileNames
        /// </summary>
        /// <param name="fileNames">The fileNames of the scripts</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(IList fileNames)
        {
            return CreateAssembly(fileNames, new ArrayList());
        }

        /// <summary>
        /// Generates an Assembly from a list of script fileNames
        /// </summary>
        /// <param name="fileNames">The fileNames of the scripts</param>
        /// <param name="references">Assembly references for the script</param>
        /// <returns>The generated assembly</returns>C
        public Assembly CreateAssembly(IList fileNames, IList references)
        {
            string fileType = null;
            foreach (var extension in fileNames.Cast<string>().Select(Path.GetExtension))
            {
                if (fileType == null)
                {
                    fileType = extension;
                }
                else if (fileType != extension)
                {
                    throw new ArgumentException("All files in the file list must be of the same type.");
                }
            }

            // ensure that compilerErrors is null
            _compilerErrors = null;

            // Select the correct CodeDomProvider based on script file extension
            CodeDomProvider codeProvider;
            switch (fileType)
            {
                case ".cs":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case ".vb":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case ".js":
                    codeProvider = new Microsoft.JScript.JScriptCodeProvider();
                    break;
                default:
                    throw new InvalidOperationException("Script files must have a .cs, .vb, or .js extension, for C#, Visual Basic.NET, or JScript respectively.");
            }

            // Set compiler parameters
            var compilerParams = new CompilerParameters { CompilerOptions = "/target:library /optimize", GenerateExecutable = false, GenerateInMemory = true, IncludeDebugInformation = false };

            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");

            // Add custom references
            foreach (string reference in references.Cast<string>().Where(reference => !compilerParams.ReferencedAssemblies.Contains(reference)))
            {
                compilerParams.ReferencedAssemblies.Add(reference);
            }

            // Do the compilation
            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParams, (string[])ArrayList.Adapter(fileNames).ToArray(typeof(string)));

            // Do we have any compiler errors
            if (results.Errors.Count > 0)
            {
                _compilerErrors = results.Errors;
                throw new Exception(
                    "Compiler error(s) encountered and saved to AssemblyFactory.CompilerErrors");
            }

            Assembly createdAssembly = results.CompiledAssembly;
            return createdAssembly;
        }

        /// <summary>
        /// The compiler errors for the last generated assembly.  Null if no compile errors.
        /// </summary>
        public CompilerErrorCollection CompilerErrors
        {
            get
            {
                return _compilerErrors;
            }
        }
    }
}
