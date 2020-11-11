using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;


namespace EntityCloner.Microsoft.EntityFrameworkCore.Benchmarks
{
    public class Md5VsSha256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public Md5VsSha256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
    //public class Program
    //{
    //    private static void Main(string[] args) => EntityClonerBenchmarkRunner.Run(args, typeof(Program).Assembly);
    //}

    //public static class EntityClonerBenchmarkRunner
    //{
    //    public static void Run(string[] args, Assembly assembly, IConfig config = null)
    //    {
    //        if (config == null)
    //        {
    //            config = DefaultConfig.Instance;
    //        }

    //        config = config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().Concat(new[] { MemoryDiagnoser.Default }).ToArray());

    //        var index = Array.FindIndex(args, s => s == "--perflab");
    //        if (index >= 0)
    //        {
    //            var argList = args.ToList();
    //            argList.RemoveAt(index);
    //            args = argList.ToArray();

    //            config = config
    //                .AddColumn(StatisticColumn.OperationsPerSecond, new ParamsSummaryColumn())
    //                .AddExporter(
    //                    MarkdownExporter.GitHub, new CsvExporter(
    //                        CsvSeparator.Comma,
    //                        new SummaryStyle(null,
    //                            printUnitsInHeader: true,
    //                            SizeUnit.KB,
    //                            TimeUnit.Microsecond,
    //                            printUnitsInContent: false)));
    //        }

    //        BenchmarkSwitcher.FromAssembly(assembly).Run(args, config);
    //    }
    //}
    //public class ParamsSummaryColumn : IColumn
    //{
    //    public string Id => nameof(ParamsSummaryColumn);
    //    public string ColumnName { get; } = "Params";
    //    public bool IsDefault(Summary summary, BenchmarkCase benchmark) => false;
    //    public string GetValue(Summary summary, BenchmarkCase benchmark) => benchmark.Parameters.DisplayInfo;
    //    public bool IsAvailable(Summary summary) => true;
    //    public bool AlwaysShow => true;
    //    public ColumnCategory Category => ColumnCategory.Params;
    //    public int PriorityInCategory => 0;
    //    public override string ToString() => ColumnName;
    //    public bool IsNumeric => false;
    //    public UnitType UnitType => UnitType.Dimensionless;
    //    public string GetValue(Summary summary, BenchmarkCase benchmark, SummaryStyle style) => GetValue(summary, benchmark);
    //    public string Legend => "Summary of all parameter values";
    //}
}
