using UnityEngine;
using Mikado.Core;

namespace Mikado.Gameplay
{
    public class LivesSystem : MonoBehaviour
    {
        [SerializeField] private LivesCounterValue lives;

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
            GameEventBus.TriggerLifeLost(lives.CurrentLives);

            if (lives.IsOutOfLives)
            {
                GameEventBus.TriggerGameOver(); 
                return;
            }
        }
    }
}