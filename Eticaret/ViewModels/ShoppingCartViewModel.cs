using Eticaret.Models;

namespace Eticaret.ViewModels
{
    /// <summary>
    /// View model yapısı view icin gerekli olan alanları tutsun dıye olusturdugumuz class
    /// </summary>
    public class ShoppingCartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
