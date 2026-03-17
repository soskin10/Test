using Cysharp.Threading.Tasks;

namespace Project.Scripts.Services.UISystem
{
    public interface IView
    {
        bool IsVisible { get; }
        
        UniTask ShowAsync();
        UniTask HideAsync();
        void Close();
    }
}