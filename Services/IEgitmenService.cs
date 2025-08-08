using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;

namespace StajyerTakipSistemi.Services
{
    public interface IEgitmenService
    {
        Task<List<BasvuruDegerlendirmeViewModel>> GetBekleyenBasvurular();
        Task<List<StajyerListeViewModel>> GetOnayliStajyerler();
        Task<BasvuruDegerlendirmeViewModel?> GetBasvuruDetay(int userId);
        Task<bool> BasvuruOnayla(int userId);
        Task<bool> BasvuruReddet(int userId, string redNedeni);
        Task<bool> GorevAta(int gorevId, List<int> stajyerIds);
        Task<List<Gorev>> GetTumGorevler();
        Task<bool> YeniGorevOlustur(string gorevAdi, string aciklama, DateTime teslimTarihi, string zorlukSeviyesi, int egitmenId);
    }
}