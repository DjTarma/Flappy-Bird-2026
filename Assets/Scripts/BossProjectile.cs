using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [HideInInspector] public int dano = 1;
    [HideInInspector] public float velocidade = 10f;
    public float tempoDeVida = 4f;

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Move para a direção que foi rotacionado (mirado)
        transform.position += transform.right * velocidade * Time.deltaTime;

        // Se sair da tela, se destrói
        if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D colisao)
    {
        // Tiro do Boss só dá dano no Jogador. Os projéteis vão passar sob as criaturas que não tenham a tag de Player.
        if (colisao.gameObject.CompareTag("Player"))
        {
            colisao.gameObject.GetComponent<PlayerControl>().ReceberDano(dano);
            Destroy(gameObject);
        }
    }
}