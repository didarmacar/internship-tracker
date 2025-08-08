using StajyerTakipSistemi.Models;
using StajyerTakipSistemi.Models.ViewModels;

namespace StajyerTakipSistemi.Services
{
    public interface IStajyerService
    {
        Task<string> GetUserBasvuruDurumu(int userId);
        Task<List<OdevViewModel>> GetStajyerOdevleri(int userId);
        Task<List<EgitimViewModel>> GetStajyerEgitimleri(int userId);
        Task<List<AktiviteViewModel>> GetStajyerAktiviteleri(int userId);
        Task<int> CalculateGenelIlerleme(int userId);
        Task<int> CalculateBuHaftaIlerleme(int userId);
        Task<Stajyer?> GetStajyerByUserId(int userId);
        Task UpdateUserBasvuru(int userId, StajyerBasvuruViewModel model, string? cvPath, string? fotoPath);
    }
}