using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure
{
    public static class Constants
    {
        public static class ConnectionString
        {
            public const string DefaultConnection = "Server=.\\SQLEXPRESS;Initial Catalog=BookStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public static class JwtToken
        {
            public static string ValidAudience = "bookstoreapi";
            public static string ValidIssuer = "https://localhost:44309";
            public static string Secret = "ThisIsToFulfillAJobRequirement";
        }

        public static class MainDetails
        {
            public static string BookStoreApi = "https://localhost:7029/api/bookstore/v1/";
        }
    }
}
