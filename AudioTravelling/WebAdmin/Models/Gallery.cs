namespace WebAdmin.Models
{
    public class Gallery
    {
        public int GalleryID { get; set; }
        public string GalleryLink { get; set; }
        public int StoreID { get; set; }

        public Store Store { get; set; }
    }
}