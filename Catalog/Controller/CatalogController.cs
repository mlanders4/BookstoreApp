using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Catalog.Accessor;
using Catalog.Engine;

namespace Catalog.Controller
{
    [ApiController]
    [Route("api/catalog")]
    [Produces("application/json")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogEngine _engine;

        public CatalogController(CatalogEngine engine)
        {
            _engine = engine;
        }

        [HttpGet("books")]
        public ActionResult<IEnumerable<Book>> GetAllBooks()
        {
            try
            {
                return Ok(_engine.GetAllBooks());
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("books/{isbn}")]
        public ActionResult<Book> GetBookByIsbn(string isbn)
        {
            try
            {
                var book = _engine.GetBookByIsbn(isbn);
                if (book == null)
                    return NotFound();
                
                return Ok(book);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("books/search")]
        public ActionResult<IEnumerable<Book>> SearchBooks([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest("Search term is required");
                
                return Ok(_engine.SearchBooks(term));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("categories")]
        public ActionResult<IEnumerable<string>> GetCategories()
        {
            try
            {
                return Ok(_engine.GetCategories());
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("categories/{category}")]
        public ActionResult<IEnumerable<Book>> GetBooksByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                    return BadRequest("Category is required");
                
                return Ok(_engine.GetBooksByCategory(category));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("sales/{saleId}")]
        public ActionResult<IEnumerable<Book>> GetBooksBySale(int saleId)
        {
            try
            {
                return Ok(_engine.GetBooksBySale(saleId));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("books")]
        public ActionResult CreateBook([FromBody] Book book)
        {
            try
            {
                if (book == null)
                    return BadRequest("Book data is required");

                bool success = _engine.CreateBook(book);
                
                if (success)
                    return CreatedAtAction(nameof(GetBookByIsbn), new { isbn = book.ISBN }, book);
                else
                    return StatusCode(500, "Failed to create book");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPut("books/{isbn}")]
        public ActionResult UpdateBook(string isbn, [FromBody] Book book)
        {
            try
            {
                if (book == null)
                    return BadRequest("Book data is required");

                if (isbn != book.ISBN)
                    return BadRequest("ISBN mismatch");

                bool success = _engine.UpdateBook(book);
                
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpDelete("books/{isbn}")]
        public ActionResult DeleteBook(string isbn)
        {
            try
            {
                bool success = _engine.DeleteBook(isbn);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("books/{isbn}/available")]
        public ActionResult<bool> IsBookAvailable(string isbn)
        {
            try
            {
                return Ok(_engine.IsBookAvailable(isbn));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("books/{isbn}/price")]
        public ActionResult<decimal> GetBookPrice(string isbn, [FromQuery] bool applyDiscount = false)
        {
            try
            {
                var book = _engine.GetBookByIsbn(isbn);
                if (book == null)
                    return NotFound();
                
                return Ok(_engine.CalculatePrice(book, applyDiscount));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
} 