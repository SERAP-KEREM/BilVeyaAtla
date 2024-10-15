using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using Firebase.Firestore;
using System.Linq; // IEnumerable ile çal??abilmek için gerekli
using System.Threading.Tasks;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerUsername; // Oyuncunun kullan?c? ad?n? göstermek için TextMeshPro ö?esi

    private Player player; // Photon player nesnesi

    // E-posta ile oyuncu bilgilerini ayarlamak için kullan?lacak metod
    public void SetUp(string email)
    {
        LoadUserNameByEmail(email); // E-posta ile kullan?c? ad?n? yükle
    }

    // Firebase'den e-posta ile kullan?c? ad?n? yükleyen asenkron metod
    private async void LoadUserNameByEmail(string email)
    {
        Debug.Log("Loading user name for Email: " + email);

        var db = FirebaseFirestore.DefaultInstance; // Firestore ba?lant?s?n? olu?tur
        var usersCollection = db.Collection("users"); // Kullan?c?lar koleksiyonu

        // E-posta ile kullan?c?y? bulma
        Query query = usersCollection.WhereEqualTo("email", email); // E-posta alan?na göre sorgu olu?tur
        QuerySnapshot snapshot = await query.GetSnapshotAsync(); // Sorguyu asenkron olarak al

        // Sorgu sonucu kontrolü
        if (snapshot.Documents.Count() > 0) // Count metodunu ça??r
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

    // Di?er oyuncu oday? terk etti?inde bu metod çal???r
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject); // Kendisiyle ayn? oyuncu ise GameObject'i yok et
        }
    }

    // Oday? terk etti?inde bu metod çal???r
    public override void OnLeftRoom()
    {
        Destroy(gameObject); // Oday? terk etti?inde GameObject'i yok et
    }
}
