namespace BulletinBoardAPI.Services.Interfaces;

public interface IJsonStorageService<T>
{
    List<T> GetAll();
    void Modify(Action<List<T>> modifier);
    TResult Modify<TResult>(Func<List<T>, TResult> modifier);
}
