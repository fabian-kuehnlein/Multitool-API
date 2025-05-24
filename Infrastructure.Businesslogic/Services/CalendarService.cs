using AutoMapper;
using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;

public class CalendarService : ICalendarService
{
    private readonly ICalendarEventRepository _repository;
    private readonly IMapper _mapper;

    public CalendarService(ICalendarEventRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

	public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end)
	{
		return await _repository.GetEventsByRangeAsync(start, end);
    }

	public async Task InsertEventAsync(CreateCalendarEvent createEvent)
	{
		await _repository.InsertEventAsync(_mapper.Map<CreateCalendarEventDAO>(createEvent));
	}

	// public Task<CalendarEvent> UpdateEventAsync(int id, CreateCalendarEvent updateEvent)
	// {
	// 	throw new NotImplementedException();
	// }

	// public Task DeleteEventAsync(int id)
	// {
	// 	throw new NotImplementedException();
	// }

	public async Task<List<Category>> GetCategoriesAsync()
	{
        return await _repository.GetCategoriesAsync();
	}
}