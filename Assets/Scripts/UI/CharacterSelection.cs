using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : NetworkBehaviour
{


    public GameObject[] characters;
	public int selectedCharacter = 0;
    private const string MenuSceneName = "Menu";
    public const string TankIndexKey = "TankIndex";


	public void NextCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter = (selectedCharacter + 1) % characters.Length;
		characters[selectedCharacter].SetActive(true);
	}

	public void PreviousCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter--;
		if (selectedCharacter < 0)
		{
			selectedCharacter += characters.Length;
		}
		characters[selectedCharacter].SetActive(true);
	}

	public void GoToMenu()
	{
		PlayerPrefs.SetInt(TankIndexKey, selectedCharacter);
        // PlayerPrefs.Save();
		SceneManager.LoadScene(MenuSceneName);
	}
}
