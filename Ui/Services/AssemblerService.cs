using MainMemory.Business;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;
    private readonly IMainMemory _mainMemory;
    public event EventHandler<byte[]>? SourceCodeAssembled;
    public Dictionary<short, ushort> DebugSymbols { get; set; }

    public AssemblerService(ASMBLR assembler, IMainMemory memory)
    {
        _assembler = assembler;
        _mainMemory = memory;
    }

    public byte[] AssembleSourceCodeService(string sourceCode)
    {
        string configFilePath = Path.GetFullPath(AppContext.BaseDirectory + "../../../../Configs/IVT.json");
        string configFile = File.ReadAllText(configFilePath);

        var objectCode = _assembler.Assemble(sourceCode, out _);
        _mainMemory.LoadMachineCode(objectCode);

        var table = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(configFile);
        foreach (var irqEntry in table)
        {

            var irqData = irqEntry.Value;

            int handlerStartAddress = Convert.ToInt32(irqData["handlerStartAddress"], 16);
            string handlerCode = irqData["handlerCode"];

            var objectCodeInterrupt = _assembler.Assemble(handlerCode, out _);
            _mainMemory.SetISR(handlerStartAddress, objectCodeInterrupt);
        }

        SourceCodeAssembled?.Invoke(this, objectCode);
        DebugSymbols =  _assembler.GetDebugSymbols();
        return objectCode;
    }
}