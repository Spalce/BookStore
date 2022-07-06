using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Core.Interfaces.Repositories;
using BookStore.Core.Models;

namespace BookStore.Core.Interfaces.Services
{
    public interface IBookService : IBaseRepository<Book>
    {
        
    }
}
