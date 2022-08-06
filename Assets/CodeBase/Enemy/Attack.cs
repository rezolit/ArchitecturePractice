using System;
using System.Linq;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;

namespace CodeBase.Enemy
{
	[RequireComponent(typeof(EnemyAnimator))]
	public class Attack : MonoBehaviour
	{
		[SerializeField] private EnemyAnimator _animator;
		[SerializeField] private float _attackCooldown = 3.0f;
		[SerializeField] private float Cleavage = 0.5f;
		[SerializeField] private float _effectiveDistance = 0.5f;

		private IGameFactory _factory;
		private Transform _heroTransform;
		private float _attackCooldownTimer;
		private bool _isAttacking;
		private int _layerMask;
		private Collider[] _hits = new Collider[1];
		private bool _attackIsActive;

		private void Awake()
		{
			_factory = AllServices.Container.Single<IGameFactory>();

			_layerMask = 1 << LayerMask.NameToLayer("Player");

			_factory.HeroCreated += OnHeroCreated;
		}

		private void Update()
		{
			UpdateCooldown();

			if (CanAttack())
				StartAttack();
		}

		public void EnableAttack()
		{
			_attackIsActive = true;
		}

		public void DisableAttack()
		{
			_attackIsActive = false;
		}

		private void UpdateCooldown()
		{
			if (CooldownIsUp() == false)
				_attackCooldownTimer -= Time.deltaTime;
		}

		private void OnAttack()
		{
			if (Hit(out Collider hit))
			{
				PhysicsDebug.DrawDebug(StartPoint(), Cleavage, 1.0f);
			}
		}

		private void OnAttackEnded()
		{
			_attackCooldownTimer = _attackCooldown;
			_isAttacking = false;
		}

		private bool Hit(out Collider hit)
		{
			int hitsCount = Physics.OverlapSphereNonAlloc(StartPoint(), Cleavage, _hits, _layerMask);

			hit = _hits.FirstOrDefault();
			
			return hitsCount > 0;
		}

		private Vector3 StartPoint()
		{
			return new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z)
			       + transform.forward * _effectiveDistance;
		}

		private void StartAttack()
		{
			transform.LookAt(_heroTransform);
			_animator.PlayAttack();

			_isAttacking = true;
		}

		private bool CooldownIsUp() => 
			_attackCooldownTimer <= 0.0f;

		private bool CanAttack() => 
			_attackIsActive && _isAttacking == false && CooldownIsUp();

		private void OnHeroCreated() => 
			_heroTransform = _factory.HeroGameObject.transform;
	}
}