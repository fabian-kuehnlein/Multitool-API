using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;

public class CalendarService : ICalendarService
{
	private readonly ICalendarEventRepository _repository;
	private readonly IMapper _mapper;

	public CalendarService(ICalendarEventRepository repository, IMapper mapper)
	{
		_repository = repository;
		_mapper = mapper;
	}

	public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories)
	{
		return await _repository.GetEventsByRangeAsync(start, end, categories);
	}

	public async Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchWord)
	{
		return await _repository.SearchCalendarEventsAsync(searchWord);
	}

	public async Task InsertEventAsync(CreateCalendarEvent createEvent)
	{
		await _repository.InsertEventAsync(createEvent);
	}

	public async Task UpdateEventAsync(CalendarEvent updateEvent)
	{
		await _repository.UpdateEventAsync(updateEvent);
	}

	public async Task DeleteEventAsync(int id)
	{
		await _repository.DeleteEventAsync(id);
	}

	public async Task<List<Category>> GetCategoriesAsync()
	{
		return await _repository.GetCategoriesAsync();
	}

	public async Task<List<Holiday>> GetHolidaysAsync(string year)
	{
		var holidays = await _repository.GetHolidaysAsync(year);
		return _mapper.Map<List<Holiday>>(holidays);
	}
}