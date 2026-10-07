using MediatR;

namespace VirtoCommerce.CatalogQuality.Core.Common;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
}
