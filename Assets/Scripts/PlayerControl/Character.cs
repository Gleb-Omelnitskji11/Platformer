using UnityEngine;

namespace PlayerControl
{
    public class Character : MonoBehaviour, IPlayerObject
    {
        [Header("Respawn data")]
        [SerializeField] private Vector3 _initialPosition = new Vector3(0f, 0f, 0f);
        [SerializeField] private CharacterMover _characterMover;


        public void Kill()
        {
            _characterMover.Stop();
            transform.position = _initialPosition;
            _characterMover.Play();
        }
    }
}
