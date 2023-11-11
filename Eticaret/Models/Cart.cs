using System.ComponentModel.DataAnnotations;

namespace Eticaret.Models
{
    public class Cart
    {
        [Key]
        public int RecordId { get; set; }
        public string CartId { get; set; } //eger loginsek login olan ksıının mailini,degilsek guid bilgisi 
        public int AlbumId { get; set; }
        public int Count { get; set; }
        public System.DateTime DateCreated { get; set; }
        public virtual Album Album { get; set; }

    }
}
