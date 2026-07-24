using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Services;

public class IdentifierService : IIdentifierService
{
    private readonly ArtemisContext _context;

    public IdentifierService(ArtemisContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateIdentifier()
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR dbo.SeqLoanAndAccountNumbersID";

        var result = await command.ExecuteScalarAsync();

        await connection.CloseAsync(); // 

        return ((long)result).ToString("D9");
    }


    public async Task<string> GenerateCreditCardNumber()
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR SeqCreditCardsID";

        var result = await command.ExecuteScalarAsync();

        await connection.CloseAsync(); // 

        return ((long)result).ToString("D16");
    }

}