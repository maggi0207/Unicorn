using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Components;

/// <summary>
/// 
/// </summary>
public partial class FileUpload2 : ComponentBase, IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private readonly string _inPutId = "fileUpload_" + Guid.NewGuid().ToString();
    private readonly string _dropZoneId = "DropZone_" + Guid.NewGuid().ToString();
    private DotNetObjectReference<FileUpload2>? _dotNetRef;
    private InputFile? _inputFiles;
    private CancellationTokenSource? _cts;
    private string? _fullpath;
    private byte[]? _pendingFileBytes;
    private bool _isDrawerOpen;
    private bool _isProcessing;

    /// <summary>
    /// AllowMultiple
    /// </summary>
    [Parameter] public bool AllowMultiple { get; set; } = true;

    /// <summary>
    /// Title
    /// </summary>
    [Parameter] public string Title { get; set; } = "Upload Title";

    /// <summary>
    /// Folderpath
    /// </summary>
    [Parameter]
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// The category of acceptable file types for this upload area.
    /// The component resolves the actual extensions internally. 
    /// </summary>
    [Parameter]
    public FileAcceptableType FileType { get; set; } = FileAcceptableType.Registration;

    /// <summary>
    /// Resolved list of acceptable file types based on <see cref="FileType"/>
    /// </summary>
    public List<KeyValuePair<string, string>> AcceptableTypes => FileTypeDefinitions.GetTypes(FileType);

    /// <summary>
    /// viewdetailurl
    /// </summary>
    [Parameter] public string ViewDetailsurl { get; set; } = "#";
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public int MaxFileSizeMB { get; set; } = 20 * 1024 * 1024;
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public EventCallback<List<IBrowserFile>> OnFilesSelected { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public EventCallback<string> OnFileUpload { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter] public EventCallback<string> OnFileRemoved { get; set; }

    /// <summary>
    /// Validation messages to display below the upload area. 
    /// </summary>
    [Parameter] public List<FileValidationMessage> ValidationMessages { get; set; } = new();

    /// <summary>
    /// Whether the file has blocking errors that prevent proceeding.
    /// </summary>
    [Parameter] public bool HasBlockingErrors { get; set; }

    /// <summary>
    /// Whether the file has non-blocking warnings.
    /// </summary>
    [Parameter] public bool HasWarnings { get; set; }

    /// <summary>
    /// When true, saves the file to folder path immediately on upload.
    /// when false, keeps the file in memory until SavePendingFileAsync() is called.
    /// use false flows where the file should only be saved on final submit.
    /// </summary>
    [Parameter] public bool SaveOnUpload { get; set; } = true;

    /// <summary>
    /// Display name of a previously uploaded file. When set, the component
    /// renders in successful/ErorsMustFix state instead of default.
    /// </summary>
    [Parameter] public string? FileName { get; set; }

    private List<IBrowserFile> SelectedFiles { get; set; } = new();

    /// <summary>
    /// Progress State
    /// </summary>
    public FileUploadState UploadState { get; set; } = FileUploadState.Default;
    /// <summary>
    /// 
    /// </summary>
    public string DisplayFileName { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int ProgressPercent { get; set; } = 0;
    /// <summary>
    /// Error or Validation Message to shown to user
    /// </summary>
    protected string? ValidationMessage { get; private set; }
    /// <summary>
    /// 
    /// </summary>
    protected IBrowserFile? CurrentFile { get; private set; }
    /// <summary>
    /// Current state of upload
    /// </summary>
    protected FileUploadState State { get; private set; } = FileUploadState.Default;

    /// <summary>
    ///
    /// </summary>
    protected override void OnInitialized()
    {
        // Restore state if a file was previously uploaded
        if (!string.IsNullOrEmpty(FileName))
        {
            DisplayFileName = FileName;
            State = HasBlockingErrors ? FileUploadState.ErrorsMustFix : FileUploadState.Successful;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnParametersSet()
    {
        //dont intefere while a file is being uploaded or processed
        if (_isProcessing)
        {
            return;
        }

        //sync state when parent clears file data 
        if (string.IsNullOrEmpty(FileName) && State != FileUploadState.Default && State != FileUploadState.Uploading)
        {
            //only reset if the component isn't actively uploading or already in default
            if (_pendingFileBytes == null && string.IsNullOrEmpty(_fullpath))
            {
                State = FileUploadState.Default;
                DisplayFileName = string.Empty;
                ValidationMessage = null;
            }
        }
    }

    private bool HasFileOnDisk => !string.IsNullOrEmpty(_fullpath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (State == FileUploadState.Default)
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("fileUpload.initDropZone", _dropZoneId, _inPutId, _dotNetRef, AllowMultiple);
        }
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        _isProcessing = true;

        //User selected a new file - delete the previous one if it exists
        if (!string.IsNullOrEmpty(_fullpath))
        {
            try
            {
                if (File.Exists(_fullpath))
                {
                    File.Delete(_fullpath);
                }
            }
            catch { }
            _fullpath = null;
            _pendingFileBytes = null;
        }

        ValidationMessage = null;

        if (!AllowMultiple && e.FileCount > 1)
        {
            await ShowMultipleFilesError();
            _isProcessing = false;
            return;
        }

        var files = AllowMultiple ? e.GetMultipleFiles() : new[] { e.File };
        SelectedFiles.Clear();
        foreach (var file in files)
        {
            if (file == null)
            {
                return;
            }

            //Empty file validation
            if (file.Size == 0)
            {
                ValidationMessage = "empty_file";
                DisplayFileName = file.Name;
                CurrentFile = file;
                ProgressPercent = 0;
                State = FileUploadState.ErrorsMustFix;
                await OnFileRemoved.InvokeAsync();
                await InvokeAsync(StateHasChanged);
                return;
            }

            //size validation
            var maxBytes = MaxFileSizeMB;
            if (file.Size > maxBytes)
            {
                ValidationMessage = $"File is too large. Max size is {MaxFileSizeMB} MB.";
                CurrentFile = file;
                ProgressPercent = 0;
                State = FileUploadState.ErrorsMustFix;
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (AcceptableTypes is { Count: > 0 })
            {
                var ext = (Path.GetExtension(file.Name) ?? string.Empty).ToLowerInvariant();
                var allowedext = AcceptableTypes.Select(t =>
                {
                    return t.Key.ToLowerInvariant();
                }).ToHashSet();
                if (!allowedext.Contains(ext))
                {
                    ValidationMessage = "invalid_format";
                    DisplayFileName = file.Name;
                    CurrentFile = file;
                    ProgressPercent = 0;
                    State = FileUploadState.ErrorsMustFix;
                    await OnFileRemoved.InvokeAsync();
                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            //start uploading
            State = FileUploadState.Uploading;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            await InvokeAsync(StateHasChanged);
            DisplayFileName = file.Name;
            try
            {
                _fullpath = await UploadToServerPathProgressBar(file, _cts.Token);
                if (!_cts.IsCancellationRequested)
                {
                    ProgressPercent = 100;
                    State = FileUploadState.Successful;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (OperationCanceledException)
            {
                if (!string.IsNullOrEmpty(_fullpath))
                {
                    try
                    {
                        if (File.Exists(_fullpath))
                        {
                            File.Delete(_fullpath);
                        }
                    }
                    catch { }
                    _fullpath = null;
                    _pendingFileBytes = null;
                }

                ValidationMessage = null;
                DisplayFileName = "";
                ProgressPercent = 0;
                State = FileUploadState.Default;
                await OnFileRemoved.InvokeAsync();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Upload failed: {ex.Message}";
                ProgressPercent = 0;
                State = FileUploadState.ErrorsMustFix;
            }

            _isProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Invoked from JS when more than one file is dropped onto a single-file uplad zone.
    /// </summary>
    /// <returns></returns>
    [JSInvokable]
    public Task OnMultipleFilesDropped()
    {
        return ShowMultipleFilesError();
    }

    private async Task ShowMultipleFilesError()
    {
        ValidationMessage = "multiple_files";
        DisplayFileName = string.Empty;
        ProgressPercent = 0;
        State = FileUploadState.Default;
        await OnFileRemoved.InvokeAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveFile()
    {
        if (!string.IsNullOrEmpty(_fullpath))
        {
            try
            {
                if (File.Exists(_fullpath))
                {
                    File.Delete(_fullpath);
                }
            }
            catch { }
            _fullpath = null;
            _pendingFileBytes = null;
        }

        ValidationMessage = "";
        DisplayFileName = "";
        ProgressPercent = 0;
        State = FileUploadState.Default;
        await OnFileRemoved.InvokeAsync();
        StateHasChanged();
    }

    private async Task CancelUpload()
    {
        UploadState = FileUploadState.Default;
        DisplayFileName = string.Empty;
        ProgressPercent = 0;
        _cts?.Cancel();
    }

    private async Task ReUpload()
    {
        await OpenFileDialog();
    }
    private async Task DownloadFile()
    {
        byte[]? bytes = null;
        if (!string.IsNullOrEmpty(_fullpath) && File.Exists(_fullpath))
        {
            bytes = await File.ReadAllBytesAsync(_fullpath);
        }
        else if (_pendingFileBytes != null)
        {
            bytes = _pendingFileBytes;
        }

        if (bytes is null || bytes.Length == 0)
        {
            return;
        }

        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("fileUpload.downloadFileFromStream", DisplayFileName, streamRef);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected async Task OpenFileDialog()
    {
        await JS.InvokeVoidAsync("fileUpload.OpenDialog", _inPutId);
    }

    private async Task<string> UploadToServerPathProgressBar(IBrowserFile file, CancellationToken cancellationToken)
    {
        const int BufferSize = 64 * 1024;
        using var stream = file.OpenReadStream(MaxFileSizeMB, cancellationToken);
        using var ms = new MemoryStream();
        var buffer = new byte[BufferSize];
        long totalRead = 0;
        var totalLength = file.Size;

        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await ms.WriteAsync(buffer, 0, read, cancellationToken);
            totalRead += read;
            ProgressPercent = totalLength > 0 ? (int) (totalRead * 100L / totalLength) : 100;
            await InvokeAsync(StateHasChanged);
        }
        var bytes = ms.ToArray();

        //Append GUID to server-side filename for uniqueness.
        var extension = Path.GetExtension(DisplayFileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(DisplayFileName);
        var serverFileName = $"{nameWithoutExt}_{Guid.NewGuid():N}{extension}";
        var fullpath = Path.Combine(FolderPath, serverFileName);

        if (SaveOnUpload)
        {
            //Save immediately to disk
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            await File.WriteAllBytesAsync(fullpath, bytes, cancellationToken);
        }
        else
        {
            //keep in memory - will be saved via SavePendingFileAsync() on submit.
            _pendingFileBytes = bytes;
        }

        SelectedFiles.Add(file);
        await OnFileUpload.InvokeAsync(fullpath);
        return fullpath;
    }

    /// <summary>
    /// Saves the pending in-memory file to disk.
    /// returns the sever file path or null if no pending file.
    /// </summary>
    /// <returns>filepath</returns>
    public async Task<string?> SavePendingFileAsync()
    {
        if (_pendingFileBytes == null || string.IsNullOrEmpty(_fullpath))
        {
            return null;
        }

        var folder = Path.GetDirectoryName(_fullpath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        await File.WriteAllBytesAsync(_fullpath, _pendingFileBytes);
        _pendingFileBytes = null;
        return _fullpath;
    }

    /// <summary>
    /// Whether there is a file in memory waiting to be saved.
    /// </summary>
    public bool HasPendingFile => _pendingFileBytes != null;

    /// <summary>
    /// Returns the first N lines of the pending in-memory file as a single string. 
    /// </summary>
    /// <param name="lineCount"></param>
    /// <returns></returns>
    public string GetPendingFileContent(int lineCount = 5)
    {
        if (_pendingFileBytes == null || _pendingFileBytes.Length == 0)
        {
            return string.Empty;
        }

        var text = System.Text.Encoding.UTF8.GetString(_pendingFileBytes);
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Take(lineCount);
        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}
