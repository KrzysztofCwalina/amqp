``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|    Method |      Mean |     Error |    StdDev |    Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |----------:|----------:|----------:|----------:|------:|------:|------:|----------:|
| Interface | 0.0348 ns | 0.0165 ns | 0.0155 ns | 0.0301 ns |     - |     - |     - |         - |
|     Class | 0.0005 ns | 0.0014 ns | 0.0012 ns | 0.0000 ns |     - |     - |     - |         - |
