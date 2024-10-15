using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using Firebase.Firestore;
using System.Linq; // IEnumerable ile �al??abilmek i�in gerekli
using System.Threading.Tasks;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerUsername; // Oyuncunun kullan?c? ad?n? g�stermek i�in TextMeshPro �?esi

    private Player player; // Photon player nesnesi

    // E-posta ile oyuncu bilgilerini ayarlamak i�in kullan?lacak metod
    public void SetUp(string email)
    {
        LoadUserNameByEmail(email); // E-posta ile kullan?c? ad?n? y�kle
    }

    // Firebase'den e-posta ile kullan?c? ad?n? y�kleyen asenkron metod
    private async void LoadUserNameByEmail(string email)
    {
        Debug.Log("Loading user name for Email: " + email);

        var db = FirebaseFirestore.DefaultInstance; // Firestore ba?lant?s?n? olu?tur
        var usersCollection = db.Collection("users"); // Kullan?c?lar koleksiyonu

        // E-posta ile kullan?c?y? bulma
        Query query = usersCollection.WhereEqualTo("email", email); // E-posta alan?na g�re sorgu olu?tur
        QuerySnapshot snapshot = await query.GetSnapshotAsync(); // Sorguyu asenkron olarak al

        // Sorgu sonucu kontrol�
        if (snapshot.Documents.Count() > 0) // Count metodunu �a??r
        {
            var userData = snapshot.Documents.First().ToDictionary(); // ?lk sonucu al
            if (userData != null && userData.ContainsKey("userName"))
            {
                playerUsername.text = userData["userName"].ToString(); // Kullan?c? ad?n? ayarla
                Debug.Log("User name loaded: " + playerUsername.text);
            }
            else
            {
                Debug.LogWarning("UserName field not found in user data."); // Kullan?c? ad? bulunamad???nda uyar? ver
            }
        }
        else
        {
            Debug.LogWarning("No user data found for email: " + email); // Kullan?c? verisi yoksa uyar? ver
        }
    }

    // Di?er oyuncu oday? terk etti?inde bu metod �al???r
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject); // Kendisiyle ayn? oyuncu ise GameObject'i yok et
        }
    }

    // Oday? terk etti?inde bu metod �al???r
    public override void OnLeftRoom()
    {
        Destroy(gameObject); // Oday? terk etti?inde GameObject'i yok et
    }
}
