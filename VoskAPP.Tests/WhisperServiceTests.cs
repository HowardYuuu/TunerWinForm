using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace VoskAPP.Tests;

public class WhisperServiceTests
{
    private sealed class FakeProcessor : IWhisperProcessorAdapter
    {
        private readonly IEnumerable<TranscriptionSegment> _segments;
        public FakeProcessor(IEnumerable<TranscriptionSegment> segments) => _segments = segments;
        public async IAsyncEnumerable<TranscriptionSegment> ProcessAsync(Stream audio, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var s in _segments)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(5, token); // 模擬一些非同步延遲
                yield return s;
            }
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeFactory : IWhisperFactoryAdapter
    {
        private readonly IEnumerable<TranscriptionSegment> _segments;
        public FakeFactory(IEnumerable<TranscriptionSegment> segments) => _segments = segments;
        public Task<IWhisperProcessorAdapter> CreateProcessorAsync(string modelPath, string language, bool useGpu = false, int gpuDevice = 0, CancellationToken token = default)
        {
            IWhisperProcessorAdapter adapter = new FakeProcessor(_segments);
            return Task.FromResult(adapter);
        }
    }

    [Fact]
    public async Task TranscribeAsync_ReturnsAllSegments()
    {
        // Arrange
        var segments = new[]
        {
            new TranscriptionSegment(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(500), "你好"),
            new TranscriptionSegment(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1000), "世界")
        };
        var factory = new FakeFactory(segments);
        var service = new WhisperService(factory);

        // 建立假 wav 檔 (內容不重要)
        string tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, new byte[10]);

        try
        {
            // Act
            var result = await service.TranscribeAsync("model.bin", tempFile, "zh");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("你好世界", string.Concat(result.Select(r => r.Text)));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TranscribeAsync_CanBeCancelled()
    {
        // Arrange: 多個段落以便在中途取消
        var segments = Enumerable.Range(0, 5)
            .Select(i => new TranscriptionSegment(TimeSpan.FromMilliseconds(i * 100), TimeSpan.FromMilliseconds(i * 100 + 100), $"S{i}"))
            .ToArray();
        var factory = new FakeFactory(segments);
        var service = new WhisperService(factory);

        string tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, new byte[10]);
        var cts = new CancellationTokenSource();

        // Act
        var task = Task.Run(async () => await service.TranscribeAsync("model.bin", tempFile, "zh", token: cts.Token));
        cts.CancelAfter(15); // 在第一個或第二個段落後取消

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);

        File.Delete(tempFile);
    }
}
