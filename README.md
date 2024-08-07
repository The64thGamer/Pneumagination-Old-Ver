# Installation

Once installed, the main folder needs to be In Visual Studio, two commands need to be ran to compile correctly.
```
dotnet add package EasyCompressor.Snappier --version 2.0.2

dotnet add package MemoryPack --version 1.21.1
```
The game's folder needs to be renamed "Pneumagination". The Pneumagination.csproj may need the line ```<GenerateAssemblyInfo>false</GenerateAssemblyInfo>``` added to remove duplicate errors. The C# project should also be regenerated in Projects>Tools. If there are still duplicate errors, manually remove those lines.
