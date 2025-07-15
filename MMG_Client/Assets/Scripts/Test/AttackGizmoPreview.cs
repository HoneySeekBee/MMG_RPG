using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackGizmoPreview : SceneSingleton<AttackGizmoPreview>
{
    public float attackRange;
    public float attackAngle;
    public float duration = 0.5f;
    public bool isProjectile = false;

    private float timer = 0f;
    private float dirY;
    private Vector3 attackOrigin;

    public void Show(Vector3 pos, float directionY, AttackData attackData)
    {
        isProjectile = attackData.AttackType == AttackPacket.AttackType.Arrow;

        attackOrigin = pos;
        dirY = directionY;
        attackRange = attackData.Range;
        attackAngle = attackData.Angle;
        timer = duration;
    }
    private void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;

        float rad = dirY * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

        if (isProjectile)
        {
            DrawProjectileAttack(forward);
        }
        else
        {
            DrawAttack(forward);
        }
    }
    private void DrawProjectileAttack(Vector3 forward)
    {
        // 투사체 형태: 직선 궤도 시각화
        float projectileRadius = 0.3f;
        Vector3 left = Vector3.Cross(Vector3.up, forward).normalized * projectileRadius;
        Vector3 right = -left;

        Debug.DrawLine(attackOrigin + left, attackOrigin + left + forward * attackRange, Color.yellow);
        Debug.DrawLine(attackOrigin + right, attackOrigin + right + forward * attackRange, Color.yellow);
        Debug.DrawLine(attackOrigin + left + forward * attackRange, attackOrigin + right + forward * attackRange, Color.yellow);
        Debug.DrawLine(attackOrigin + left, attackOrigin + right, Color.yellow);

    }
    private void DrawAttack(Vector3 forward)
    {
        // 근접 공격: 잘린 원뿔
        int segments = 20;
        float innerRadius = 0.3f;

        for (int i = 0; i <= segments; i++)
        {
            float angleStep = -attackAngle / 2f + (attackAngle / segments) * i;
            Quaternion rot = Quaternion.Euler(0, angleStep, 0);
            Vector3 dir = rot * forward;

            Vector3 inner = attackOrigin + dir * innerRadius;
            Vector3 outer = attackOrigin + dir * attackRange;

            Debug.DrawLine(inner, outer, Color.red);
        }

        // 도넛 테두리
        Vector3 leftDir = Quaternion.Euler(0, -attackAngle / 2f, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, attackAngle / 2f, 0) * forward;

        Vector3 innerLeft = attackOrigin + leftDir * innerRadius;
        Vector3 outerLeft = attackOrigin + leftDir * attackRange;
        Vector3 innerRight = attackOrigin + rightDir * innerRadius;
        Vector3 outerRight = attackOrigin + rightDir * attackRange;

        Debug.DrawLine(outerLeft, outerRight, Color.red);
        Debug.DrawLine(innerLeft, innerRight, Color.red);

    }
}
