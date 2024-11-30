﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Services;

[ApiController]
[Route("api/folders")]
public class FolderController : ControllerBase
{
    private readonly ILogger<FolderController> _logger;
    private readonly IImportFolderManager _folderManager;

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
}