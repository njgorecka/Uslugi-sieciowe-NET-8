using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelQuotesApi.Data;
using TravelQuotesApi.Interfaces;
using TravelQuotesApi.Models;

[Route("api/[controller]")]
[ApiController]
public class QuotesController : ControllerBase
{
    private readonly IRepository<Quote> _quoteRepository;

    public QuotesController(IRepository<Quote> quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    // GET: api/Quotes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes()
    {
        var quotes = await _quoteRepository.GetAllAsync();
        return Ok(quotes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Quote>> GetQuote(int id)
    {
        var quote = await _quoteRepository.GetByIdAsync(id);
        if (quote == null) { return NotFound(); }
        return Ok(quote);
    }

    [HttpPost]
    public async Task<ActionResult<Quote>> PostQuote(Quote quote)
    {
        await _quoteRepository.CreateAsync(quote);
        return CreatedAtAction("GetQuote", new { id = quote.Id }, quote);
    }
}