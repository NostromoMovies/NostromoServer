using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FolderWatcherController : ControllerBase
    {
    //    private readonly MultiFolderWatcher _watcher;

    //    public FolderWatcherController(MultiFolderWatcher watcher)
    //    {
    //        _watcher = watcher;
    //    }

    //    [HttpPost("add")]
    //    public IActionResult AddFolder([FromBody] FolderToWatch folder)
    //    {
    //        _watcher.AddFolder(folder.Path, folder.FileType);
    //        return Ok($"Now watching folder: {folder.Path} for {folder.FileType} files");
    //    }

    //    [HttpGet("list")]
    //    public IActionResult ListWatchedFolders()
    //    {
    //        var folders = _watcher.GetWatchedFolders();
    //        return Ok(folders);
    //    }

    //    [HttpPost("remove")]
    //    public IActionResult RemoveFolder([FromBody] string folderPath)
    //    {
    //        _watcher.RemoveFolder(folderPath);
    //        return Ok($"Stopped watching folder: {folderPath}");
    //    }
    //}

    //public class FolderToWatch
    //{
    //    public string Path { get; set; }
    //    public string FileType { get; set; }
    }
}
