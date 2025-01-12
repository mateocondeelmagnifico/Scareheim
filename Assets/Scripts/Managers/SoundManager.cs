using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance{ get; private set; }
    public Sound[] Sonidos;
    public AudioSource[] Sources;
    [SerializeField]private Slider volumeSlider;

    [SerializeField] private bool autoPlayMusic;
    public float volumeSetting;

    public bool isFirst;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if(autoPlayMusic)PlaySound("Music");
    }

    private void Start()
    {
        volumeSlider.value = InfoKeeper.instance.volume;
        volumeSetting = volumeSlider.value;
        if (SceneManager.GetActiveScene().buildIndex == 0) isFirst = true;
    }

    private void Update()
    {
        if(isFirst) Sources[0].volume = volumeSetting;
        volumeSetting = volumeSlider.value;
        Sources[2].volume = volumeSetting;
    }

    //Called by other gameobjects, manages all sounds in the game
    public void PlaySound(string name)
    {
        for(int i = 0; i < Sonidos.Length; i++)
        {
            if(Sonidos[i].name == name)
            {
                Sound mySound = Sonidos[i];

                Sources[mySound.source].volume = mySound.volume * volumeSetting;
                Sources[mySound.source].clip = mySound.clip;
                Sources[mySound.source].Play();
            }
        }
    }
}
