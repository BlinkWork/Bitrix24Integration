namespace Bitrix24Website.Models
{
    public class PaginationResult<TEntity> (int pageIndex, int count, IEnumerable<TEntity> data) where TEntity : class
    {
        public int PageIndex { get; } = pageIndex;
        public int Count { get; } = count;
        public IEnumerable<TEntity> Data { get; } = data;
    }
}
