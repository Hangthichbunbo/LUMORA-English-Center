using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Advertisement;
using StoryHub.Domain.Enums;

namespace StoryHub.Application.Interfaces;

public interface IAdvertisementService
{
    // Client selection
    Task<AdvertisementDto?> GetActiveAdForPlacementAsync(AdPlacement placement, int? storyId = null, string? categorySlug = null, CancellationToken cancellationToken = default);
    Task TrackAdClickAsync(int adId, CancellationToken cancellationToken = default);

    // Admin management
    Task<PaginatedList<CreateOrUpdateAdDto>> GetAdminAdsPaginatedAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<CreateOrUpdateAdDto?> GetAdForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateAdAsync(CreateOrUpdateAdDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAdAsync(CreateOrUpdateAdDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> ToggleAdStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAdAsync(int id, CancellationToken cancellationToken = default);
}
