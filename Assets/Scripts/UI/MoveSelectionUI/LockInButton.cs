using Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MoveSelectionUI
{
    public class LockInButton : MonoBehaviour
    {

        [field: SerializeField] public GameStateStorage GameStateStorage { get; set; }

        public void OnPressLockIn()
        {
            if (GameStateStorage.SelectedMoves.Count == 4 && !string.IsNullOrEmpty(GameStateStorage.StarterPokemon))
            {
                SceneManager.LoadScene("BattleScene");
            }
        }
    }
}
