using Eticaret.Models;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.Data
{
	public class EticaretContext:DbContext
	{
        public EticaretContext(DbContextOptions<EticaretContext>options):base(options) 
        {
                
        }
        /// <summary>
        /// ApplyUser classını baz alarak ApplyUsers tablosunu veri TAbanına eklesin diye yazdık.
        /// </summary>
        public DbSet<ApplyUser> ApplyUsers { get; set; }
        public DbSet<Genre> Genres { get; set; }//genre cllassın daki alanlara bak  Genres adında sql tarafında bir tablo olusturur

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
