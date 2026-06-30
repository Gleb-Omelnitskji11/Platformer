using UnityEngine;

namespace PlayerControl
{
    public class PlayerPartComponent : MonoBehaviour, IPlayerObject
    {
        [SerializeField] private PartType _partType;

        public PartType PartType => _partType;
        public void Kill()
        {
            
        }
    }

    public enum PartType
    {
        Head
    }
}