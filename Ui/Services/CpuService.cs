
using System.Data.Common;
using System.IO;
using System.Windows.Media;
using CPU.Business.Models;
using ICSharpCode.AvalonEdit;
using Ui.Components;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Microprogram;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class CpuService : ICpuService
{
    private readonly CPU.Business.CPU _cpu;
    private readonly IMicroprogramViewModel _microprogramService;
    private readonly IDiagramViewModel _diagram;
    private FileViewModel? _fileViewModel;
    Color _semiTransparentYellow;
    private Dictionary<short, ushort> _debugSymbls;
    private Dictionary<int, int> _mirLookUpIndex = new Dictionary<int, int>
    {
        {0, 0},
        {1, 1},
        {2, 5},
        {3, 2},
        {4, 3},
        {5, 4},
        {6, 6}
    };
    public HighlightCurrentLineBackgroundRenderer? Highlight
    {
        get;
        set;
    }

    public CpuService(IMicroprogramViewModel microprogramService, CPU.Business.CPU cpu, IDiagramViewModel diagram)
    {
        _cpu = cpu;
        _diagram = diagram;
        _microprogramService = microprogramService;
        _semiTransparentYellow = Color.FromArgb(38, 255, 255, 0);
        _ = LoadJsonMpm();
        _debugSymbls = new Dictionary<short, ushort>();
    }

    public async Task LoadJsonMpm(string filePath = "", bool debug = false)
    {
        string fullPath;
        
        if (filePath == "")
        {
            var projectRoot = AppContext.BaseDirectory + "../../../";
            fullPath = Path.Combine(projectRoot, "../Configs", "Mpm.json");
        }
        else
        {
            fullPath = filePath;
        }

        var jsonString = await File.ReadAllTextAsync(fullPath);
        //both can be paralized if necesarry
        _microprogramService.LoadMicroprogramFromJson(jsonString);
        _cpu.LoadJsonMpm(jsonString);
    }

    public void StepMicrocommand()
    {
        _diagram.ResetHighlight();
        var (row, column) = _cpu.StepMicrocommand();

        if (row == 0 && column == 0)
        {
            _microprogramService.ClearAllHighlightedRows();
            
            if (Highlight != null && _fileViewModel != null && _fileViewModel.EditorInstance != null)
            {
                short index = (short)(_cpu.Registers[REGISTERS.PC] - 16);
                if (_debugSymbls.ContainsKey(index))
                    Highlight?.SetLine(_debugSymbls[index]);
            }
        }

        _microprogramService.CurrentRow = row;
        _microprogramService.CurrentColumn = _mirLookUpIndex[column];
    }
    public void StepMicroinstruction()
    {
        _diagram.ResetHighlight();
        int row, column;
        row = 1;
        column = 1;
        while (column != 6)
        {
            (row, column) = _cpu.StepMicrocommand();
        }
        if (row == 0)
        {
            _microprogramService.ClearAllHighlightedRows();
        }

        _microprogramService.CurrentRow = row;
        _microprogramService.CurrentColumn = -1;

        if (Highlight != null && _fileViewModel != null && _fileViewModel.EditorInstance != null)
        {
            short index = (short)(_cpu.Registers[REGISTERS.PC] - 16);
            if (_debugSymbls.ContainsKey(index))
                Highlight?.SetLine(_debugSymbls[index]);
        }
    }
    public void StepInstruction()
    {
        _diagram.ResetHighlight();
        int row, col;
        row = 1;
        col = 1;
        _cpu.StepMicrocommand();
        while (row != 0 || col != 0)
        {
            (row, col) = _cpu.StepMicrocommand();
        }

        if (Highlight != null && _fileViewModel != null && _fileViewModel.EditorInstance != null)
        {
            short index = (short)(_cpu.Registers[REGISTERS.PC] - 16);
            if (_debugSymbls.ContainsKey(index))
                Highlight?.SetLine(_debugSymbls[index]);
        }
    }
    public void ResetProgram()
    {
        _cpu.ResetProgram();
        _microprogramService.CurrentRow = -1;
        _microprogramService.CurrentColumn = -1;
        _microprogramService.ClearAllHighlightedRows();
    }
    public void SetActiveEditor(FileViewModel fileViewModel)
    {
        _fileViewModel = fileViewModel;

    }
    public void StartDebugging()
    {
        if (_fileViewModel?.EditorInstance != null)
        {
            short index = (short)(_cpu.Registers[REGISTERS.PC] - 16);
            ushort lineNum = _debugSymbls[index];
            Highlight = new HighlightCurrentLineBackgroundRenderer(_fileViewModel.EditorInstance, lineNum, _semiTransparentYellow);
            _fileViewModel.EditorInstance.TextArea.TextView.BackgroundRenderers.Add(Highlight);
        }
    }
    public void StopDebugging()
    {
        if (Highlight != null && _fileViewModel?.EditorInstance != null)
        {
            _fileViewModel.EditorInstance.TextArea.TextView.BackgroundRenderers.Remove(Highlight);
            Highlight = null;
            _cpu.ResetProgram();
            _microprogramService.CurrentRow = -1;
            _microprogramService.CurrentColumn = -1;
            _microprogramService.ClearAllHighlightedRows();
        }
    }
    public void SetDebugSymbols(Dictionary<short, ushort> debugSymbols)
    {
        _debugSymbls = debugSymbols;
    }
}