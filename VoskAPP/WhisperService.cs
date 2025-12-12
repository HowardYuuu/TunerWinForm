using Whisper.net;

namespace VoskAPP;

// 獨立的段落資料模型，避免測試依賴外部套件型別
public sealed record TranscriptionSegment(TimeSpan Start, TimeSpan End, string Text);

public interface IWhisperProcessorAdapter : IAsyncDisposable
{
    IAsyncEnumerable<TranscriptionSegment> ProcessAsync(Stream audio, CancellationToken token = default);
}

public interface IWhisperFactoryAdapter
{
    Task<IWhisperProcessorAdapter> CreateProcessorAsync(string modelPath, string language, bool useGpu = false, int gpuDevice = 0, CancellationToken token = default);
}

// 實際包裝 Whisper.net 的處理器
internal sealed class WhisperProcessorAdapter : IWhisperProcessorAdapter
{
    private readonly WhisperProcessor _inner;

    public WhisperProcessorAdapter(WhisperProcessor inner)
    {
        _inner = inner;
    }

    public async IAsyncEnumerable<TranscriptionSegment> ProcessAsync(Stream audio, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var r in _inner.ProcessAsync(audio, token))
        {
            yield return new TranscriptionSegment(r.Start, r.End, r.Text);
        }
    }

    public ValueTask DisposeAsync() => _inner.DisposeAsync();
}

// 實際工廠
internal sealed class WhisperFactoryAdapter : IWhisperFactoryAdapter
{
    public Task<IWhisperProcessorAdapter> CreateProcessorAsync(string modelPath, string language, bool useGpu = false, int gpuDevice = 0, CancellationToken token = default)
    {
        var options = new WhisperFactoryOptions
        {
            UseGpu = useGpu,
            GpuDevice = gpuDevice,
            DelayInitialization = false
        };
        var factory = WhisperFactory.FromPath(modelPath, options);
        var processor = factory.CreateBuilder().WithLanguage(language).Build();
        IWhisperProcessorAdapter adapter = new WhisperProcessorAdapter(processor);
        return Task.FromResult(adapter);
    }
}

// 提供給應用與測試呼叫的服務
public class WhisperService
{
    private readonly IWhisperFactoryAdapter _factory;

    public WhisperService(IWhisperFactoryAdapter factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<TranscriptionSegment>> TranscribeAsync(string modelPath, string wavPath, string language, bool useGpu = false, int gpuDevice = 0, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await using var processor = await _factory.CreateProcessorAsync(modelPath, language, useGpu, gpuDevice, token);
        await using var fs = File.OpenRead(wavPath);

        var list = new List<TranscriptionSegment>();
        await foreach (var seg in processor.ProcessAsync(fs, token))
        {
            list.Add(seg);
        }
        return list;
    }
}
