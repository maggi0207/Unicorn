using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
using static UI.EmployerPortal.Web.Features.EmployerRegistration.Components.PreliminaryQuestions;
namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Components;


/// <summary>
/// Fike Upload Parameters
/// </summary>
public partial class FileUpload<TModel> : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private readonly string _inPutId = "fileUpload_" + Guid.NewGuid().ToString();
    private readonly string _dropZoneId = "DropZone_" + Guid.NewGuid().ToString();
    private InputFile? _inputFiles;
    private CancellationTokenSource? _cts;
    //private List<FileUploadService> _fileUploadServices = [];
    private string? _fullpath;
    /// <summary>
    /// 
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
    public string FolderPath { get; set; } = "C:\\Data\\UI\\Tax\\SUITES\\EmployerPortalUpload\\Registration\\";
    /// <summary>
    /// Acceptable File Types
    /// </summary>
    [Parameter]
    public List<KeyValuePair<string, string>> AcceptableTypes { get; set; } = new()
    {
            new KeyValuePair<string, string>(".PDF","Portable Document Format"),
            new KeyValuePair<string, string>(".JPG","Joint Photographic Experts Group"),
            new KeyValuePair<string, string>(".HEIC","High Effeciency Image File Format"),
            new KeyValuePair<string, string>(".PNG"," Portable Network Graphics")
    };
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
    [Parameter] public EventCallback OnFileRemoved { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public TModel? Model { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter] public FileType? SelFileType { get; set; }

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
    public string FileMimeType { get; set; } = string.Empty;
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
    protected override async Task OnParametersSetAsync()
    {
        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = rulingDoc?.RulingDocFilename ?? "";
                        UploadState = rulingDoc?.RulingDocUploadState ?? FileUploadState.Default;
                        FileMimeType = rulingDoc?.RulingDocFileMIMEType ?? "";
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        DisplayFileName = objIrs?.IRSAcceptanceLetterFilename ?? "";
                        UploadState = objIrs?.IRSAcceptanceLetterUploadState ?? FileUploadState.Default;
                        FileMimeType = objIrs?.IRSAcceptanceLetterFileMIMEType ?? "";
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = inc?.ArticlesOfIncorporationFilename ?? "";
                        UploadState = inc?.ArticlesOfIncorporationUploadState ?? FileUploadState.Default;
                        FileMimeType = inc?.ArticlesOfIncorporationFileMIMEType ?? "";
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        DisplayFileName = gov?.Filepath ?? "";
                        UploadState = gov?.FileUploadState ?? FileUploadState.Default;
                        FileMimeType = gov?.FileMIMEType ?? "";
                        break;
                    default:
                        break;
                }
            }


            State = UploadState;
            //await InvokeAsync(StateHasChanged);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fistRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool fistRender)
    {
        if (fistRender)
        {
            await JS.InvokeAsync<IJSObjectReference>("fileUpload.initDropZone", _dropZoneId, _inPutId);

        }
    }
    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        ValidationMessage = null;
        var files = AllowMultiple ? e.GetMultipleFiles() : new[] { e.File };
        SelectedFiles.Clear();
        foreach (var file in files)
        {

            if (file == null)
            {
                return;
            }
            //Size Validation
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
                var ext = Path.GetExtension(file.Name) ?? string.Empty;
                var allowedext = AcceptableTypes.
                    Select(t =>
                    {
                        var key = t.Key.ToLowerInvariant();
                        return key;
                    }).ToHashSet();
                if (!allowedext.Contains(ext))
                {
                    ValidationMessage = $"File is not correct type.";
                    CurrentFile = file;
                    ProgressPercent = 0;
                    State = FileUploadState.ErrorsMustFix;
                    await InvokeAsync(StateHasChanged);
                    return;

                }
            }
            //Start Uploading
            State = FileUploadState.Uploading;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            //await InvokeAsync(StateHasChanged);
            DisplayFileName = file.Name;
            if (Model != null)
            {
                if (SelFileType.HasValue)
                {
                    switch (SelFileType.Value)
                    {
                        case FileType.Ruling:
                            var rulingDoc = Model as PreliminaryQuestionsModel;
                            rulingDoc?.RulingDocFilename = file.Name;
                            rulingDoc?.RulingDocFileMIMEType = file.ContentType;
                            break;
                        case FileType.IRS:
                            var objIrs = Model as PreliminaryQuestionsModel;
                            objIrs?.IRSAcceptanceLetterFilename = file.Name;
                            objIrs?.IRSAcceptanceLetterFileMIMEType = file.ContentType;
                            break;
                        case FileType.INC:
                            var inc = Model as PreliminaryQuestionsModel;
                            inc?.ArticlesOfIncorporationFilename = file.Name;
                            inc?.ArticlesOfIncorporationFileMIMEType = file.ContentType;
                            break;
                        case FileType.GOV:
                            var gov = Model as OwnershipAgency;
                            gov?.Filepath = file.Name;
                            gov?.FileMIMEType = file.ContentType;
                            break;
                        default:
                            break;
                    }
                }
            }

            try
            {
                _fullpath = await UploadToServerPathProgressbar(file, _cts.Token);
                if (!_cts.IsCancellationRequested)
                {
                    ProgressPercent = 100;
                    State = FileUploadState.Successful;
                    if (Model != null)
                    {
                        if (SelFileType.HasValue)
                        {
                            switch (SelFileType.Value)
                            {
                                case FileType.Ruling:
                                    var rulingDoc = Model as PreliminaryQuestionsModel;
                                    rulingDoc?.RulingDocUploadState = FileUploadState.Successful;
                                    break;
                                case FileType.IRS:
                                    var objIrs = Model as PreliminaryQuestionsModel;
                                    objIrs?.IRSAcceptanceLetterUploadState = FileUploadState.Successful;
                                    break;
                                case FileType.INC:
                                    var inc = Model as PreliminaryQuestionsModel;
                                    inc?.ArticlesOfIncorporationUploadState = FileUploadState.Successful;
                                    break;
                                case FileType.GOV:
                                    var gov = Model as OwnershipAgency;
                                    gov?.FileUploadState = FileUploadState.Successful;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (OperationCanceledException)
            {
                ValidationMessage = "Upload Cancelled.";
                ProgressPercent = 0;
                State = FileUploadState.ErrorsMustFix;
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Upload failed: {ex.Message}";
                ProgressPercent = 0;
                State = FileUploadState.ErrorsMustFix;
            }
            await InvokeAsync(StateHasChanged);
        }

    }
    private async Task RemoveFile()
    {
        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = rulingDoc?.RulingDocFilename ?? "";
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        DisplayFileName = objIrs?.IRSAcceptanceLetterFilename ?? "";
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = inc?.ArticlesOfIncorporationFilename ?? "";
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        DisplayFileName = gov?.Filepath ?? "";
                        break;
                    default:
                        break;
                }
            }
        }

        var filepath = Path.Combine(FolderPath, DisplayFileName);
        try
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            DisplayFileName = "";
            ProgressPercent = 0;
            ValidationMessage = "";
            StateHasChanged();
            State = FileUploadState.Default;

            if (Model != null)
            {
                if (SelFileType.HasValue)
                {
                    switch (SelFileType.Value)
                    {
                        case FileType.Ruling:
                            var rulingDoc = Model as PreliminaryQuestionsModel;
                            rulingDoc?.RulingDocFilename = "";
                            rulingDoc?.RulingDocUploadState = FileUploadState.Default;
                            break;
                        case FileType.IRS:
                            var objIrs = Model as PreliminaryQuestionsModel;
                            objIrs?.IRSAcceptanceLetterFilename = "";
                            objIrs?.IRSAcceptanceLetterUploadState = FileUploadState.Default;
                            break;
                        case FileType.INC:
                            var inc = Model as PreliminaryQuestionsModel;
                            inc?.ArticlesOfIncorporationFilename = "";
                            inc?.ArticlesOfIncorporationUploadState = FileUploadState.Default;
                            break;
                        case FileType.GOV:
                            var gov = Model as OwnershipAgency;
                            gov?.Filepath = "";
                            gov?.FileUploadState = FileUploadState.Default;
                            break;
                        default:
                            break;
                    }
                }
            }

        }
        catch (Exception ex)
        {
            ValidationMessage = ex.Message;
            State = FileUploadState.ErrorsMustFix;
        }


    }
    private async Task CancelUpload()
    {
        UploadState = FileUploadState.Default;
        DisplayFileName = string.Empty;

        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        rulingDoc?.RulingDocFilename = "";
                        rulingDoc?.RulingDocUploadState = FileUploadState.Default;
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        objIrs?.IRSAcceptanceLetterFilename = "";
                        objIrs?.IRSAcceptanceLetterUploadState = FileUploadState.Default;
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        inc?.ArticlesOfIncorporationFilename = "";
                        inc?.ArticlesOfIncorporationUploadState = FileUploadState.Default;
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        gov?.Filepath = "";
                        gov?.FileUploadState = FileUploadState.Default;
                        break;
                    default:
                        break;
                }
            }
        }

        ProgressPercent = 0;
        _cts?.Cancel();

    }
    private async Task ReUpload()
    {

        DisplayFileName = "";
        ProgressPercent = 0;
        ValidationMessage = "";

        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        rulingDoc?.RulingDocFilename = "";
                        rulingDoc?.RulingDocUploadState = FileUploadState.Default;
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        objIrs?.IRSAcceptanceLetterFilename = "";
                        objIrs?.IRSAcceptanceLetterUploadState = FileUploadState.Default;
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        inc?.ArticlesOfIncorporationFilename = "";
                        inc?.ArticlesOfIncorporationUploadState = FileUploadState.Default;
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        gov?.Filepath = "";
                        gov?.FileUploadState = FileUploadState.Default;
                        break;
                    default:
                        break;
                }
            }
        }

        StateHasChanged();
        State = FileUploadState.Default;


    }
    private async Task DownloadFile()
    {

        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = rulingDoc?.RulingDocFilename ?? "";
                        FileMimeType = rulingDoc?.RulingDocFileMIMEType ?? "";
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        DisplayFileName = objIrs?.IRSAcceptanceLetterFilename ?? "";
                        FileMimeType = objIrs?.IRSAcceptanceLetterFileMIMEType ?? "";
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = inc?.ArticlesOfIncorporationFilename ?? "";
                        FileMimeType = inc?.ArticlesOfIncorporationFileMIMEType ?? "";
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        DisplayFileName = gov?.Filepath ?? "";
                        FileMimeType = gov?.FileMIMEType ?? "";
                        break;
                    default:
                        break;
                }
            }
        }

        var fileurl = Path.Combine(FolderPath, DisplayFileName, "");

        var content = await File.ReadAllBytesAsync(fileurl);
        var base64 = Convert.ToBase64String(content);
        await JS.InvokeVoidAsync("fileUpload.downloadFile", base64, FileMimeType, DisplayFileName);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>

    protected async Task OpenFileDialog()
    {
        await JS.InvokeVoidAsync("fileUpload.OpenDialog", _inPutId);
    }
    /// <summary>
    /// Upload is method
    /// </summary>
    /// <returns></returns>
    private async Task<string> UploadToServerPathProgressbar(IBrowserFile file, CancellationToken cancellationToken)
    {
        const int BufferSize = 64 * 1024;
        using var stream = file.OpenReadStream(MaxFileSizeMB, cancellationToken);
        using var ms = new MemoryStream();
        var buffer = new byte[BufferSize];
        long totalRead = 10;
        var totalLength = file.Size;

        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await ms.WriteAsync(buffer, 0, read, cancellationToken);
            totalRead += read;
            ProgressPercent = (int) (totalRead * 100L / totalLength);

            await InvokeAsync(StateHasChanged);
            await Task.Delay(50, cancellationToken);
        }
        //for (var i = 1; i <= totalRead; i++)
        //{
        //    ProgressPercent = (i * 100);

        //    await InvokeAsync(StateHasChanged);
        //    await Task.Delay(100, cancellationToken);
        //}
        var bytes = ms.ToArray();
        //Add file to folder
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        if (Model != null)
        {
            if (SelFileType.HasValue)
            {
                switch (SelFileType.Value)
                {
                    case FileType.Ruling:
                        var rulingDoc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = rulingDoc?.RulingDocFilename ?? "";
                        break;
                    case FileType.IRS:
                        var objIrs = Model as PreliminaryQuestionsModel;
                        DisplayFileName = objIrs?.IRSAcceptanceLetterFilename ?? "";
                        break;
                    case FileType.INC:
                        var inc = Model as PreliminaryQuestionsModel;
                        DisplayFileName = inc?.ArticlesOfIncorporationFilename ?? "";
                        break;
                    case FileType.GOV:
                        var gov = Model as OwnershipAgency;
                        DisplayFileName = gov?.Filepath ?? "";
                        break;
                    default:
                        break;
                }
            }
        }

        var fullpath = Path.Combine(FolderPath, DisplayFileName);
        await File.WriteAllBytesAsync(fullpath, bytes, cancellationToken);
        SelectedFiles.Add(file);
        //_fileUploadServices = await UploadServices.LoadFileWcf(fullpath);
        await OnFileUpload.InvokeAsync(fullpath);
        return fullpath;
    }

}

