using GameServer.GameRoomFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Attack
{
    public class ProjectileManager
    {
        private List<Projectile> _projectiles = new();

        public void AddProjectile(Projectile p)
        {
            _projectiles.Add(p);
        }

        public void Update(float deltaTime, GameRoom room)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile p = _projectiles[i];
                p.Update(deltaTime, room);

                if (p.IsExpired)
                    _projectiles.RemoveAt(i);
            }
        }
    }
}
