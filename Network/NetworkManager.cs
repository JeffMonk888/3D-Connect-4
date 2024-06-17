
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { set; get; }

    public Server server;
    public Client client;
    [SerializeField] private TMP_InputField addressInput;
   

    public bool backButtonClicked = false;

    private void Start()
    {
        addressInput = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnOnlineHostButton()
    {
        server.Init(8007);
        SceneManager.LoadScene("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        string ipAddress = addressInput.text; // Use the input field value as the IP address
        client.Init(ipAddress, 8007);
        SceneManager.LoadScene("HostMenu");
    }

    public void OnHostBackButton()
    {
        client.Shutdown();
        Destroy(gameObject);
        Instance = null;
    }

    public bool BackButtonClicked
    {
        get { return backButtonClicked; }
    }

    

}
