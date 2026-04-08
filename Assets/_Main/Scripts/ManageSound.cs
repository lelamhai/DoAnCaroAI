using UnityEngine;

public class ManageSound : MonoBehaviour
{
    public static ManageSound Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip click;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    public void PlaySoundClick(Cell clickedCell)
    {
        audioSource.PlayOneShot(click);
    }
}
