using Doinject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour, IInjectableComponent
{
    [SerializeField] TextMeshProUGUI playerNameText;
    
    PlayerModel PlayerModel { get; set; }

    [Inject]
    public void Construct(PlayerModel playerModel)
    {
        PlayerModel = playerModel;
    }

    [OnInjected]
    public void OnInjected()
    {
        playerNameText.text = PlayerModel.displayName;
    }

}
