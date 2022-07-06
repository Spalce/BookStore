using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookStore.Core.Interfaces.Repositories;
using BookStore.Core.Models;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public BookRepository(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        public async Task<IEnumerable<Book>> GetRecordsAsync()
        {
            var model = await _dbContext.Books.ToListAsync().ConfigureAwait(false);
            if (model != null)
            {
                return _mapper.Map<IEnumerable<Book>>(model);
            }

            return null;
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            var model = await _dbContext.Books.FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
            if (model != null)
            {
                return _mapper.Map<Book>(model);
            }

            return null;
        }

        public async Task<Book> AddAsync(Book entity)
        {
            var mainModel = _mapper.Map<Models.Book>(entity);
            await _dbContext.AddAsync(mainModel);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        public async Task<bool> UpdateAsync(Book entity)
        {
            var item = await _dbContext.Books.FindAsync(entity.Id);
            
            item.Title = entity.Title;
            item.Author = entity.Author;
            item.Description = entity.Description;
            item.Price = entity.Price;
            item.CategoryId = entity.CategoryId;
            item.ImageUrl = entity.ImageUrl;
            item.UpdatedDate = DateTime.Now;

            if (entity != null)
            {
                _dbContext.Books.Update(item);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _dbContext.Books.FindAsync(id);
            if (item != null)
            {
                _dbContext.Books.Remove(item);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }
    }
}
