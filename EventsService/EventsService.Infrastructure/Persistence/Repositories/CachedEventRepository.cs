using EventsService.Application.Abstractions.Persistence.Repositories;
using StackExchange.Redis;

namespace EventsService.Infrastructure.Persistence.Repositories;

public class CachedEventRepository : ICachedEventRepository
{
    private readonly IDatabase _redisDb;
    private readonly IEventRepository _repository;

    public CachedEventRepository(IConnectionMultiplexer multiplexer, IEventRepository repository)
    {
        _redisDb = multiplexer.GetDatabase();
        _repository = repository;
    }
}
