using MediatR;

namespace VirtoCommerce.CatalogQuality.Core.Common;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
