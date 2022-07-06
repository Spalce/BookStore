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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public CategoryRepository(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        public async Task<IEnumerable<Category>> GetRecordsAsync()
        {
            var model = await _dbContext.Categories.ToListAsync().ConfigureAwait(false);
            if (model != null)
            {
                return _mapper.Map<IEnumerable<Category>>(model);
            }

            return null;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var model = await _dbContext.Categories.FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
            if (model != null)
            {
                return _mapper.Map<Category>(model);
            }

            return null;
        }
        
        public async Task<Category> AddAsync(Category entity)
        {
            var mainModel = _mapper.Map<Models.Category>(entity);
            await _dbContext.AddAsync(mainModel);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        public async Task<bool> UpdateAsync(Category entity)
        {
            var item = await _dbContext.Categories.FindAsync(entity.Id);
            
            _dbContext!.Entry(item).State = EntityState.Modified;
            
            if (entity != null)
            {
                _dbContext.Categories.Update(item);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _dbContext.Categories.FindAsync(id);
            if (item != null)
            {
                _dbContext.Categories.Remove(item);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }
    }
}
