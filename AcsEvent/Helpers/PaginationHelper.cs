using AcsEvent.Response;

namespace AcsEvent.Helpers;

public class PaginationHelper
{
    public static PagedResponse<List<T>> CreatePagedResponse<T>(List<T> pagedData, int pageNumber, int pageSize, int totalRecords)
    {
        var response = new PagedResponse<List<T>>(pagedData, pageNumber, pageSize);

        // Calculate total pages
        int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        response.TotalPages = totalPages;
        response.TotalRecords = totalRecords;

        return response;
    }
}