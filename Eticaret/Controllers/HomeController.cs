using Eticaret.Data;
using Eticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Eticaret.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EticaretContext eticaretContext;

        public HomeController(ILogger<HomeController> logger,EticaretContext context)
        {
            _logger = logger;
            eticaretContext = context;
        }

        public IActionResult Index()
        {
            //Session yapısı
            //Session
            //Oturum anlamına gelir,veriyi tarafında tutmaya yarar.
            //SessionTimeOut ile Session suresını ayarlarız,
            //Facebook da session suresi sınırsı,sen logout olmadıkca dusmuyor,
            //banka uygulamalrı bilet satıs uygulamalrında timeout oluyor
            //Controller baska bir controllera veri tasımak icin Session yapısı kullamılır

            //Session içerisne deger atmak icin
            HttpContext.Session.SetInt32("sepetAdedi",3);
            
            SampleData sampleData = new SampleData(eticaretContext);
            sampleData.AddGenres();
            sampleData.AddArtist();
            sampleData.AddAlbums();
            return View();
        }

       
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		//Takim sayfasını getirıp gosterecek metodu yazalim
		public IActionResult Team()
		{
            /*int? adett = HttpContext.Session.GetInt32("sepetAdedi");*///int ile int?(boş gecilebilme)
            /* ViewData["sepet"]= adett;*/ //view tarafında sepet verisini gorebilmek icin eklendi
            //ViewData["sepet"] = HttpContext.Session.GetInt32("sepetAdedi");



			return View();
		}
		public IActionResult ContactUs()
		{

			return View();
		}
        //Asagida metot  bize katlın
        public IActionResult JoinUs()
        {

            return View();
        }
        /// <summary>
        /// joinUs sayfasındaki  Buttona tıklandıgında form icerisnde verileri asp-actions belistıgımız yerw yani buraya post yöntemiyle gonderilir burada o verileri veritabanıdaki tabloya eklenir
        /// </summary>
        /// <returns></returns>
        [HttpPost]
		public IActionResult UserSave(string name,string surname, string email,string phone,string message)
		{
            ApplyUser user=new ApplyUser() { Name=name,Surname=surname,Email=email,Telephone=phone,Description=message};
            eticaretContext.ApplyUsers.Add(user);
            eticaretContext.SaveChanges();
			return View();
			
        }
		public IActionResult Kategoriler()
		{
            List<Genre>turler=eticaretContext.Genres.ToList();
			return View(turler);
		}
        /// <summary>
        /// sayfayı acmaya yarayan metot
        /// </summary>
        /// <returns></returns>
        public IActionResult KategoriEkle()
        {
            return View();
        }
        /// <summary>
        /// button tıklanınca form uzerindeki verileri backende yolamaya yarayan metod
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult KategoriEkle(Genre genre)
        {
            string duzenlenenVeri = MetniDuzenle(genre.Name);
            if(!KategoriVarmi(duzenlenenVeri))
            {
                genre.Name= duzenlenenVeri;
                eticaretContext.Genres.Add(genre);
                eticaretContext.SaveChanges();
                return View();
            }
            else 
                return View("Error");
            eticaretContext.Genres.Add(genre);
            eticaretContext.SaveChanges();
            return View();
        }
        //girilen tur adı tabloda varsa kaydı ekleme
        public bool KategoriVarmi(string kategoriAdi)
        {
            Genre dbdenGelen = eticaretContext.Genres.Where(satir=>satir.Name==kategoriAdi).FirstOrDefault();
            if (dbdenGelen != null)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Girilmis olan bilgiyii ilk harfi buyuk digerleri kucuk olacak sekilde duzenliyor
        /// rock....Rock
        /// </summary>
        public string MetniDuzenle(string tur)
        {
            string result = "";
            result+=tur.Substring(0,1).ToUpper();
            for (int i = 1; i < tur.Length; i++)
            {
                result += tur[i].ToString().ToLower();
            }
            return result;
        }

        //Edit sayfasını acacak olan metot
        //[Route("edityapılacaktur")]
        public IActionResult Edit(int id)
        {
            Genre editYapilacakTur=eticaretContext.Genres.Where(satir=> satir.Id==id).FirstOrDefault();
            return View(editYapilacakTur);
        }
        [HttpPost]

        public IActionResult Edit(Genre genre)
        {
            eticaretContext.Genres.Update(genre);
            eticaretContext.SaveChanges();
          return View("Kategoriler",eticaretContext.Genres.ToList());
        }
        public IActionResult Details(int id) 
        {         

            Genre genre =eticaretContext.Genres.Where(satir=>satir.Id==id).FirstOrDefault();
            return View(genre);
        
        }  
        public IActionResult Delete(int id) 
        {         

            Genre genre =eticaretContext.Genres.Where(satir=>satir.Id==id).FirstOrDefault();
            return View(genre);
        
        }
        [HttpPost]

        public IActionResult DeleteConfirmed(int id) 
        {         

            Genre genre =eticaretContext.Genres.Where(satir=>satir.Id==id).FirstOrDefault();
            eticaretContext.Remove(genre);
            eticaretContext.SaveChanges();
            return View("Kategoriler", eticaretContext.Genres.ToList());

        }

        public IActionResult Sanatcilar()
        { 
            List<Artist> sanatcilar = eticaretContext.Artists.ToList();
            return View(sanatcilar);
        }
        public IActionResult Detay(int id) 
        {
            return View(eticaretContext.Artists.Where(satir=>satir.ArtistId==id).FirstOrDefault());
        
        }
        public IActionResult Albumler()
        {
            List<Album> albumler=eticaretContext.Albums.Include(satir=>satir.Genre).Include(satir => satir.Artist).ToList();
            return View(albumler);
        }
        public IActionResult AlbumlerHepsi()
        {

            List<Album> albumler = eticaretContext.Albums.Include(satir => satir.Genre).Include(satir => satir.Artist).ToList();
            ViewData["CartCount"] = HttpContext.Session.GetString("adet");
            return View(albumler);
        }

        
    }
}

