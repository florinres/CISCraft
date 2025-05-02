using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;

    public AssemblerService(ASMBLR assembler)
    {
        _assembler = assembler;
    }

    public byte[] AssembleSourceCodeService(string sourceCode)
    {
        var len = 0;
        var objectCode = _assembler.Assemble(sourceCode, out len);
        return objectCode;
    }
}