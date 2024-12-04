﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;

[ApiController]
[Route("api/folders")]
public class FolderController : ControllerBase
{
    private readonly ILogger<FolderController> _logger;
    private readonly IImportFolderManager _folderManager;

    private static readonly HashSet<string> _excludedFormats =
[
        "msdos", 
        "ramfs",
        "configfs",
        "fusectl",
        "tracefs",
        "hugetlbfs",
        "mqueue",
        "debugfs",
        "binfmt_misc",
        "devpts",
        "pstorefs",
        "bpf_fs",
        "cgroup2fs",
        "securityfs",
        "proc",
        "tmpfs",
        "sysfs",
    ];

    public FolderController(
        ILogger<FolderController> logger,
        IImportFolderManager folderManager)
    {
        _logger = logger;
        _folderManager = folderManager;
    }

    [HttpGet("set")]
    public async Task<IActionResult> SetFolder([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Folder path is required.");
        }

        var result = await _folderManager.AddImportFolderAsync(path);
        if (!result)
        {
            return StatusCode(500, "Failed to add folder.");
        }

        return Ok($"Folder added successfully: {path}");
    }

    [HttpGet("test")]
    public IActionResult TestEndpoint()
    {
        return Ok("Test endpoint hit!");
    }

    [HttpGet("remove")]
    public async Task<IActionResult> RemoveFolder([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Folder path is required.");
        }

        var result = await _folderManager.RemoveImportFolderAsync(path);
        if (!result)
        {
            return NotFound($"Folder not found or could not be removed: {path}");
        }

        return Ok($"Folder removed successfully: {path}");
    }

    [HttpGet("Drives")]
    public ActionResult<IEnumerable<Drive>> GetMountPoints()
    {
        return DriveInfo.GetDrives()
            .Select(d =>
            {
                if (d.DriveType == DriveType.Unknown)
                    return null;

                string fullName;
                try
                {
                    fullName = d.RootDirectory.FullName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while trying to get the full name of the drive: {ex}", ex.Message);
                    return null;
                }

                string driveFormat;
                try
                {
                    driveFormat = d.DriveFormat;
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred while trying to get the drive format of the drive: {ex}", ex.Message);
                    return null;
                }

                foreach (var format in _excludedFormats)
                {
                    if (driveFormat == format)
                        return null;
                }

                FoldersAndFiles childItems = null;
                try
                {
                    childItems = d.IsReady
                        ? new FoldersAndFiles()
                        {
                            Files = d.RootDirectory.GetFiles()?.Length ?? 0,
                            Folders = d.RootDirectory.GetDirectories()?.Length ?? 0,
                        }
                        : null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while trying to get the child items of the drive: {ex}", ex.Message);
                }

                return new Drive()
                {
                    Path = fullName,
                    IsAccessible = childItems != null,
                    Sizes = childItems,
                    Type = d.DriveType,
                };
            })
            .Where(mountPoint => mountPoint != null)
            .OrderBy(mountPoint => mountPoint.Path)
            .ToList();
    }

    [HttpGet]
    public ActionResult<IEnumerable<Folder>> GetFolder([FromQuery] string path)
    {
        if (!Directory.Exists(path))
        {
            return NotFound("Directory not found");
        }

        var root = new DirectoryInfo(path);
        return root.GetDirectories()
            .Select(dir =>
            {
                FoldersAndFiles childItems = null;
                try
                {
                    childItems = new FoldersAndFiles()
                    {
                        Files = dir.GetFiles()?.Length ?? 0,
                        Folders = dir.GetDirectories()?.Length ?? 0
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred while trying to get the child items of the directory: {ex}", ex.Message);
                }

                return new Folder() { Path = dir.FullName, IsAccessible = childItems != null, Sizes = childItems };
            })
            .OrderBy(folder => folder.Path)
            .ToList();
    }
}