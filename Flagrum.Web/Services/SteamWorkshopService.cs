using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Flagrum.Archiver.Binmod.Data;
using Steamworks;

namespace Flagrum.Web.Services;

public class SteamWorkshopService
{
    private const uint AppId = 637650;

    private readonly JSInterop _interop;
    private readonly AppId_t _appId;
    private Binmod _activeMod;
    private bool _isInitialized;
    private Action _onCreate;
    private Action _onUpdate;
    private string _tempPreview;
    private string _tempDat;
    private string _tempBinmod;
    private string _tempDirectory;
    private CallResult<CreateItemResult_t> _createItemCallback;
    private CallResult<SubmitItemUpdateResult_t> _submitItemUpdateCallback;
    private Timer _timer;

    public SteamWorkshopService(JSInterop interop)
    {
        _interop = interop;
        _appId = new AppId_t(AppId);
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => SteamAPI.RunCallbacks();
    }

    public bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        var success = SteamAPI.Init();

        if (!success)
        {
            return false;
        }

        _isInitialized = true;
        return true;
    }

    public void Publish(Binmod mod, Action onComplete)
    {
        _activeMod = mod;
        _onCreate = onComplete;

        _timer.Start();
        _createItemCallback = CallResult<CreateItemResult_t>.Create(OnCreate);
        var call = SteamUGC.CreateItem(_appId, EWorkshopFileType.k_EWorkshopFileTypeFirst);
        _createItemCallback.Set(call);
    }

    public void Update(Binmod mod, string changeNote, Action onComplete)
    {
        File.AppendAllText("C:\\Testing\\log.txt", $"Update called!\r\n");
        _activeMod = mod;
        _onUpdate = onComplete;

        if (mod.PreviewBytes.Length > 953673)
        {
            File.AppendAllText("C:\\Testing\\log.txt", $"Preview file too big!\r\n");
            // TODO: Show alert to user
            return;
        }

        var fileId = new PublishedFileId_t(mod.ItemId);
        var updateHandle = SteamUGC.StartItemUpdate(_appId, fileId);

        SteamUGC.SetItemTitle(updateHandle, mod.WorkshopTitle);
        SteamUGC.SetItemDescription(updateHandle, mod.Description);
        SteamUGC.SetItemVisibility(updateHandle, (ERemoteStoragePublishedFileVisibility)mod.Visibility);

        _tempPreview = Path.GetTempFileName();
        File.WriteAllBytes(_tempPreview, mod.PreviewBytes);
        SteamUGC.SetItemPreview(updateHandle, _tempPreview);

        if (mod.Tags.Count > 0)
        {
            SteamUGC.SetItemTags(updateHandle, mod.Tags);
        }

        _tempDirectory = $"{Path.GetTempPath()}\\{mod.Uuid}";
        Directory.CreateDirectory(_tempDirectory);
        _tempBinmod = $"{_tempDirectory}\\{Path.GetFileName(mod.Path)}";
        File.Copy(mod.Path, _tempBinmod);

        // dat file doesn't appear to have any content
        _tempDat = $"{_tempDirectory}\\{mod.ItemId}.dat";
        File.Create(_tempDat);

        SteamUGC.SetItemContent(updateHandle, _tempDirectory);

        _timer.Start();
        _submitItemUpdateCallback = CallResult<SubmitItemUpdateResult_t>.Create(OnUpdate);
        var call = SteamUGC.SubmitItemUpdate(updateHandle, changeNote);
        _submitItemUpdateCallback.Set(call);
    }

    private void OnCreate(CreateItemResult_t result, bool error)
    {
        _timer.Stop();
        File.AppendAllText("C:\\Testing\\log.txt", $"OnCreate called with result {result.m_eResult}\r\n");
        
        if (result.m_eResult == EResult.k_EResultOK)
        {
            _activeMod.IsUploaded = true;
            _activeMod.ItemId = result.m_nPublishedFileId.m_PublishedFileId;
            _activeMod.Visibility =
                (int)ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
            // TODO: Save changes to the modmeta

            Update(_activeMod, "", _onCreate);
        }
    }

    private void OnUpdate(SubmitItemUpdateResult_t result, bool error)
    {
        _timer.Stop();
        File.AppendAllText("C:\\Testing\\log.txt", $"OnUpdate called with result {result.m_eResult}\r\n");
        
        if (result.m_eResult == EResult.k_EResultOK)
        {
            _activeMod.LastUpdated = DateTime.Now;
            _onUpdate();
            
            File.Delete(_tempPreview);
            File.Delete(_tempBinmod);
            File.Delete(_tempDat);
            Directory.Delete(_tempDirectory);
        }
    }
}