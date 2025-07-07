using GameServer.Data;
using GameServer.GameRoomFolder;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain
{
    public class CharacterStatus
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public GameRoom Room { get; private set; }

        public float HP { get; private set; }
        public float MaxHP { get; private set; }

        public WeaponData EquippedWeapon { get; private set; }

        public bool IsDead => HP <= 0;

        public void OnDamaged(CharacterStatus attacker, float damage)
        {
            Console.WriteLine($"{Name}이 {attacker.Name}으로 부터 공격당함");
            HP -= damage;
            Console.WriteLine($"HP : {HP}/{MaxHP} | 데미지 {damage}");
        }
        public CharacterStatus(int id, string name, Vector3 startPos, Quaternion rotation, GameRoom room)
        {
            Id = id;
            Name = name;
            Position = startPos;
            Rotation = rotation;
            Room = room;

            MaxHP = 100;
            HP = MaxHP;
            EquippedWeapon = new WeaponData()
            {
                Type = WeaponType.Fist,
                AttackType = AttackType.Punch,
                Damage = 1,
                Range = 2,
                Cooldown = 0.2f,
            }; // 예: 기본 무기
        }
        public static CharacterStatus ConvertToRuntimeStatus(CharacterStatusDTO dto, string name, GameRoom room)
        {
            return new CharacterStatus(
                dto.characterId,
                name,
                new Vector3(0, 0, 0),
                Quaternion.Identity,
                room
            );
        }
    }
}
