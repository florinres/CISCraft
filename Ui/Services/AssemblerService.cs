using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;
    
    public event EventHandler<byte[]>? SourceCodeAssembled;

    public AssemblerService(ASMBLR assembler)
    {
        _assembler = assembler;
    }

    public byte[] AssembleSourceCodeService(string sourceCode)
    {
        var objectCode = _assembler.Assemble(sourceCode, out _);
        
        SourceCodeAssembled?.Invoke(this, objectCode);
        
        return objectCode;
    }
}