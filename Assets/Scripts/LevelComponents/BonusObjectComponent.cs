using DamageNumbersPro;
using DG.Tweening;
using PlayerControl;
using UnityEngine;

namespace LevelComponents
{
    public class BonusObjectComponent : BasePlayerTriggerComponent
    {
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

        protected override void OnPlayerEnterAction(IPlayerObject playerObject)
        {
            if (!IsPlayerHead(playerObject)) return;
            
            if (Time.time - _lastHeadHitTime < _delay)
                return;
            
            Debug.Log("+1");
            _lastHeadHitTime = Time.time;
            PlayBonus();
        }

        private bool IsPlayerHead(IPlayerObject playerObject)
        {
            var playerPart = playerObject as PlayerPartComponent;
            if (playerPart != null && playerPart.PartType == PartType.Head)
            {
                return true;
            }

            return false;
        }

        private void PlayBonus()
        {
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