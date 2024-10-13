using Firebase.Database;
using System.Threading.Tasks;

public class UserManager
{
    private DatabaseReference databaseReference;

    public UserManager()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference; // Firebase Database referans?n? al
    }

    public async Task<string> GetUserName(string userId)
    {
        string userName = "";
        var userRef = databaseReference.Child("users").Child(userId); // Kullan?c? yolunu ayarla

        // Kullan?c? ad?n? almak için asenkron olarak bekle
        var snapshot = await userRef.Child("username").GetValueAsync();

        if (snapshot.Exists)
        {
            userName = snapshot.Value.ToString(); // Kullan?c? ad?n? al
        }
       

        return userName; // Kullan?c? ad?n? döndür
    }
}
