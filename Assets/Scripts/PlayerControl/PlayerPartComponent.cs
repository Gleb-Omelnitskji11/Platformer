using UnityEngine;

namespace PlayerControl
{
    public class PlayerPartComponent : MonoBehaviour
    {
        [SerializeField] private PartType _partType;
        [SerializeField] private SimpleCharacterController _characterController;

        public PartType PartType => _partType;
        public SimpleCharacterController CharacterController => _characterController;
    }

    public enum PartType
    {
        Head
    }
}