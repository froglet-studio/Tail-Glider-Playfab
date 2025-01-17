using UnityEngine;

namespace StarWriter.Core.AI
{
    public class AIGunner : MonoBehaviour
    {
        [SerializeField] Gun gun;
        [SerializeField] GameObject gunMount;

        public Teams Team;
        public Ship Ship;
        
        void Start()
        {
            gun.Team = Team;
            gun.Ship = Ship;
        }
    }
}