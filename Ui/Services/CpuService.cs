
using System.IO;
using CPU.Business.Models;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.Services;

public class CpuService : ICpuService
{
    private readonly CPU.Business.CPU _cpu;
    private readonly IMicroprogramViewModel _microprogramService;
    private readonly IDiagramViewModel _diagram;
    
    public CpuService(IMicroprogramViewModel microprogramService, CPU.Business.CPU cpu, IDiagramViewModel diagram, IMicroprogramViewModel microprogramViewModel)
    {
        _cpu = cpu;
        _diagram = diagram;
        _microprogramService = microprogramService;
        _ = LoadJsonMpm();
    }

    public async Task LoadJsonMpm(string filePath = "", bool debug = false)
    {
        string fullPath;
        
        if (filePath == "")
        {
            var projectRoot = AppContext.BaseDirectory!;
            fullPath = Path.Combine(projectRoot, "Assets", "Mpm.json");
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

    public (int, int) StepMicrocommand()
    {
        _diagram.ResetHighlight();
         var (row, column) = _cpu.StepMicrocommand();
         if (row == 0 && column == 0)
         {
             _microprogramService.ClearAllHighlightedRows();
         }
         _microprogramService.CurrentRow = row;
         _microprogramService.CurrentColumn = column;
         return (row, column);
    }
}