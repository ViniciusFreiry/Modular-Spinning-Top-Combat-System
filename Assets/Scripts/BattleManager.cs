using System.Collections.Generic;
using UnityEngine;
using static MapProgressManager;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public List<PawnPhysics> player = new List<PawnPhysics>();
    public List<PawnPhysics> enemies = new List<PawnPhysics>();

    public GameObject CustomizerCanvasPrefab;
    public GameObject pawnPrefab;
    public Transform arenaCenter;

    private bool matchEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        List<PawnBuild> pawns = MapProgressManager.Instance.activePawns;
        int pawnSlot = 0;
        Vector3 pawnPosition = Vector3.zero;

        foreach (PawnBuild build in pawns)
        {
            if (build.topPart != null && build.attackPart != null && build.defensePart != null && build.basePart != null)
            {
                switch(pawnSlot)
                {
                    case 0: pawnPosition = new Vector3(0.75f, -3, 0);
                        break;

                    case 1: pawnPosition = new Vector3(2.25f, -2.25f, 0);
                        break;

                    case 2: pawnPosition = new Vector3(3, -0.75f, 0);
                        break;
                }

                // Crie o peão com os dados do build
                GameObject pawn = Instantiate(pawnPrefab, pawnPosition, Quaternion.identity);
                PawnBuilder pawnScript = pawn.GetComponent<PawnBuilder>();
                PawnPhysics pawnPhysics = pawn.GetComponent<PawnPhysics>();

                pawnScript.BuildFromParts(build); // Método que você cria
                player.Add(pawnPhysics);
                pawnPhysics.arenaCenter = this.arenaCenter;
                pawn.tag = "Player";
            }

            pawnSlot++;
        }

        GenerateEnemyPawns();
    }

    public void CheckVictoryCondition()
    {
        if (matchEnded) return;

        CheckElimination(player);
        CheckElimination(enemies);

        if (player.Count <= 0 && enemies.Count > 0)
        {
            EndMatch(enemies);
        }
        else if (enemies.Count <= 0 && player.Count > 0)
        {
            EndMatch(player);
        }
        else if (player.Count <= 0 && enemies.Count <= 0)
        {
            EndMatch(null); // empate
        }
    }

    private void CheckElimination(List<PawnPhysics> pawnList)
    {
        for (int i = 0; i < pawnList.Count; i++)
        {
            if (pawnList[i].isOutOfStamina)
            {
                pawnList.Remove(pawnList[i]);
                i--;
            }
        }
    }

    public void StopPawnsCoroutine(GameObject target, Vector2 direction)
    {
        foreach(PawnPhysics pawn in player)
        {
            pawn.PerformImpactPause(direction, (pawn.gameObject == target ? true : false));
        }

        foreach (PawnPhysics pawn in enemies)
        {
            pawn.PerformImpactPause(direction, (pawn.gameObject == target ? true : false));
        }
    }

    private void EndMatch(List<PawnPhysics> winner)
    {
        matchEnded = true;

        if (winner != null)
        {
            if (winner == player)
            {
                Instantiate(MapProgressManager.Instance.rewardCanvas).GetComponent<RewardManager>().Show((PawnPart selectedPart) => {
                    MapProgressManager.Instance.unlockedParts.Add(selectedPart);
                    Instantiate(CustomizerCanvasPrefab);
                });
                Debug.Log($"🏆 Player Wins");
            }
            else if (winner == enemies)
            {
                Debug.Log($"🏆 Player Loses");
            }
        }
        else
        {
            Debug.Log("🤝 Empate!");
        }
    }

    private void GenerateEnemyPawns()
    {
        int currentLayer = MapProgressManager.Instance.currentLayer;

        int layerIndex = currentLayer;

        int enemyCount = 1;

        if (layerIndex <= 5 && layerIndex > 1)
            enemyCount = 2;
        else if (layerIndex == 0)
            enemyCount = 3;

        for (int i = 0; i < enemyCount; i++)
        {
            PawnPart top = GetRandomPart(PartType.Top);
            PawnPart attack = GetRandomPart(PartType.Attack);
            PawnPart defense = GetRandomPart(PartType.Defense);
            PawnPart basePart = GetRandomPart(PartType.Base);

            PawnBuild build = new PawnBuild
            {
                topPart = top,
                attackPart = attack,
                defensePart = defense,
                basePart = basePart
            };

            Vector3 enemyPos = GetEnemySpawnPosition(i);
            GameObject enemyPawn = Instantiate(pawnPrefab, enemyPos, Quaternion.identity);

            PawnBuilder builder = enemyPawn.GetComponent<PawnBuilder>();
            PawnPhysics physics = enemyPawn.GetComponent<PawnPhysics>();

            builder.BuildFromParts(build);
            physics.arenaCenter = this.arenaCenter;
            enemyPawn.tag = "Enemy";
            enemies.Add(physics);
        }
    }

    private PawnPart GetRandomPart(PartType type)
    {
        List<PawnPart> pool = MapProgressManager.Instance.allParts.FindAll(p => p.partType == type && p.rarity == Rarity.Common);
        if (pool.Count == 0) return null;

        return pool[Random.Range(0, pool.Count)];
    }

    private Vector3 GetEnemySpawnPosition(int slot)
    {
        switch (slot)
        {
            case 0: return new Vector3(-0.75f, 3f, 0);
            case 1: return new Vector3(-2.25f, 2.25f, 0);
            case 2: return new Vector3(-3f, 0.75f, 0);
            default: return Vector3.zero;
        }
    }
}