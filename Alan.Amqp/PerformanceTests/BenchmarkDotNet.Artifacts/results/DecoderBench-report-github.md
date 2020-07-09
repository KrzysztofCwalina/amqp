``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                          Method |              Mean |           Error |          StdDev |     Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|-------------------------------- |------------------:|----------------:|----------------:|----------:|--------:|--------:|-----------:|
|              Bytes_Decode1MB_MA |    462,754.167 ns |   2,751.0877 ns |   2,147.8683 ns |   12.6953 | 12.6953 | 12.6953 |  1048600 B |
|              Bytes_Decode1MB_SP |          4.173 ns |       0.0159 ns |       0.0133 ns |         - |       - |       - |          - |
|      Bytes_Decode1MB_SP_ToArray |    455,473.268 ns |   4,308.2113 ns |   3,597.5516 ns |   13.6719 | 13.6719 | 13.6719 |  1048600 B |
|          ArrayInt32_Decode1M_MA | 86,314,030.769 ns | 284,015.4066 ns | 237,165.7276 ns | 6000.0000 |       - |       - | 29360383 B |
|          ArrayInt32_Decode1M_SP |    962,466.270 ns |  15,586.7656 ns |  14,579.8702 ns |         - |       - |       - |        1 B |
|  ArrayInt32_Decode1M_SP_ToArray |  2,179,585.463 ns |  16,218.2619 ns |  14,377.0668 ns |   46.8750 | 46.8750 | 46.8750 |  4194385 B |
| ArrayInt32_Decode1M_SP_Iterator |  2,619,921.540 ns |   5,765.3181 ns |   5,110.8044 ns |         - |       - |       - |       22 B |
