using UnityEngine;

public class Projetil : MonoBehaviour
{
    public int dano = 10;
    public float velocidade = 15f;
    public float tempoDeVida = 3f;

    void Start()
    {
        // Adiciona um Rigidbody Kinematic invisível para forçar o Unity a detectar a batida
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Move para a direita
        transform.position += Vector3.right * velocidade * Time.deltaTime;

        // Destrói se sair da tela
        if (transform.position.x > 20f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D colisao)
    {
        // Se bater no inimigo comum
        if (colisao.gameObject.CompareTag("Inimigo"))
        {
            InimigoControl script = colisao.gameObject.GetComponent<InimigoControl>();
            if (script != null) script.ReceberDano(dano);
            Destroy(gameObject);
        }
        // Se bater no Boss
        else if (colisao.gameObject.CompareTag("Boss"))
        {
            BossControl scriptBoss = colisao.gameObject.GetComponent<BossControl>();
            if (scriptBoss != null) scriptBoss.ReceberDano(dano);
            Destroy(gameObject);
        }
    }
}