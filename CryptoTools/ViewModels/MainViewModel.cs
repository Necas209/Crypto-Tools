using System.Threading.Tasks;

namespace CryptoTools.ViewModels;

public class MainViewModel : ViewModelBase
{
    public async Task Logout() => await Model.CloseConnection();
}