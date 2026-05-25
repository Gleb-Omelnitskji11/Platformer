using DamageNumbersPro;
using DG.Tweening;
using PlayerControl;
using UnityEngine;

namespace LevelComponents
{
    public class BonusObjectComponent : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private DamageNumberMesh _numberMeshPrefab;


        [Header("Animation")] [SerializeField] private float _duration = 0.15f;
        [SerializeField] private float _strength = 0.15f;
        [SerializeField] private int _vibrato = 20;

        private Tween _shakeTween;
        private Vector3 _startPosition;
        private float _lastHeadHitTime;
        private const float _delay = 0.5f;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - _lastHeadHitTime < _delay)
                return;

            Collider other = collision.collider;
            if (other.CompareTag(_playerTag))
            {
                if (other.TryGetComponent<PlayerPartComponent>(out PlayerPartComponent playerPart))
                {
                    if (playerPart.PartType == PartType.Head)
                    {
                        _lastHeadHitTime = Time.time;
                        PlayBonus();
                    }
                }
            }
        }

        private void PlayBonus()
        {
            Debug.Log("+1");
            _numberMeshPrefab.Spawn(transform.position);
            PlayShake();
        }

        private void PlayShake()
        {
            _shakeTween?.Kill();

            transform.localPosition = _startPosition;

            _shakeTween = transform.DOShakePosition(
                _duration,
                _strength,
                _vibrato,
                randomness: 90,
                snapping: false,
                fadeOut: true);
        }
    }
}