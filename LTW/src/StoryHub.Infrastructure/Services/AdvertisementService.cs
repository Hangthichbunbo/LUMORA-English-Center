using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Advertisement;
using StoryHub.Domain.Entities;
using StoryHub.Domain.Enums;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class AdvertisementService : IAdvertisementService
{
    private readonly StoryHubDbContext _context;
    private readonly IHtmlSanitizationService _htmlSanitizer;
    private readonly ICurrentUserService _currentUserService;

    public AdvertisementService(
        StoryHubDbContext context,
        IHtmlSanitizationService htmlSanitizer,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _htmlSanitizer = htmlSanitizer;
        _currentUserService = currentUserService;
    }

    public async Task<AdvertisementDto?> GetActiveAdForPlacementAsync(
        AdPlacement placement,
        int? storyId = null,
        string? categorySlug = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var query = _context.Advertisements
            .AsNoTracking()
            .Where(a => a.IsEnabled && a.Placement == placement && a.StartAt <= now && a.EndAt >= now);

        if (storyId.HasValue)
        {
            query = query.Where(a => a.TargetStoryId == null || a.TargetStoryId == storyId.Value);
        }

        if (!string.IsNullOrEmpty(categorySlug))
        {
            query = query.Where(a => a.TargetCategorySlug == null || a.TargetCategorySlug == categorySlug);
        }

        var candidateAds = await query
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (candidateAds.Count == 0) return null;

        // Group by highest priority and pick randomly among top priority candidates
        var highestPriority = candidateAds[0].Priority;
        var topAds = candidateAds.Where(a => a.Priority == highestPriority).ToList();
        var selectedAd = topAds[Random.Shared.Next(topAds.Count)];

        // Increment Impression count asynchronously
        try
        {
            await _context.Advertisements
                .Where(a => a.Id == selectedAd.Id)
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.ImpressionCount, x => x.ImpressionCount + 1), cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var tracked = await _context.Advertisements.FirstOrDefaultAsync(a => a.Id == selectedAd.Id, cancellationToken);
            if (tracked != null)
            {
                tracked.ImpressionCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return new AdvertisementDto
        {
            Id = selectedAd.Id,
            Name = selectedAd.Name,
            AdType = selectedAd.AdType,
            Placement = selectedAd.Placement,
            ImageUrl = selectedAd.ImageUrl,
            DestinationUrl = selectedAd.DestinationUrl,
            HtmlContent = selectedAd.AdType == AdType.SafeHtml ? _htmlSanitizer.SanitizeHtml(selectedAd.HtmlContent) : selectedAd.HtmlContent,
            OpenInNewTab = selectedAd.OpenInNewTab,
            Priority = selectedAd.Priority
        };
    }

    public async Task TrackAdClickAsync(int adId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Advertisements
                .Where(a => a.Id == adId)
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.ClickCount, x => x.ClickCount + 1), cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var ad = await _context.Advertisements.FirstOrDefaultAsync(a => a.Id == adId, cancellationToken);
            if (ad != null)
            {
                ad.ClickCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền quản lý quảng cáo.");
        }
    }

    public async Task<PaginatedList<CreateOrUpdateAdDto>> GetAdminAdsPaginatedAsync(
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var query = _context.Advertisements
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new CreateOrUpdateAdDto
            {
                Id = a.Id,
                Name = a.Name,
                AdType = a.AdType,
                Placement = a.Placement,
                ImageUrl = a.ImageUrl,
                DestinationUrl = a.DestinationUrl,
                HtmlContent = a.HtmlContent,
                StartAt = a.StartAt,
                EndAt = a.EndAt,
                Priority = a.Priority,
                IsEnabled = a.IsEnabled,
                OpenInNewTab = a.OpenInNewTab,
                TargetStoryId = a.TargetStoryId,
                TargetCategorySlug = a.TargetCategorySlug,
                RowVersion = a.RowVersion
            });

        return await PaginatedList<CreateOrUpdateAdDto>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
    }

    public async Task<CreateOrUpdateAdDto?> GetAdForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var ad = await _context.Advertisements
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (ad == null) return null;

        return new CreateOrUpdateAdDto
        {
            Id = ad.Id,
            Name = ad.Name,
            AdType = ad.AdType,
            Placement = ad.Placement,
            ImageUrl = ad.ImageUrl,
            DestinationUrl = ad.DestinationUrl,
            HtmlContent = ad.HtmlContent,
            StartAt = ad.StartAt,
            EndAt = ad.EndAt,
            Priority = ad.Priority,
            IsEnabled = ad.IsEnabled,
            OpenInNewTab = ad.OpenInNewTab,
            TargetStoryId = ad.TargetStoryId,
            TargetCategorySlug = ad.TargetCategorySlug,
            RowVersion = ad.RowVersion
        };
    }

    public async Task<ServiceResult<int>> CreateAdAsync(CreateOrUpdateAdDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var ad = new Advertisement
        {
            Name = dto.Name.Trim(),
            AdType = dto.AdType,
            Placement = dto.Placement,
            ImageUrl = dto.ImageUrl,
            DestinationUrl = dto.DestinationUrl?.Trim(),
            HtmlContent = dto.AdType == AdType.SafeHtml ? _htmlSanitizer.SanitizeHtml(dto.HtmlContent) : dto.HtmlContent,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            Priority = dto.Priority,
            IsEnabled = dto.IsEnabled,
            OpenInNewTab = dto.OpenInNewTab,
            TargetStoryId = dto.TargetStoryId,
            TargetCategorySlug = dto.TargetCategorySlug,
            CreatedBy = _currentUserService.UserId ?? "admin",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Advertisements.Add(ad);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<int>.Success(ad.Id, "Tạo quảng cáo thành công.");
    }

    public async Task<ServiceResult> UpdateAdAsync(CreateOrUpdateAdDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var ad = await _context.Advertisements.FindAsync([dto.Id], cancellationToken);
        if (ad == null) return ServiceResult.Failure("Không tìm thấy quảng cáo.");

        _context.Entry(ad).Property(a => a.RowVersion).OriginalValue = dto.RowVersion;

        ad.Name = dto.Name.Trim();
        ad.AdType = dto.AdType;
        ad.Placement = dto.Placement;
        ad.ImageUrl = dto.ImageUrl;
        ad.DestinationUrl = dto.DestinationUrl?.Trim();
        ad.HtmlContent = dto.AdType == AdType.SafeHtml ? _htmlSanitizer.SanitizeHtml(dto.HtmlContent) : dto.HtmlContent;
        ad.StartAt = dto.StartAt;
        ad.EndAt = dto.EndAt;
        ad.Priority = dto.Priority;
        ad.IsEnabled = dto.IsEnabled;
        ad.OpenInNewTab = dto.OpenInNewTab;
        ad.TargetStoryId = dto.TargetStoryId;
        ad.TargetCategorySlug = dto.TargetCategorySlug;
        ad.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Cập nhật quảng cáo thành công.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("Quảng cáo đã bị thay đổi trước đó. Vui lòng tải lại trang.");
        }
    }

    public async Task<ServiceResult> ToggleAdStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var ad = await _context.Advertisements.FindAsync([id], cancellationToken);
        if (ad == null) return ServiceResult.Failure("Không tìm thấy quảng cáo.");

        ad.IsEnabled = !ad.IsEnabled;
        ad.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success($"Đã {(ad.IsEnabled ? "bật" : "tắt")} quảng cáo '{ad.Name}'.");
    }

    public async Task<ServiceResult> DeleteAdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var ad = await _context.Advertisements.FindAsync([id], cancellationToken);
        if (ad == null) return ServiceResult.Failure("Không tìm thấy quảng cáo.");

        _context.Advertisements.Remove(ad);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Đã xóa quảng cáo.");
    }
}
