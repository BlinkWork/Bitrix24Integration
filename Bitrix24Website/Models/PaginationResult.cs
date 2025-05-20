namespace Bitrix24Website.Models
{
    public class PaginationResult<TEntity> (int pageIndex, int count, IEnumerable<TEntity> data, Boolean success) where TEntity : class
    {
        public Boolean Success { get; } = success;
        public int PageIndex { get; } = pageIndex;
        public int Count { get; } = count;
        public IEnumerable<TEntity> Data { get; } = data;
    }
}
