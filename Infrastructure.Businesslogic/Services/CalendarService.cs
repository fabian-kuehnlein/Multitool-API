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

	public async Task InsertEventAsync(CreateCalendarEvent createEvent)
	{
		await _repository.InsertEventAsync(_mapper.Map<CreateCalendarEventDAO>(createEvent));
	}

	public async Task<CalendarEvent> UpdateEventAsync(CalendarEvent updateEvent)
	{
		var updatedEventDao = await _repository.UpdateEventAsync(_mapper.Map<CalendarEventDAO>(updateEvent));
		return _mapper.Map<CalendarEvent>(updatedEventDao);
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