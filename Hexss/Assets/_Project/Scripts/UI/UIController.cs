using UnityEngine;
using VContainer;

public class UIController : MonoBehaviour
{
    public Hud hud => hudGo;
    public Menu menu => menuGo;
    [SerializeField] private Hud hudGo;
    [SerializeField] private Menu menuGo;
    public void ShowMenu()
    {
        menuGo.gameObject.SetActive(true);
        hudGo.gameObject.SetActive(false);
    }

    public void ShowHud()
    {
        menuGo.gameObject.SetActive(false);
        hudGo.gameObject.SetActive(true);
    }
    
}
