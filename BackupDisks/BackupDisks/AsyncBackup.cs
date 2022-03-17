using System.Threading.Channels;
using Serilog;

namespace BackupDisks;

public class AsyncBackup
{
    private readonly Channel<string[]> _fileQueue = Channel.CreateUnbounded<string[]>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;

    public AsyncBackup(ILogger logger)
    {
        _logger = logger;
    }

    public void CancelCopy()
    {
        _cancellationTokenSource.Cancel();
    }
    
    public async Task Copy(string src, string dst)
    {
        _logger.Information("Start channel");
        await StartChannel(src, dst, _cancellationTokenSource.Token);
        await ReadChannel(_cancellationTokenSource.Token);
    }

    private async Task StartChannel(string src, string dst, CancellationToken cancellationToken)
    {
        var stack = new Stack<string>();
        if (!Directory.Exists(src) || !Directory.Exists(dst))
        {
            _logger.Error(new Exception("Invalid source or destination folder"), "Error copying folder. Make sure folders exists");
            return;
        }
        stack.Push(src);
        while (stack.TryPop(out var dir))
        {
            _logger.Information("Found folder {dir}");
            CreateFolder(src, dst, dir);
            var dirs = Directory.GetDirectories(dir);
            PushToStack(stack, dirs);
            await WriteFilesToChannel(src, dst, dir, cancellationToken);
        }
        
    }

    private async Task ReadChannel(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var file = await _fileQueue.Reader.ReadAsync(cancellationToken);
            await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                _logger.Information("Start copying file {File}", file[0]);
                try
                {
                    File.Copy(file[0], file[1]);
                    _logger.Information("Finished copying file {File}", file[0]);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error copying file {File}", file[0]);
                }
            });
            if (cancellationToken.IsCancellationRequested) break;
        }
    }
    
    private async Task WriteFilesToChannel(string src, string dst, string folder, CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(folder);
        var newFolder = GetNewFolderName(src, dst, folder); 
        foreach (var file in files)
        {
            await _fileQueue.Writer.WriteAsync(new[] {file, Path.Combine(newFolder, Path.GetFileName(file))}, cancellationToken);
        }
    }
    
    private void CreateFolder(string src, string dst, string newFolderPath)
    {
        var folderName = GetNewFolderName(src, dst, newFolderPath);
        _logger.Information("Create folder {FolderName}", folderName);
        try
        {
            Directory.CreateDirectory(folderName);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error creating folder {FolderName}", folderName);
            throw;
        }
        
    }

    private string GetNewFolderName(string src, string dst, string newFolderPath)
    {
        var newFolder = Path.Combine(dst, new DirectoryInfo(src).Name);
        if (!src.Equals(newFolderPath))
        {
            newFolder = Path.Combine(newFolder, newFolderPath.Substring(src.Length + 1));
        }

        return newFolder;
    }
    
    private void PushToStack(Stack<string> stack, string[] list)
    {
        foreach (var s in list)
        {
            stack.Push(s);
        }
    }
}