using Assembler.Business;

namespace Ui.Interfaces.Services;


public interface IAssemblerService
{
    event EventHandler<byte[]> SourceCodeAssembled;
    
    /// <summary>
    /// Event fired when assembly errors occur
    /// </summary>
    event EventHandler<IReadOnlyList<AssemblyError>> AssemblyErrorsReported;
    
    /// <summary>
    /// Assembles source code and returns debug symbols
    /// </summary>
    /// <param name="sourceCode">The source code to assemble</param>
    /// <param name="sectionOffset">The section offset address</param>
    /// <returns>Debug symbols dictionary or null if assembly failed</returns>
    Dictionary<ushort, ushort> AssembleSourceCodeService(string sourceCode, ushort sectionOffset);
}