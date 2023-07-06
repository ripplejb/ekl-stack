using System.Text;
using System.Threading.Channels;
using Serilog;

namespace BackupDisks;

public class AsyncBackup
{
    private readonly Channel<string[]> _fileQueue = Channel.CreateUnbounded<string[]>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;
    private static string _blankText = "";

    public AsyncBackup(ILogger logger)
    {
        GenerateLargeBlankText();
        _logger = logger;
    }

    public void CancelCopy()
    {
        _cancellationTokenSource.Cancel();
    }
    
    public async Task Copy(string src, string dst)
    {
        _logger.Information("Start channel");
        var startChannel = StartChannel(src.EndsWith('/')?src:$"{src}/", dst.EndsWith('/')?dst:$"{dst}/", _cancellationTokenSource.Token);
        var readChannel =  ReadChannel(_cancellationTokenSource.Token);
        await Task.WhenAll(startChannel, readChannel);
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
            _logger.Information("Found folder {Dir}", dir);
            var success = CreateFolder(src, dst, dir);
            if (!success) continue;
            try
            {
                var dirs = Directory.GetDirectories(dir);
                PushToStack(stack, dirs);
                await WriteFilesToChannel(src, dst, dir, cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "error writing to the channel");
            }
        }
        
    }

    private async Task ReadChannel(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var file = Array.Empty<string>();
            try
            {
                file = await _fileQueue.Reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Information(ex, "Operation cancelled");
                break;
            }

            await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                _logger.Information("Start copying file {File}", file[0]);
                try
                {
                    if (!File.Exists(file[1]))
                    {
                        WriteConsole($"Start copying file {file[0]}");
                        File.Copy(file[0], file[1]);
                        WriteConsole($"Finished copying file {file[0]}");
                        _logger.Information("Finished copying file {File}", file[0]);
                    }
                    else
                    {
                        WriteConsole($"File {file[0]} already exists");
                        _logger.Information("File {File} already exists", file[1]);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error copying file {File}", file[0]);
                }
            }, cancellationToken);
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
    
    private bool CreateFolder(string src, string dst, string newFolderPath)
    {
        var folderName = GetNewFolderName(src, dst, newFolderPath);
        _logger.Information("Create folder {FolderName}", folderName);
        try
        {
            Directory.CreateDirectory(folderName);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error creating folder {FolderName}", folderName);
        }

        return false;
    }

    private string GetNewFolderName(string src, string dst, string newFolderPath)
    {
        var newFolder = Path.Combine(dst, new DirectoryInfo(src).Name);
        if (!src.Equals(newFolderPath))
        {
            newFolder = Path.Combine(newFolder, newFolderPath.Substring(src.Length));
        }

        return newFolder;
    }
    
    private static void PushToStack(Stack<string> stack, IEnumerable<string> list)
    {
        foreach (var s in list)
        {
            stack.Push(s);
        }
    }

    private static void GenerateLargeBlankText()
    {
        if (_blankText.Length != 0) return;
        var sb = new StringBuilder();
        for (var i = 0; i < 1000; i++)
        {
            sb.Append(' ');
        }

        _blankText = sb.ToString();
    }
    
    private static void WriteConsole(string message)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 10); 
        Console.WriteLine(_blankText);

        Console.SetCursorPosition(0, Console.WindowHeight - 10); 
        Console.WriteLine(message);
    }
}