using System.Threading.Tasks;
using TFG.GOAP;
using UnityEngine;

namespace TFG.NPC
{
    public class HybridMemoryDrivenAI : MonoBehaviour
    {
        public MemoryIntentBridge intent;
        public CombatMovement movement;
        public WorldState world;
        public bool driveWithBT = true;

        void Awake()
        {
            if (!intent) intent = GetComponent<MemoryIntentBridge>();
            if (!movement) movement = GetComponent<CombatMovement>();
            if (world == null) world = new WorldState();
        }

        void Start()
        {
            world.Set("following_player", false);
            world.Set("attack_player", false);
            world.Set("player_hostile", false);
            world.Set("pacified", true);
        }

        void OnSocialBlackboardChanged(SocialBlackboard bb)
        {
            world.Set("following_player", bb.followingPlayer);
            world.Set("attack_player", bb.attackPlayer);
            world.Set("player_hostile", bb.playerHostile);
            world.Set("pacified", bb.pacified);
        }

        void Update()
        {
            if (!driveWithBT)
            {
                var following = world.Get("following_player", false);
                var attacking = world.Get("attack_player", false);
                var pacified = world.Get("pacified", true);

                if (attacking && !pacified)
                    movement.TickAttackPlayer();
                else if (following)
                    movement.TickFollowAndHelp();
                else
                {
                }
            }
        }

        public void ReceivePlayerText(string text)
        {
            _ = ReceivePlayerTextAsync(text);
        }

        public async Task<string> ReceivePlayerTextAsync(string text)
        {
            return (intent != null) ? await intent.OnPlayerStatementAsync(text) : null;
        }
    }
}