using ArtemisBanking.Core.Application.Dtos.Dashboard;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardAdminDto> GetAdminDashboard();
}