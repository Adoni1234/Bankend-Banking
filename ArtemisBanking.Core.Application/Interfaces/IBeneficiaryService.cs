using ArtemisBanking.Core.Application.Dtos.Beneficiary;

namespace ArtemisBanking.Core.Application.Interfaces;

public interface IBeneficiaryService : IGenericService<int, BeneficiaryDto>
{
    Task<List<BeneficiaryDto>> GetByUserIdAsync(string userId);
    Task AddAsync(string userId, string savingAccountId);

}