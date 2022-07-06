using AutoMapper;
using BookStore.Core.Models;


namespace BookStoreApi.Helpers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<BookStore.Infrastructure.Models.Book, Book>().ReverseMap();
            CreateMap<BookStore.Infrastructure.Models.Category, Category>().ReverseMap();
        }
    }
}
