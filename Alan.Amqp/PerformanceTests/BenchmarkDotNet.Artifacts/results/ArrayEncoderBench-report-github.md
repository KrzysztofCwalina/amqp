``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                 Method |         Mean |     Error |    StdDev |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|----------------------- |-------------:|----------:|----------:|-----------:|------:|------:|-----------:|
| ArrayInt32_Encode1M_MA | 109,328.3 μs | 666.42 μs | 590.76 μs | 12000.0000 |     - |     - | 50332122 B |
| ArrayInt32_Encode1M_SB |     899.5 μs |   6.47 μs |   6.05 μs |          - |     - |     - |        1 B |
