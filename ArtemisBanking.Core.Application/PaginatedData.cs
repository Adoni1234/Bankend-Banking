using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Core.Application;

public class PaginatedData<T>
{
    public IEnumerable<T> Items { get; set; } // los 20 elementos de una pagina
    public Pagination Pagination { get; set; } // 

    public PaginatedData(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
    {
        Items = items;
        Pagination = new Pagination(totalCount, currentPage, pageSize);
    }

    public PaginatedData(IEnumerable<T> items, Pagination pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    // Para consultas a la DB.
    public static async Task<PaginatedData<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedData<T>(items, totalCount, pageNumber, pageSize);
    }
    
    
    // Para colecciones ya cargadas en memoria.
    public static PaginatedData<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedData<T>(items, totalCount, pageNumber, pageSize);
    }
}

public class Pagination
{
    public int TotalCount { get; set; } // 100 elementos
    public int CurrentPage { get; set; } // 
    public int TotalPages { get; set; } // 5 paginas de 20 elementtos
    public int PageSize { get; set; } // 20 elementos

    public Pagination(int totalCount, int currentPage, int pageSize)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public Pagination() { }
}
