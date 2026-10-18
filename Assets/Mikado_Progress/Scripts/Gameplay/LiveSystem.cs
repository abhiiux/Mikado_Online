using UnityEngine;
using Mikado.Core;
using Mikado.Data;

namespace Mikado.Gameplay
{
    public class LivesSystem : MonoBehaviour
    {
        [SerializeField] private LivesCounterValue lives;
        [SerializeField] private WinPanelDetailsSO winPanelDetails;

        void Awake()
        {
            lives.ResetLives();
        }
        void OnEnable()
        {
            GameEventBus.OnFoulCommitted += HandleFoul;
        }
        void OnDisable()
        {
            GameEventBus.OnFoulCommitted -= HandleFoul;
        }

        private void HandleFoul()
        {
            // if (lives.IsOutOfLives)
            // {
            //     GameEventBus.TriggerGameOver(); 
            //     return;
            // }

            lives.LoseLife();
            winPanelDetails.UpdatePickup();
            GameEventBus.TriggerLifeLost(lives.CurrentLives);

            if (lives.IsOutOfLives)
            {
                GameEventBus.TriggerGameOver(); 
                return;
            }
        }
    }
}