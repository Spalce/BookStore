using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Core.Interfaces.Repositories;
using BookStore.Core.Interfaces.Services;
using BookStore.Core.Models;
using Microsoft.Extensions.Logging;

namespace BookStore.Core.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;
        private readonly ILogger<Book> _logger;

        public BookService(IBookRepository repository, ILogger<Book> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<IEnumerable<Book>> GetRecordsAsync()
        {
            try
            {
                return await _repository.GetRecordsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Book> AddAsync(Book entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;
                return await _repository.AddAsync(entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Book entity)
        {
            try
            {
                return await _repository.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
