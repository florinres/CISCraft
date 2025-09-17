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
    public List<ISR>? Isrs;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
    public AssemblerService(ASMBLR assembler, IMainMemory memory)
    {
        _assembler = assembler;
        _mainMemory = memory;
    }
    private List<ISR> ReadIVTJson(){
        string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
        string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
        if (!File.Exists(jsonPath))
            return new List<ISR>();
        string json = File.ReadAllText(jsonPath);

        return JsonSerializer.Deserialize<List<ISR>>(json, JsonOpts) ?? new();
        }

    public byte[] AssembleSourceCodeService(string sourceCode)
    {

        var objectCode = _assembler.Assemble(sourceCode, out _);
        var originalDbgSymbols = _assembler.GetDebugSymbols();
        _mainMemory.LoadMachineCode(objectCode);

        this.Isrs = ReadIVTJson();
        foreach (var isr in Isrs)
        {
            string handlerCode = isr.TextCode;
            ushort handlerStartAddress = isr.ISRAddress;

            var objectCodeInterrupt = _assembler.Assemble(handlerCode, out _);
            _mainMemory.SetISR(handlerStartAddress, objectCodeInterrupt);
        }

        SourceCodeAssembled?.Invoke(this, objectCode);
        _assembler.SetInternalDebugSymbols(originalDbgSymbols);
        DebugSymbols = _assembler.GetDebugSymbols();
        return objectCode;
    }
}