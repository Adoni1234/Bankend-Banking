using ArtemisBanking.Core.Application.Dtos.Beneficiary;
using ArtemisBanking.Core.Application.Dtos.CardTransaction;
using ArtemisBanking.Core.Application.Dtos.Commerce;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Common.Enums;
using ArtemisBanking.Core.Domain.Entities;
using ArtemisBanking.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application.Services;

public class CommerceService : GenericServices<int, Commerce, CommerceDto>, ICommerceService 
{
    private readonly ICommerceRepository _commerceRepository;
    private readonly IMapper _mapper;
    private readonly IBaseAccountService _accountService;
    
    public CommerceService(ICommerceRepository repository, IMapper mapper, IBaseAccountService accountService) : base(repository, mapper)
    {
        _commerceRepository = repository;
        _mapper = mapper;
        _accountService = accountService;
    }

    public async Task<Result<int>> GetCommerceIdByUserIdAsync(string userId)
    {
        var user = await _accountService.GetUserById(userId);
        
        if (user == null)
        {
            return Result<int>.Fail("No se encontró el usuario.");
        }
        
        if (!user.CommerceId.HasValue)
        {
            return Result<int>.Fail("El usuario no está asociado a un comercio.");
        }
        
        return Result<int>.Ok(user.CommerceId.Value);
    }

    public async Task<Result<PaginatedData<CommerceDto>>> GetCommercesPaginatedAsync(int page, int pageSize)
    {
        var query = _commerceRepository.GetAllQueryable()
            .AsNoTracking()
            .OrderByDescending(c => c.Id)
            .Select(c => _mapper.Map<CommerceDto>(c));

        var paginatedData = await PaginatedData<CommerceDto>.CreateAsync(query, page, pageSize);
        return Result<PaginatedData<CommerceDto>>.Ok(paginatedData);
    }

    public async Task<Result<List<CommerceDto>>> GetAllActiveCommercesAsync()
    {
        var commerces = await _commerceRepository.GetAllQueryable()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        var commerceDtos = _mapper.Map<List<CommerceDto>>(commerces);
        return Result<List<CommerceDto>>.Ok(commerceDtos);
    }

    public async Task<Result> ChangeStatusAsync(int id, bool status)
    {
        var commerce = await _commerceRepository.GetByIdAsync(id);
        
        if (commerce == null)
        {
            return Result.Fail("No se encontró el comercio.");
        }

        commerce.IsActive = status;
        await _commerceRepository.UpdateAsync(id, commerce);

        // Si se desactiva el comercio, desactivar todos los usuarios asociados
        if (!status)
        {
            var usersResult = await _accountService.GetAllUserOfRole(Roles.Commerce, isActive: true);
            if (usersResult.IsSuccess)
            {
                var usersToDeactivate = usersResult.Value
                    .Where(u => u.CommerceId == id)
                    .ToList();

                foreach (var user in usersToDeactivate)
                {
                    await _accountService.SetStateOnUser(user.Id, false);
                }
            }
        }
        // Nota: Si se reactiva el comercio, los usuarios permanecen inactivos
        // y deben hacer reset de contraseña para activarse (según requerimientos)

        return Result.Ok();
    }
}