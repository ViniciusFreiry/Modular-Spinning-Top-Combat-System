using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PawnPhysics : MonoBehaviour
{
    public float pullForce = 10f;
    public float tangentialFactor = 0.2f; // Controla o quanto ele "orbita" o centro
    public Transform arenaCenter;
    public GameObject launchTrailPrefab;
    public GameObject impactEffectPrefab;
    public GameObject impactSlashPrefab;

    public int attack = 50;
    public int defense = 30;
    public int balance = 50;
    public float currentStamina = 100f;
    public float staminaDrainRate = 10f;
    public bool isOutOfStamina = false;
    public float maxStamina = 100f;

    public float staminaDrainBase = 5f; // Base por segundo
    public float maxAngularVelocity = 5000f;

    private Pawn pawn;
    private Rigidbody2D rb;
    private bool stoped = true;
    private float eliminationFreezeDuration = 0.5f;
    private Vector2 collisionPoint = Vector2.zero;

    private bool shielded = false;

    private void Start()
    {
        pawn = GetComponent<Pawn>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (arenaCenter == null || isOutOfStamina) return;

        float spin = GetComponent<Pawn>().GetSpinOrientationInt();

        if (stoped)
        {
            rb.angularVelocity = maxAngularVelocity * spin;
            stoped = false;
        }

        float balanceFactor = Mathf.Clamp01(1 - (balance / (balance + 10f))); // quanto maior o balance, menor o drain
        float drain = staminaDrainBase * balanceFactor * Time.fixedDeltaTime;

        currentStamina -= drain;
        currentStamina = Mathf.Max(currentStamina, 0);

        // Reduz velocidade de rotação proporcional à stamina
        float staminaRatio = Mathf.Max(currentStamina / maxStamina, 0);
        float targetAngularVelocity = maxAngularVelocity * staminaRatio * spin;

        // Aplica rotação suavemente
        rb.angularVelocity = Mathf.MoveTowards(rb.angularVelocity, targetAngularVelocity, 20f);

        if (currentStamina <= 0)
        {
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, 2f), Mathf.Lerp(rb.velocity.y, 0, 2f));
            CheckVitory();
        }

        Vector2 toCenter = (arenaCenter.position - transform.position);
        Vector2 direction = toCenter.normalized;

        // Calcula vetor tangente à direção do centro (perpendicular ao vetor radial)
        Vector2 tangential = new Vector2(-direction.y, direction.x); // 90 graus à esquerda

        // Força combinada escalada pela stamina
        Vector2 combinedForce = (direction + tangential * (tangentialFactor * spin)) * pullForce * staminaRatio;

        rb.AddForce(combinedForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Colisão com outro peão
        PawnPhysics otherPawn = collision.gameObject.GetComponent<PawnPhysics>();
        if (otherPawn != null)
        {
            collisionPoint = collision.GetContact(0).point;

            if (!isOutOfStamina)
            {
                bool isFriend = gameObject.CompareTag(otherPawn.gameObject.tag);

                if (pawn != null)
                {
                    if (otherPawn.isOutOfStamina)
                    {
                        // Força de empurrão em quem já está "morto"
                        Vector2 launchDirection = (otherPawn.transform.position - transform.position).normalized;
                        DeathCoroutine(otherPawn.gameObject, launchDirection);
                        otherPawn.PerformImpactPause(launchDirection, true);
                        FinalSlashEffect(launchDirection);
                    }
                    else
                    {
                        // REPULSÃO
                        Vector2 repelDirection = (transform.position - otherPawn.transform.position).normalized;
                        float impactForce = 500f;
                        float impactScale = (currentStamina + otherPawn.currentStamina) / (2 * maxStamina);

                        rb.AddForce(repelDirection * impactForce * impactScale);

                        // ABILITY
                        if (!isFriend) TriggerAbility(otherPawn);
                    }
                }

                // EFEITO DE IMPACTO
                if (impactEffectPrefab != null)
                {
                    GameObject impact = Instantiate(impactEffectPrefab, collisionPoint, Quaternion.identity);
                    AudioSource audio = impact.GetComponent<AudioSource>();
                    if (audio != null) audio.Play();
                    Destroy(impact, 1f);
                }

                // DANO
                if (!isFriend && otherPawn.currentStamina > 0 && currentStamina > 0) ApplyImpactDamage(otherPawn, false);
            }
        }

        // Colisão com a parede da arena
        if (collision.gameObject.CompareTag("ArenaWall"))
        {
            Vector2 centerDirection = (arenaCenter.position - transform.position).normalized;
            float wallBounceForce = 500f;
            float staminaRatio = currentStamina / maxStamina;

            rb.AddForce(centerDirection * wallBounceForce * staminaRatio);
        }
    }

    public float ApplyImpactDamage(PawnPhysics attacker, bool onlyValue = false)
    {
        float rawDamage = attacker.attack * (1 - (defense / (defense + 10f)));
        float damage = Mathf.Max(rawDamage, 0);

        if (shielded) damage *= 0.5f;
        if (onlyValue) return damage;
        currentStamina = Mathf.Max(currentStamina - damage, 0);

        if (currentStamina <= 0 && !isOutOfStamina)
        {
            Vector2 impactDirection = (transform.position - attacker.transform.position).normalized;
            DeathCoroutine(this.gameObject, impactDirection);
            FinalSlashEffect(impactDirection);

            CheckVitory();
        }

        return damage;
    }

    private void CheckVitory()
    {
        isOutOfStamina = true;
        BattleManager.Instance.CheckVictoryCondition();
    }

    private void DeathCoroutine(GameObject target, Vector2 direction)
    {
        BattleManager.Instance.StopPawnsCoroutine(target, direction);
    }

    public void PerformImpactPause(Vector2 direction, bool launch)
    {
        if (launch) StartCoroutine(LaunchOutWithPause(direction));
        else StartCoroutine(LaunchOutWithPause(direction, false));
    }

    private IEnumerator LaunchOutWithPause(Vector2 direction, bool launch = true)
    {
        Vector2 velocity = rb.velocity;
        float angularVelocity = rb.angularVelocity;
        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.isKinematic = true;
        collider.isTrigger = true;

        yield return new WaitForSeconds(eliminationFreezeDuration);

        rb.isKinematic = false;

        if (!launch)
        {
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
            collider.isTrigger = false;
        } else
        {
            LaunchOut(direction);
        }
    }

    public void LaunchOut(Vector2 direction)
    {
        isOutOfStamina = true;
        rb.velocity = Vector2.zero;

        rb.AddForce(direction.normalized * 2000f);

        // Instancia o efeito de rastro
        if (launchTrailPrefab != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Instantiate(launchTrailPrefab, transform.position, Quaternion.Euler(-angle + 180, 90, 90), transform);
        }
    }

    private void FinalSlashEffect(Vector2 impactDirection)
    {
        if (impactSlashPrefab == null) return;

        float angle = Mathf.Atan2(impactDirection.y, impactDirection.x) * Mathf.Rad2Deg;

        GameObject effect = Instantiate(impactSlashPrefab, collisionPoint, Quaternion.Euler(-angle, 90, 90));
        Destroy(effect, eliminationFreezeDuration);
    }

    public void SetStatus(int attack, int defense, int balance, float stamina)
    {
        this.attack = attack;
        this.defense = defense;
        this.balance = balance;
        this.currentStamina = stamina;
        this.maxStamina = stamina;
    }

    private void TriggerAbility(PawnPhysics target)
    {
        switch (pawn.GetSpecial())
        {
            case SpecialAbility.ExplosionSpin:
                Explode(target);
                break;

            case SpecialAbility.ShieldBurst:
                StartCoroutine(TemporaryShield(2f));
                break;

            case SpecialAbility.StaminaDrain:
                DrainStamina(target);
                break;

            case SpecialAbility.MagneticPull:
                StartCoroutine(MagneticAttraction(target, 2f));
                break;

            case SpecialAbility.None:
            default:
                break;
        }
    }

    private void Explode(PawnPhysics target)
    {
        Vector2 forceDir = (target.transform.position - transform.position).normalized;
        target.rb.AddForce(forceDir * 500f); // Valor ajustável
    }

    private IEnumerator TemporaryShield(float duration)
    {
        shielded = true;
        yield return new WaitForSeconds(duration); // Duração
        shielded = false;
    }

    private void DrainStamina(PawnPhysics target, float drainPercent = 1f)
    {
        float drainAmount = target.ApplyImpactDamage(this, true) * drainPercent;
        if (target.currentStamina > 0)
        {
            float actualDrain = Mathf.Min(drainAmount, target.currentStamina);
            // target.currentStamina -= actualDrain;
            currentStamina = Mathf.Min(currentStamina + actualDrain, maxStamina);
        }
    }

    private IEnumerator MagneticAttraction(PawnPhysics target, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration && target != null)
        {
            Vector2 direction = (transform.position - target.transform.position).normalized;
            target.rb.AddForce(direction * 0.5f); // Ajustável
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}