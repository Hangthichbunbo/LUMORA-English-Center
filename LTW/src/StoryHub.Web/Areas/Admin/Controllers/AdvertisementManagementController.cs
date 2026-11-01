using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.DTOs.Advertisement;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdvertisementManagementController : Controller
{
    private readonly IAdvertisementService _adService;
    private readonly IFileStorageService _fileStorageService;

    public AdvertisementManagementController(IAdvertisementService adService, IFileStorageService fileStorageService)
    {
        _adService = adService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var ads = await _adService.GetAdminAdsPaginatedAsync(pageIndex, 20, cancellationToken);
        return View(ads);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateOrUpdateAdDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrUpdateAdDto dto, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            if (imageFile.Length > 1 * 1024 * 1024)
            {
                ModelState.AddModelError("imageFile", $"Dung lượng ảnh quảng cáo ({imageFile.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa 1 MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
            else
            {
                try
                {
                    using var stream = imageFile.OpenReadStream();
                    dto.ImageUrl = await _fileStorageService.SaveFileAsync(stream, imageFile.FileName, "ads", cancellationToken);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi tải ảnh quảng cáo: " + ex.Message);
                }
            }
        }

        if (!ModelState.IsValid) return View(dto);

        var result = await _adService.CreateAdAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            return View(dto);
        }

        TempData["SuccessMessage"] = "Tạo chiến dịch quảng cáo thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ad = await _adService.GetAdForEditAsync(id, cancellationToken);
        if (ad == null) return NotFound();
        return View(ad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreateOrUpdateAdDto dto, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        string? oldUrl = null;
        string? newUrl = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            if (imageFile.Length > 1 * 1024 * 1024)
            {
                ModelState.AddModelError("imageFile", $"Dung lượng ảnh quảng cáo ({imageFile.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa 1 MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
            else
            {
                try
                {
                    var existingAd = await _adService.GetAdForEditAsync(dto.Id, cancellationToken);
                    if (existingAd != null && !string.IsNullOrWhiteSpace(existingAd.ImageUrl))
                    {
                        oldUrl = existingAd.ImageUrl;
                    }

                    using var stream = imageFile.OpenReadStream();
                    newUrl = await _fileStorageService.SaveFileAsync(stream, imageFile.FileName, "ads", cancellationToken);
                    dto.ImageUrl = newUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi cập nhật ảnh quảng cáo: " + ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(newUrl))
            {
                await _fileStorageService.DeleteFileAsync(newUrl, cancellationToken);
            }
            return View(dto);
        }

        var result = await _adService.UpdateAdAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(newUrl))
            {
                await _fileStorageService.DeleteFileAsync(newUrl, cancellationToken);
            }
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            return View(dto);
        }

        if (!string.IsNullOrWhiteSpace(newUrl) && !string.IsNullOrWhiteSpace(oldUrl) && oldUrl != newUrl)
        {
            await _fileStorageService.DeleteFileAsync(oldUrl, cancellationToken);
        }

        TempData["SuccessMessage"] = "Cập nhật quảng cáo thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var result = await _adService.ToggleAdStatusAsync(id, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ad = await _adService.GetAdForEditAsync(id, cancellationToken);
        var result = await _adService.DeleteAdAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            if (ad != null && !string.IsNullOrWhiteSpace(ad.ImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(ad.ImageUrl, cancellationToken);
            }
            TempData["SuccessMessage"] = result.Message;
        }
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
