using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.Models
{
    public partial class ShoppingCart
    {
        public EticaretContext _context { get; set; } //veri tabanına kayıt yapılacak
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId"; //const: sabit
        public ShoppingCart GetCart(HttpContext context)
        {
            var cart = new ShoppingCart();
            cart._context = _context;
            cart.ShoppingCartId = cart.GetCartId(context); //loginsen maili,deglse guidi aldı

            return cart;
        }

        // Helper method to simplify shopping cart calls
        public ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }
        /// <summary>
        /// Sepete ekelenen album bbilgisini alıp,oa anki user bigisi ile birlikte sqltarfaında carts tablosuna yazar. 
        /// eger daha once o user tarafındaan eklenmisse degeri atırır,hic eklenmemmisse sıfırda o albumun bilgilerini
        /// </summary>
        /// <param name="album"></param>
        public void AddToCart(Album album)
        {
            // Get the matching cart and album instances
            var cartItem = _context.Carts.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.AlbumId == album.Id);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    AlbumId = album.Id,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                _context.Carts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, 
                // then add one to the quantity
                cartItem.Count++;
            }
            // Save changes
            _context.SaveChanges();
        }
        /// <summary>
        /// seeppetten urun silme isşemi yapaar aynı zamanda on sılınen once adedini azaltır ensondan bir tane kaldıysa tamamen siler
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = _context.Carts.Single(cart => cart.CartId == ShoppingCartId
                && cart.RecordId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    _context.Carts.Remove(cartItem);
                }
                // Save changes
                _context.SaveChanges();
            }
            return itemCount;
        }
        /// <summary>
        /// sepeti bosaltma islemş aynı zamanda veritabanındaki carts tablosunu da urunleri siler
        /// </summary>
        public void EmptyCart()
        {
            var cartItems = _context.Carts.Where(cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                _context.Carts.Remove(cartItem);
            }
            // Save changes
            _context.SaveChanges();
        }
        /// <summary>
        /// o useerın seppetıne ekleme ıslemi baslangıcdan bu yana dbdeki tum uurnleri getirir
        /// </summary>
        /// <returns></returns>
        public List<Cart> GetCartItems()
        {
            return _context.Carts.Include(p => p.Album).Where(cart => cart.CartId == ShoppingCartId).ToList();
            // _context.Carts.Where(cart => cart.CartId == ShoppingCartId).ToList();
        }
        public int GetCount()
        {
            // Get the count of each item in the cart and sum them up
            int? count = (from cartItems in _context.Carts
                          where cartItems.CartId == ShoppingCartId
                          select (int?)cartItems.Count).Sum();
            // Return 0 if all entries are null
            return count ?? 0;
        }
        /// <summary>
        /// seeptteki urunlerın toplam tutarını getirir
        /// </summary>
        /// <returns></returns>
        public decimal GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = (from cartItems in _context.Carts
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Count *
                              cartItems.Album.Price).Sum();

            return total ?? decimal.Zero;
        }
        /// <summary>
        /// sıparıs bilgisini  olusturur ve hem order detailse hem orderee kayıt ekler
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();
            // Iterate over the items in the cart, 
            // adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    AlbumId = item.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Album.Price,
                    Quantity = item.Count
                };
                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.Album.Price);

                _context.OrderDetails.Add(orderDetail);

            }
            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            _context.SaveChanges();
            // Empty the shopping cart
            EmptyCart();
            // Return the OrderId as the confirmation number
            return order.OrderId;
        }

        // We're using HttpContextBase to allow access to cookies.
        //Session yapısını kullanarak giriş yapan bır useer ise giris yapamnın maili donen , diger durumlarda giris yapmıs olaan verilen Guid bilgisini bize donecek
        public string GetCartId(HttpContext context)
        {
            if (context.Session.GetString("CartSessionKey") == null)
            {
                //login durumda
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session.SetString("CartSessionKey", context.User.Identity.Name);
                }
                //henuz giris yapmadıysa
                else
                {

                    // Generate a new random GUID using System.Guid class
                    //Guid tempCartId = Guid.NewGuid();
                    // Send tempCartId back to client as a cookie
                    //context.Session.SetString("CartSessionKey", tempCartId.ToString());

                    context.Session.SetString("CartSessionKey", Guid.NewGuid().ToString());
                }
            }
            return context.Session.GetString("CartSessionKey");
        }
        // When a user has logged in, migrate their shopping cart to
        // be associated with their username

        /// <summary>
        /// kıllanıcı login oldugu an sepetındeki urunlerı alp sqlden o userın mailiyle cart tablosundaki guid blgisini gunceller
        /// </summary>
        /// <param name="userName"></param>
        public void MigrateCart(string userName)
        {
            var shoppingCart = _context.Carts.Where(
                c => c.CartId == ShoppingCartId);

            foreach (Cart item in shoppingCart)
            {
                item.CartId = userName;
            }
            _context.SaveChanges();
        }
    }
}
