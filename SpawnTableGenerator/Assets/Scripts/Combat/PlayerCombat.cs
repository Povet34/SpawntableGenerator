using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 플레이어 전투.
    /// 1키 / 2키 → 무기 슬롯 스왑 (근접 / 원거리).
    /// 좌클릭 홀드 → 현재 슬롯 무기 발사 (쿨다운 지속 연사).
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("무기 슬롯")]
        public MeleeWeaponSO meleeWeapon;   // 슬롯 1
        public RangedWeaponSO rangedWeapon; // 슬롯 2

        [Header("발사 지점")]
        [Tooltip("원거리 발사 시작 지점. 비우면 'Muzzle' 자식을 자동 탐색.")]
        public Transform muzzle;

        int _slot = 0; // 0=근접, 1=원거리
        float _cooldown;

        Camera _cam;
        readonly List<Vector3> _posBuf = new List<Vector3>();
        readonly List<Monster> _monBuf = new List<Monster>();
        Player.PlayerController _ctrl;

        WeaponSO ActiveWeapon => _slot == 0 ? (WeaponSO)meleeWeapon : rangedWeapon;
        public int ActiveSlot => _slot;

        /// <summary>무기 슬롯이 바뀔 때 발행(UI 모델 어댑터가 구독).</summary>
        public event System.Action SlotChanged;

        void Awake()
        {
            _cam = Camera.main;
            _ctrl = GetComponent<Player.PlayerController>();
            if (muzzle == null) muzzle = transform.Find("Muzzle");
            if (meleeWeapon == null) meleeWeapon = MakeDefaultMelee();
            if (rangedWeapon == null) rangedWeapon = MakeDefaultRanged();
        }

        void Update()
        {
            if (_cam == null) _cam = Camera.main;
            _cooldown -= Time.deltaTime;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) SetSlot(0);
                if (kb.digit2Key.wasPressedThisFrame) SetSlot(1);
            }

            // 좌클릭 홀드 → 현재 무기 연속 발사
            if (Mouse.current != null && Mouse.current.leftButton.isPressed && _cooldown <= 0f)
                Fire();
        }

        void SetSlot(int slot)
        {
            if (_slot == slot) return;
            _slot = slot;
            SlotChanged?.Invoke();
        }

        void Fire()
        {
            var w = ActiveWeapon;
            if (w == null) return;

            Vector3 aimDir = AimDir();
            FaceDir(aimDir);

            if (w is MeleeWeaponSO melee)
                FireMelee(melee, aimDir);
            else if (w is RangedWeaponSO ranged)
                FireRanged(ranged, aimDir);
        }

        public void FireMelee(MeleeWeaponSO melee, Vector3 aimDir)
        {
            GatherMonsters();
            var hits = AoeTargets.InArc(transform.position, aimDir, melee.radius, melee.arcDeg, _posBuf);
            foreach (int i in hits)
            {
                var h = _monBuf[i].Health;
                if (h != null) h.TakeDamage(melee.damage, melee.damageType);
            }

            MeleeArcVfx.Spawn(transform.position, aimDir, melee.radius, melee.arcDeg, 0.1f,
                               new Color(0.3f, 1f, 0.5f));

            if (melee.slashVfxPrefab != null)
            {
                var vgo = Object.Instantiate(melee.slashVfxPrefab, transform.position,
                                             Quaternion.LookRotation(aimDir));
                Object.Destroy(vgo, 3f);
            }

            _cooldown = melee.cooldown;
            NotifyViewPressure(aimDir);
        }

        public void FireRanged(RangedWeaponSO ranged, Vector3 aimDir)
        {
            // Muzzle(얼굴 발사구)에서 발사. 없으면 몸통 위쪽에서 폴백.
            Vector3 origin = muzzle != null
                ? muzzle.position
                : transform.position + Vector3.up * 1.1f + aimDir * 1.2f;
            LaserProjectile.Spawn(ranged.projectilePrefab, origin, aimDir, ranged, hitsPlayer: false);
            _cooldown = ranged.cooldown;
            NotifyViewPressure(aimDir);
        }

        Vector3 AimDir()
        {
            if (_cam != null && Mouse.current != null)
            {
                Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
                if (plane.Raycast(ray, out float e))
                {
                    Vector3 d = ray.GetPoint(e) - transform.position;
                    d.y = 0f;
                    if (d.sqrMagnitude > 1e-4f) return d.normalized;
                }
            }
            return transform.forward;
        }

        void FaceDir(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return;
            transform.rotation = Quaternion.LookRotation(dir);
            if (_ctrl != null) _ctrl.OverrideRotation(dir);
        }

        void GatherMonsters()
        {
            _posBuf.Clear();
            _monBuf.Clear();
            var all = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
            foreach (var m in all)
            {
                if (m == null || m.Health == null || m.Health.IsDead) continue;
                _monBuf.Add(m);
                _posBuf.Add(m.transform.position);
            }
        }

        void NotifyViewPressure(Vector3 dir)
        {
            var packs = Object.FindObjectsByType<Monsters.MonsterPack>(FindObjectsSortMode.None);
            foreach (var pk in packs) pk.NotifyPlayerAttack(dir);
        }

        static MeleeWeaponSO MakeDefaultMelee()
        {
            var w = ScriptableObject.CreateInstance<MeleeWeaponSO>();
            w.damage = 40f;
            w.damageType = DamageType.Piercing;
            w.cooldown = 0.45f;
            w.radius = 4.5f;
            w.arcDeg = 120f;
            return w;
        }

        static RangedWeaponSO MakeDefaultRanged()
        {
            var w = ScriptableObject.CreateInstance<RangedWeaponSO>();
            w.damage = 14f;
            w.damageType = DamageType.Normal;
            w.cooldown = 0.22f;
            w.projectileSpeed = 75f;
            w.projectileRange = 40f;
            w.projectileColor = new Color(4f, 0.6f, 0.4f, 1f);
            return w;
        }
    }
}
