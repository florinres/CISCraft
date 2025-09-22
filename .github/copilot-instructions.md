# MicroCISC Coding Agent Instructions

## Repository Overview

**MicroCISC** is a comprehensive CPU simulator and emulator project implementing a custom 16-bit CISC (Complex Instruction Set Computing) architecture. The project provides:

- A custom assembler for the MicroCISC assembly language
- A CPU emulator with microprogram execution
- Main memory simulation with interrupt handling
- A Windows Presentation Foundation (WPF) user interface for interactive debugging and visualization
- Comprehensive test suites for validation

### Project Characteristics
- **Language**: C# targeting .NET 9.0
- **Size**: ~116 C# source files, medium-sized codebase
- **Architecture**: Modular design with separate business logic, UI, and test projects
- **Target Platform**: Windows (WPF UI requires Windows runtime)
- **Build System**: .NET SDK with MSBuild

## Build Instructions

### Prerequisites
- **.NET 9.0 SDK** (CRITICAL: Project explicitly targets .NET 9.0)
- **Windows OS** for UI functionality (WPF requires Windows Desktop runtime)
- **Visual Studio 2022** or compatible IDE (recommended)

### Build Commands

**ALWAYS** use the EnableWindowsTargeting flag when building on non-Windows systems:

```bash
# Clean build (recommended first step)
dotnet clean

# Restore dependencies
dotnet restore

# Build entire solution
dotnet build -p:EnableWindowsTargeting=true

# Run tests
dotnet test -p:EnableWindowsTargeting=true

# Publish UI application (Windows x64)
dotnet publish -c Release -r win-x64 -p:EnableWindowsTargeting=true -p:PublishSingleFile=true -p:SelfContained=true -o Release Ui/Ui.csproj
```

### Known Build Issues and Workarounds

1. **NETSDK1045 Error**: If you encounter ".NET SDK does not support targeting .NET 9.0", ensure you have .NET 9.0 SDK installed. The project CANNOT be downgraded to .NET 8.0 due to dependencies. **This is a CRITICAL blocking issue** - building requires exactly .NET 9.0 SDK.

2. **Windows Desktop SDK Missing**: On Linux/macOS, use `-p:EnableWindowsTargeting=true` flag for cross-compilation. Without this flag, WPF projects fail with "Microsoft.NET.Sdk.WindowsDesktop.targets not found".

3. **WPF Compilation Issues**: UI project requires Windows Desktop SDK. Individual business logic projects can be built independently for testing core functionality, but ALL projects target .NET 9.0.

4. **Environment Mismatch**: If running on systems with .NET 8.0 or lower, **all build commands will fail**. The GitHub Actions CI uses .NET 9.0 explicitly.

### Test Execution

Tests generate trace files (`SnapShots_*.txt`) for debugging. **ALWAYS** run tests from the repository root to ensure correct configuration file paths:

```bash
# Run all tests
dotnet test -p:EnableWindowsTargeting=true

# Run specific test project
dotnet test CPU.Tests/CPU.Tests.csproj -p:EnableWindowsTargeting=true
```

**Test Dependencies**: Tests require `../../../../Configs/MPM.json` and `../../../../Configs/IVT.json` relative to test output directory. These paths are hardcoded in the test setup.

## Project Architecture and Layout

### Solution Structure
```
MicroCISC/
├── Assembler.Business/        # Core assembler logic and grammar
├── Assembler.Main/           # Standalone assembler application
├── CPU.Business/             # CPU emulation core logic
├── CPU.Main/                # Console CPU runner
├── CPU.Tests/               # CPU instruction tests
├── MainMemory.Business/     # Memory management and interrupt handling
├── MainMemory.BusinessTests/ # Memory subsystem tests
├── MainMemory.Main/         # Standalone memory demo
├── Ui/                      # WPF user interface
├── Configs/                 # Configuration files (MPM.json, IVT.json)
├── Tools/                   # Utility scripts (Python converter)
└── Docs/                    # PDF documentation
```

### Key Configuration Files

**MPM.json** (`Configs/MPM.json`): Microprogram memory definitions. Contains CPU microinstruction sequences for all instruction types. CRITICAL for CPU execution.

**IVT.json** (`Configs/IVT.json`): Interrupt Vector Table. Defines interrupt handlers with assembly code. Modified at runtime by UI.

### Entry Points and Main Programs

1. **Ui/Program.cs**: WPF application for interactive CPU simulation
2. **CPU.Main/Program.cs**: Console application that executes assembled code
3. **Assembler.Main/Program.cs**: Standalone assembler (reads `main.s`, outputs `main.obj`)

### Assembly Language
The project implements a custom assembly language. Example syntax (from `Ui/Assets/default_code.txt`):
```assembly
proc start
    mov r0, 5
    call factorial
    jmp factorial
    nop
    halt
endp start
```

### Critical Path Dependencies

1. **Configuration Loading**: All components expect `Configs/MPM.json` at relative path `../../../../Configs/MPM.json` from executable directory
2. **Test Trace Generation**: Tests write to `SnapShots_*.txt` files for debugging
3. **Assembly File Processing**: Applications look for `main.s` in working directory

## Validation Steps

### CI/CD Pipeline
- **GitHub Actions**: `.github/workflows/build_and_test.yaml`
- **Trigger**: Pull requests to main branch  
- **Process**: Ubuntu runner with .NET 9.0, builds and tests entire solution
- **Release**: `.github/workflows/genereate_release.yaml` publishes Windows executable on tag push

### Manual Validation Checklist
1. **Build Verification**: Run `dotnet build -p:EnableWindowsTargeting=true` successfully
2. **Test Execution**: All tests in `CPU.Tests` and `MainMemory.BusinessTests` pass
3. **Configuration Validation**: Verify `Configs/*.json` files are valid JSON
4. **Assembly Testing**: Create simple `.s` file and test with assembler main program
5. **UI Functionality**: Run WPF application and verify CPU diagram renders correctly

### Development Tools and Utilities

**Python Conversion Tool** (`Tools/convert_to_json.py`): Converts Excel microprogram files to JSON format. Requires pandas and openpyxl dependencies:
```bash
pip install pandas openpyxl
cd Tools && python3 convert_to_json.py
```

### Development Notes

- **TODO Comments**: Found minimal TODO items, project appears production-ready
- **Test Coverage**: Comprehensive instruction-level testing with trace validation  
- **Memory Management**: Uses wrapper classes for memory abstraction
- **Interrupt Handling**: Full interrupt vector table implementation with runtime modification
- **Configuration Validation**: Both MPM.json and IVT.json are valid JSON files

### Working with the Codebase

**DO**: Always build with EnableWindowsTargeting flag on non-Windows systems
**DO**: Run tests from repository root directory for correct configuration paths  
**DO**: Use existing test patterns when adding new instruction tests
**DO**: Preserve microprogram JSON structure when modifying MPM.json

**DON'T**: Modify .NET target framework (locked to 9.0 across ALL projects)
**DON'T**: Change hardcoded configuration file paths without updating all references
**DON'T**: Remove or modify test trace generation logic (creates SnapShots_*.txt files)
**DON'T**: Bypass the assembler grammar system for instruction parsing
**DON'T**: Attempt to build without .NET 9.0 SDK - project will fail completely

### Quick Start for Agent

To make changes effectively:
1. Build project first to understand current state: `dotnet build -p:EnableWindowsTargeting=true`
2. For CPU instruction changes: Modify business logic, update tests in `CPU.Tests/`
3. For assembly language changes: Update `Assembler.Business/CiscGrammar.cs` and `Encoder.cs`
4. For UI changes: Work in `Ui/` project, test with WPF application
5. Always run tests after changes: `dotnet test -p:EnableWindowsTargeting=true`

**Trust these instructions** - they are based on comprehensive repository analysis. Only search for additional information if instructions prove incomplete or incorrect for your specific task.