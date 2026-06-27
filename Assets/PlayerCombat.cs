using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 7f;
    public float attackDamage = 1f; // THE FIX: This was missing!

    [Header("Interaction Settings")]
    public float interactionRange = 5f;

    [Header("Physics Settings")]
    public LayerMask ignoreLayers;

    void Update()
    {
        // 🖱️ Left Click to Attack
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformAttack();
        }

        // ⌨️ E Key to Interact with Tower
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CheckForInteraction();
        }
    }

    void PerformAttack()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Visual debug line (Visible in Scene view)
        Debug.DrawRay(ray.origin, ray.direction * attackRange, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, attackRange, ~ignoreLayers))
        {
            // Check if we hit an enemy
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log("Dealt " + attackDamage + " damage to " + hit.collider.gameObject.name);
            }
        }
    }

    void CheckForInteraction()
    {
        // Shoot the ray from the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // The ~ignoreLayers tells the ray to ignore the player's own body
        if (Physics.Raycast(ray, out hit, interactionRange, ~ignoreLayers))
        {
            Tower tower = hit.collider.GetComponent<Tower>();

            // If it didn't find the script on the object hit, check the parent
            if (tower == null) tower = hit.collider.GetComponentInParent<Tower>();

            if (tower != null)
            {
                tower.Interact();
                Debug.Log("Hit Tower: " + hit.collider.gameObject.name);
            }
            else
            {
                Debug.Log("Raycast hit: " + hit.collider.gameObject.name + " (Not a Tower)");
            }
        }
    }
}