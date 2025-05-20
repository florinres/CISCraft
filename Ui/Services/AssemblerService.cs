using MainMemory.Business;
using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;
    private readonly IMainMemory _mainMemory;
    public event EventHandler<byte[]>? SourceCodeAssembled;

    public AssemblerService(ASMBLR assembler, IMainMemory memory)
    {
        _assembler = assembler;
        _mainMemory = memory;
    }

    public byte[] AssembleSourceCodeService(string sourceCode)
    {
        var objectCode = _assembler.Assemble(sourceCode, out _);
        _mainMemory.LoadMachineCode(objectCode);
        
        SourceCodeAssembled?.Invoke(this, objectCode);
        
        return objectCode;
    }
}